using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

/// <summary>
/// Context in which we're interested in learning user data about a given user.
/// </summary>
public interface IUserDataContext
{
	UserIdentifier UserId { get; }

	ExtendedUserData ExtendedUserData { get; }
}
