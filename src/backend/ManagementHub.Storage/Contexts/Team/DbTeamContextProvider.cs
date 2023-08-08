using System.Linq;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Storage.Collections;

namespace ManagementHub.Storage.Contexts.Team;

public class DbTeamContextProvider : ITeamContextProvider
{
	private readonly DbTeamContextFactory dbTeamContextFactory;
	
	public DbTeamContextProvider(ManagementHubDbContext dbContext, CollectionFilteringContext filteringContext)
	{
		this.dbTeamContextFactory = new DbTeamContextFactory(
			dbContext,
			filteringContext
		);
	}

	public IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs)
	{
		return this.dbTeamContextFactory.QueryTeams(ngbs);
	}
}
