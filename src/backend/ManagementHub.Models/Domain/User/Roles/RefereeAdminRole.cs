using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows the admin to modify certifications, hr payments, user details, referee details.
/// </summary>
public record RefereeAdminRole : IUserRole
{
}
