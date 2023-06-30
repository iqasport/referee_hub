using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows the admin to manage tests.
/// </summary>
public record TestAdminRole : IUserRole
{
}
