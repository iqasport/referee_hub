using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to manage teams, number of players, etc. associated with that Ngb.
/// </summary>
public record NgbStatsManagerRole(NgbConstraint Ngb) : IUserRole
{
}
