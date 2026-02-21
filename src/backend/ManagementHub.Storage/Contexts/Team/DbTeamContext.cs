using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
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

	public IQueryable<Models.Data.Team> QueryTeamsInternal(NgbConstraint ngbs, TeamGroupAffiliation? groupAffiliation = null)
	{
		IQueryable<Models.Data.Team> t = this.dbContext.Teams
				.Include(t => t.NationalGoverningBody);

		if (!ngbs.AppliesToAny)
		{
			t = t.Join(this.dbContext.NationalGoverningBodies.WithConstraint(ngbs), tt => tt.NationalGoverningBodyId, n => n.Id, (tt, n) => tt);
		}

		if (groupAffiliation.HasValue)
		{
			t = t.Where(x => x.GroupAffiliation == groupAffiliation.Value);
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

	public IQueryable<ITeamContext> QueryTeams(NgbConstraint ngbs, TeamGroupAffiliation? groupAffiliation = null) => this.QueryTeamsInternal(ngbs, groupAffiliation).AsNoTracking().Select(Selector);

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
			LogoUrl = teamData.LogoUrl,
			Description = teamData.Description,
			ContactEmail = teamData.ContactEmail,
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
		team.LogoUrl = teamData.LogoUrl;
		team.Description = teamData.Description;
		team.ContactEmail = teamData.ContactEmail;
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

	public IQueryable<TeamMemberInfo> QueryTeamMembers(TeamIdentifier teamId, NgbConstraint ngbs)
	{
		// Start with RefereeTeams and join with Teams to validate NGB constraint
		var teamsQuery = this.dbContext.Teams.AsQueryable();

		if (!ngbs.AppliesToAny)
		{
			teamsQuery = teamsQuery.Join(
				this.dbContext.NationalGoverningBodies.WithConstraint(ngbs),
				t => t.NationalGoverningBodyId,
				n => n.Id,
				(t, n) => t);
		}

		var query = this.dbContext.RefereeTeams
			.Where(rt => rt.TeamId == teamId.Id)
			.Where(rt => rt.RefereeId != null)
			.Join(
				teamsQuery,
				rt => rt.TeamId,
				t => t.Id,
				(rt, t) => rt);

		// Join with Users
		var usersQuery = query.Join(
			this.dbContext.Users,
			rt => rt.RefereeId,
			u => u.Id,
			(rt, u) => u);

		// Apply filtering BEFORE projection
		var filter = this.filteringContext.FilteringParameters.Filter;
		if (!string.IsNullOrEmpty(filter))
		{
			filter = $"%{filter}%";
			if (this.dbContext.Database.IsNpgsql())
			{
				usersQuery = usersQuery.Where(u => EF.Functions.ILike(u.FirstName + " " + u.LastName, filter));
			}
			else
			{
				usersQuery = usersQuery.Where(u => EF.Functions.Like(u.FirstName + " " + u.LastName, filter));
			}
		}

		// Distinct users
		usersQuery = usersQuery.Distinct();

		// Set total count for pagination metadata
		if (this.filteringContext.FilteringMetadata != null)
		{
			this.filteringContext.FilteringMetadata.TotalCount = usersQuery.Count();
		}

		// Apply pagination
		usersQuery = usersQuery.Page(this.filteringContext.FilteringParameters);

		// Now project to TeamMemberInfo with primary team information
		// Join with RefereeTeams to get the user's playing team
		return usersQuery
			.GroupJoin(
				this.dbContext.RefereeTeams
					.Where(rt => rt.AssociationType == ManagementHub.Models.Enums.RefereeTeamAssociationType.Player)
					.Join(
						this.dbContext.Teams,
						rt => rt.TeamId,
						t => t.Id,
						(rt, t) => new { rt.RefereeId, TeamId = t.Id, TeamName = t.Name }),
				u => u.Id,
				rt => rt.RefereeId,
				(u, primaryTeams) => new { User = u, PrimaryTeam = primaryTeams.FirstOrDefault() })
			.Select(x => new TeamMemberInfo
			{
				UserId = x.User.UniqueId != null
					? UserIdentifier.Parse(x.User.UniqueId)
					: UserIdentifier.FromLegacyUserId(x.User.Id),
				Name = $"{x.User.FirstName} {x.User.LastName}",
				PrimaryTeamName = x.PrimaryTeam != null ? x.PrimaryTeam.TeamName : null,
				PrimaryTeamId = x.PrimaryTeam != null ? new TeamIdentifier(x.PrimaryTeam.TeamId) : null
			});
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
		LogoUrl = tt.LogoUrl,
		Description = tt.Description,
		ContactEmail = tt.ContactEmail,
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
		LogoUrl = tt.LogoUrl,
		Description = tt.Description,
		ContactEmail = tt.ContactEmail,
	});

	public async Task<ITeamContext?> GetTeamAsync(TeamIdentifier teamId, NgbConstraint ngbs)
	{
		return await this.QueryTeamsInternal(ngbs)
			.Where(t => t.Id == teamId.Id)
			.Select(Selector)
			.FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId, NgbConstraint ngbs)
	{
		var teamDbId = teamId.Id;

		// Build Teams query with NGB constraint
		var teamsQuery = this.dbContext.Teams.AsQueryable();

		if (!ngbs.AppliesToAny)
		{
			teamsQuery = teamsQuery.Join(
				this.dbContext.NationalGoverningBodies.WithConstraint(ngbs),
				t => t.NationalGoverningBodyId,
				n => n.Id,
				(t, n) => t);
		}

		// Query TeamManagers, join with Teams to validate NGB, then join with Users to get manager info
		var managers = await this.dbContext.TeamManagers
			.Where(tm => tm.TeamId == teamDbId)
			.Join(
				teamsQuery,
				tm => tm.TeamId,
				t => t.Id,
				(tm, t) => tm)
			.Join(
				this.dbContext.Users,
				tm => tm.UserId,
				u => u.Id,
				(tm, u) => new ManagerInfo
				{
					UserId = u.UniqueId != null
						? UserIdentifier.Parse(u.UniqueId)
						: UserIdentifier.FromLegacyUserId(u.Id),
					Name = $"{u.FirstName} {u.LastName}",
					Email = u.Email
				})
			.ToListAsync();

		return managers;
	}
}
