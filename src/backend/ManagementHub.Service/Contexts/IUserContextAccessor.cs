using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;
public interface IUserContextAccessor
{
	Task<IUserContext> GetCurrentUserContextAsync();

	Task<IUserContext> GetUserContextAsync(UserIdentifier userId);

	Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId);

	Task<IUserAvatarContext> GetUserAvatarContextAsync(UserIdentifier userId);

	Task<UserAttributes> GetUserAttributesAsync(UserIdentifier userId);
}
