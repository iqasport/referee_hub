using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to view referees that match the NgbConstraint.
/// </summary>
public class RefereeViewerRole : IUserRole
{
    public RefereeViewerRole(NgbConstraint ngb) => this.Ngb = ngb;
    public NgbConstraint Ngb { get; set; }
}
