using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IUserAuthenticationContext
{
    UserIdentifier UserId { get; }

    UserPassword UserPassword { get; }
}
