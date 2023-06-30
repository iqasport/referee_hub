namespace ManagementHub.Models.Domain.User;

/// <summary>
/// A BCrypt hash of the password (with the algorithm version and salt).
/// The string is wrapped in this struct to ensure it cannot be mixed up with any other string by accident.
/// </summary>
public record struct UserPassword(string PasswordHash)
{
}
