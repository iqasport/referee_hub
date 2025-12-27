using System.Linq;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Storage.DbAccessors;

public class TournamentDbAccessor : IDbAccessor<TournamentIdentifier>
{
	private readonly ManagementHubDbContext dbContext;

	public TournamentDbAccessor(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public IQueryable<IIdentifiable> SelectWithId(TournamentIdentifier identifier)
	{
		return this.dbContext.Tournaments
			.Where(t => t.UniqueId == identifier.ToString());
	}
}
