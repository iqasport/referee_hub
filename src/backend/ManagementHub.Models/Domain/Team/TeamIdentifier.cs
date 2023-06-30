using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.Team;

/// <summary>
/// Identifier of a National Governing Body.
/// In the future it will ensure that it is initialized with the id in the correct format.
/// </summary>
public record struct TeamIdentifier(long Id) : IIdentifiable
{
}
