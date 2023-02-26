using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IContextProvider
{
    /// <summary>
    /// Gets a user context instance for the specified <paramref name="userId"/>.
    /// </summary>
    Task<IUserContext> GetUserContextAsync(UserIdentifier userId, CancellationToken cancellationToken);
}
