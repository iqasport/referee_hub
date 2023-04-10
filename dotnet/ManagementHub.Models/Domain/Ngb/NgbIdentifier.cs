using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.Ngb;

/// <summary>
/// Identifier of a National Governing Body.
/// In the future it will ensure that it is initialized with the id in the correct format.
/// </summary>
public record struct NgbIdentifier(long Id) : IIdentifiable
{
}
