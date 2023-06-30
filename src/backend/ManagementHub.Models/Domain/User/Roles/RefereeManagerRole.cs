using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to manage referees that match the NgbConstraint.
/// Allows disactivating referees, approve verification requests.
/// </summary>
public record RefereeManagerRole(NgbConstraint Ngb) : IUserRole
{
}
