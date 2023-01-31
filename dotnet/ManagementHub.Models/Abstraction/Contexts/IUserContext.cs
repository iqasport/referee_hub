using ManagementHub.Models.Domain;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IUserContext
{
    /// <summary>
    /// Identifier of the user.
    /// </summary>
    UserIdentifier UserId { get; }

    /// <summary>
    /// Email of the user.
    /// </summary>
    string Email { get; }

    string FirstName { get; }

    string LastName { get; }

    string Bio { get; }

    string Pronouns { get; }

    bool ShowPronouns { get; }

    ModificationTimestamp ModificationTimestamp { get; }

    UserPassword Password { get; }

    SignInStatistics SignInStatistics { get; }

    UserConfirmation Confirmation { get; }

    Invitation Invitation { get; }
}
