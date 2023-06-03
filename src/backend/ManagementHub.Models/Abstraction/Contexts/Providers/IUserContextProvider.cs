using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface IUserContextProvider
{
	/// <summary>
	/// Gets a user context instance for the specified <paramref name="userId"/>.
	/// </summary>
	Task<IUserContext> GetUserContextAsync(UserIdentifier userId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a user data context instance for the specified <paramref name="userId"/>.
	/// </summary>
	Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a user avatar context instance for the specified <paramref name="userId"/>.
	/// </summary>
	Task<IUserAvatarContext> GetUserAvatarContextAsync(UserIdentifier userId, CancellationToken cancellationToken);
}
