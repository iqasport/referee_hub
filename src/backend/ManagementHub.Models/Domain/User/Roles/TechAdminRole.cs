using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role for technical administrators to see additional diagnostics.
/// </summary>
public record TechAdminRole : IUserRole
{
}
