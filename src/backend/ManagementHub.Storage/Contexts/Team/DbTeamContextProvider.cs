using System.Linq;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Storage.Contexts.Team;

public class DbTeamContextProvider : ITeamContextProvider
{
	private readonly DbTeamContextFactory dbTeamContextFactory;
	
	public DbTeamContextProvider(ManagementHubDbContext dbContext)
	{
		this.dbTeamContextFactory = new DbTeamContextFactory(
			dbContext.Teams,
			dbContext.NationalGoverningBodies
		);
	}

	public IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs)
	{
		return this.dbTeamContextFactory.QueryTeams(ngbs);
	}
}
