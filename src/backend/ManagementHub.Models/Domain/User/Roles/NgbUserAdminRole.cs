using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to manage non-referee users who hold roles associated with that Ngb.
/// Allows modifying roles of these user, invite users to a specific role.
/// </summary>
public record NgbUserAdminRole(NgbConstraint Ngb) : INgbUserRole
{
}
