using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to view statistics associated with that Ngb.
/// </summary>
public record NgbStatsViewerRole(NgbConstraint Ngb) : INgbUserRole
{
}
