using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role for technical administrators to see additional diagnostics.
/// </summary>
public record TechAdminRole : INgbUserRole
{
	public NgbConstraint Ngb => NgbConstraint.Any;
}
