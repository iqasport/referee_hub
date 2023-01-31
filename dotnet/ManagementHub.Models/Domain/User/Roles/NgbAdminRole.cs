using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to manage everything associated with that Ngb.
/// </summary>
public class NgbAdminRole : IUserRole
{
    public NgbAdminRole(NgbConstraint ngb) => this.Ngb = ngb;
    public NgbConstraint Ngb { get; set; }
}
