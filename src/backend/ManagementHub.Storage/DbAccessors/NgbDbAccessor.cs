using System.Linq;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Storage.Extensions;

namespace ManagementHub.Storage.DbAccessors;
public class NgbDbAccessor : IDbAccessor<NgbIdentifier>
{
	private readonly ManagementHubDbContext dbContext;

	public NgbDbAccessor(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public IQueryable<IIdentifiable> SelectWithId(NgbIdentifier identifier)
	{
		return this.dbContext.NationalGoverningBodies.WithIdentifier(identifier);
	}
}
