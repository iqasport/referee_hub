using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain;
using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts;

public class DbUserContext : IUserContext
{
    public DbUserContext(
        IQueryable<User> usersCollection,
        ILogger<DbUserContext> logger,
        UserIdentifier userId,
        GetUserContextOptions loadOptions)
    {
        
    }
    
	public UserIdentifier UserId => throw new NotImplementedException();

	public UserData UserData => throw new NotImplementedException();

	public ModificationTimestamp ModificationTimestamp => throw new NotImplementedException();

	public UserPassword Password => throw new NotImplementedException();

	public SignInStatistics SignInStatistics => throw new NotImplementedException();

	public UserConfirmation Confirmation => throw new NotImplementedException();

	public Invitation Invitation => throw new NotImplementedException();
}
