using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IContextProvider
{
    /// <summary>
    /// Gets a user context instance for the specified <paramref name="userId"/>.
    /// </summary>
    Task<IUserContext> GetUserContext(UserIdentifier userId);
}
