using System.Linq;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Storage.Extensions;

namespace ManagementHub.Storage.DbAccessors;

public class TeamDbAccessor : IDbAccessor<TeamIdentifier>
{
	private readonly ManagementHubDbContext dbContext;

	public TeamDbAccessor(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public IQueryable<IIdentifiable> SelectWithId(TeamIdentifier identifier)
	{
		return this.dbContext.Teams.WithIdentifier(identifier);
	}
}
