using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role for IQA admins.
/// </summary>
public record IqaAdminRole : INgbUserRole
{
	public NgbConstraint Ngb => NgbConstraint.Any;
}
