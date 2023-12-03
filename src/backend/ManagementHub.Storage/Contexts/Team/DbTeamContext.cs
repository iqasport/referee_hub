using System;
using System.Linq;
using System.Linq.Expressions;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
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

	public IQueryable<ITeamContext> QueryTeams(NgbConstraint ngbs)
	{
		IQueryable<Models.Data.Team> t = this.dbContext.Teams.AsNoTracking()
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

		return t.Select(Selector);
	}

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
