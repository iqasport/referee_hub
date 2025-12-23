using System.Linq;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Extensions;

namespace ManagementHub.Storage.DbAccessors;

public class UserDbAccessor : IDbAccessor<UserIdentifier>
{
	private readonly ManagementHubDbContext dbContext;

	public UserDbAccessor(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public IQueryable<IIdentifiable> SelectWithId(UserIdentifier identifier)
	{
		return this.dbContext.Users.WithIdentifier(identifier);
	}
}
