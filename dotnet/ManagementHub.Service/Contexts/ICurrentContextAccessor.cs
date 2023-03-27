using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;
public interface ICurrentContextAccessor
{
	Task<IUserContext> GetCurrentUserContextAsync();

	Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId);

	Task<IUserAvatarContext> GetUserAvatarContextAsync(UserIdentifier userId);
}
