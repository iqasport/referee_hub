using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Identifier of a user.
/// In the future it will ensure that it is initialized with the id in the correct format.
/// </summary>
public record struct UserIdentifier(long Id) : IIdentifiable
{
}