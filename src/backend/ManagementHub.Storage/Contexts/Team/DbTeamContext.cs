using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Contexts.Team;

public record DbTeamContext(TeamIdentifier TeamId, NgbIdentifier NgbId, TeamData TeamData) : ITeamContext;

public class DbTeamContextFactory
{
	private readonly ManagementHubDbContext dbContext;
	private readonly CollectionFilteringContext filteringContext;

	public DbTeamContextFactory(ManagementHubDbContext dbContext, CollectionFilteringContext filteringContext)
	{
		this.dbContext = dbContext;
		this.filteringContext = filteringContext;
	}

	public IQueryable<Models.Data.Team> QueryTeamsInternal(NgbConstraint ngbs)
	{
		IQueryable<Models.Data.Team> t = this.dbContext.Teams
				.Include(t => t.NationalGoverningBody);

		if (!ngbs.AppliesToAny)
		{
			t = t.Join(this.dbContext.NationalGoverningBodies.WithConstraint(ngbs), tt => tt.NationalGoverningBodyId, n => n.Id, (tt, n) => tt);
		}

		t = t.OrderBy(x => x.Name);

		var filter = this.filteringContext.FilteringParameters.Filter;
		filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";
		if (!string.IsNullOrEmpty(filter))
		{
			if (this.dbContext.Database.IsNpgsql())
			{
				t = t.Where(x =>
					EF.Functions.ILike(x.Name, filter) ||
					EF.Functions.ILike(x.City, filter) ||
					EF.Functions.ILike(x.State!, filter));
			}
			else
			{
				t = t.Where(x =>
					EF.Functions.Like(x.Name, filter) ||
					EF.Functions.Like(x.City, filter) ||
					EF.Functions.Like(x.State!, filter));
			}
		}

		if (this.filteringContext.FilteringMetadata != null)
		{
			//TODO: figure out some way to make this async in a nice way
			this.filteringContext.FilteringMetadata.TotalCount = t.Count();
		}

		t = t.Page(this.filteringContext.FilteringParameters);

		return t;
	}

	public IQueryable<ITeamContext> QueryTeams(NgbConstraint ngbs) => this.QueryTeamsInternal(ngbs).AsNoTracking().Select(Selector);

	public async Task<ITeamContext> CreateTeamAsync(NgbIdentifier ngb, TeamData teamData)
	{
		var ngbDbId = await this.dbContext.NationalGoverningBodies.WithIdentifier(ngb).Select(n => n.Id).SingleOrDefaultAsync();
		if (ngbDbId == default)
		{
			throw new ArgumentException($"NGB {ngb} not found", nameof(ngb));
		}

		var team = new Models.Data.Team
		{
			NationalGoverningBodyId = ngbDbId,
			Name = teamData.Name,
			City = teamData.City,
			State = teamData.State,
			Country = teamData.Country,
			GroupAffiliation = teamData.GroupAffiliation,
			Status = teamData.Status,
			JoinedAt = teamData.JoinedAt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		};

		this.dbContext.Teams.Add(team);
		var written = await this.dbContext.SaveChangesAsync();
		if (written == 0)
		{
			throw new InvalidOperationException("Failed to create team");
		}

		Debug.Assert(team.Id != default);
		return FromDatabase(team, ngb);
	}

	public async Task<ITeamContext> UpdateTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId, TeamData teamData)
	{
		using var transaction = await this.dbContext.Database.BeginTransactionAsync();
		var team = this.QueryTeamsInternal(NgbConstraint.Single(ngb)).Where(t => t.Id == teamId.Id).SingleOrDefault();
		if (team == null)
		{
			throw new ArgumentException($"Team {teamId} was not found in NGB {ngb}");
		}

		var previousStatus = team.Status!.Value;

		team.Name = teamData.Name;
		team.City = teamData.City;
		team.State = teamData.State;
		team.Country = teamData.Country;
		team.GroupAffiliation = teamData.GroupAffiliation;
		team.Status = teamData.Status;
		team.JoinedAt = teamData.JoinedAt;
		team.UpdatedAt = DateTime.UtcNow;

		if (previousStatus != teamData.Status)
		{
			team.TeamStatusChangesets.Add(new Models.Data.TeamStatusChangeset
			{
				TeamId = team.Id,
				PreviousStatus = previousStatus.ToString(),
				NewStatus = teamData.Status.ToString(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			});
		}

		await this.dbContext.SaveChangesAsync();
		await transaction.CommitAsync();

		return FromDatabase(team, ngb);
	}

	public async Task DeleteTeamAsync(TeamIdentifier teamId)
	{
		using var transaction = await this.dbContext.Database.BeginTransactionAsync();

		await this.dbContext.RefereeTeams.Where(rt => rt.TeamId == teamId.Id).ExecuteDeleteAsync();

		await this.dbContext.TeamStatusChangesets.Where(tsc => tsc.TeamId == teamId.Id).ExecuteDeleteAsync();

		var deleted = await this.dbContext.Teams.Where(t => t.Id == teamId.Id).ExecuteDeleteAsync();

		Debug.Assert(deleted == 1);
		await transaction.CommitAsync();
	}

	public async Task<ITeamContext?> GetTeamAsync(TeamIdentifier teamId)
	{
		var team = await this.dbContext.Teams
			.Include(t => t.NationalGoverningBody)
			.Where(t => t.Id == teamId.Id)
			.AsNoTracking()
			.FirstOrDefaultAsync();

		if (team == null)
		{
			return null;
		}

		var ngbId = new NgbIdentifier(team.NationalGoverningBody!.CountryCode);
		return FromDatabase(team, ngbId);
	}

	public IQueryable<TeamMemberInfo> QueryTeamMembers(TeamIdentifier teamId)
	{
		// Start with RefereeTeams and filter first
		var query = this.dbContext.RefereeTeams
			.Where(rt => rt.TeamId == teamId.Id)
			.Where(rt => rt.RefereeId != null);

		// Join with Users
		var usersQuery = query.Join(
			this.dbContext.Users,
			rt => rt.RefereeId,
			u => u.Id,
			(rt, u) => u);

		// Apply filtering BEFORE projection
		var filter = this.filteringContext.FilteringParameters.Filter;
		filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";
		if (!string.IsNullOrEmpty(filter))
		{
			if (this.dbContext.Database.IsNpgsql())
			{
				usersQuery = usersQuery.Where(u => EF.Functions.ILike($"{u.FirstName} {u.LastName}", filter));
			}
			else
			{
				usersQuery = usersQuery.Where(u => EF.Functions.Like($"{u.FirstName} {u.LastName}", filter));
			}
		}

		// Now project to TeamMemberInfo
		return usersQuery
			.Select(u => new TeamMemberInfo
			{
				UserId = u.UniqueId != null
					? UserIdentifier.Parse(u.UniqueId)
					: UserIdentifier.FromLegacyUserId(u.Id),
				Name = $"{u.FirstName} {u.LastName}"
			})
			.Distinct();
	}

	public static DbTeamContext FromDatabase(Models.Data.Team tt, NgbIdentifier ngb) => new DbTeamContext(new TeamIdentifier(tt.Id), ngb, new TeamData
	{
		Name = tt.Name,
		City = tt.City,
		State = tt.State,
		Country = tt.Country,
		GroupAffiliation = tt.GroupAffiliation!.Value,
		Status = tt.Status!.Value,
		JoinedAt = tt.JoinedAt ?? new DateTime(),
	});

	public static Expression<Func<Models.Data.Team, DbTeamContext>> Selector = tt => new DbTeamContext(new TeamIdentifier(tt.Id), new NgbIdentifier(tt.NationalGoverningBody!.CountryCode), new TeamData
	{
		Name = tt.Name,
		City = tt.City,
		State = tt.State,
		Country = tt.Country,
		GroupAffiliation = tt.GroupAffiliation!.Value,
		Status = tt.Status!.Value,
		JoinedAt = tt.JoinedAt ?? new DateTime(),
	});
}
