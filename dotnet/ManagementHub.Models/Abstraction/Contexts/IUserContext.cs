using ManagementHub.Models.Domain;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IUserContext
{
    UserIdentifier UserId { get; }

    UserData UserData { get; }

    ModificationTimestamp ModificationTimestamp { get; }

    UserPassword Password { get; }

    SignInStatistics SignInStatistics { get; }

    UserConfirmation Confirmation { get; }

    Invitation Invitation { get; }
}
