namespace ManagementHub.Models.Domain.User;

/// <summary>
/// A BCrypt hash of the password (with the algorithm version and salt).
/// </summary>
public record UserPassword(string PasswordHash)
{
}
