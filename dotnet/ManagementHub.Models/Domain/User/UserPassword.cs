namespace ManagementHub.Models.Domain.User;

public class UserPassword
{
    public UserPassword(string passwordHash)
    {
        this.PasswordHash = passwordHash;
    }
    
    /// <summary>
    /// A BCrypt hash of the password (with the algorithm version and salt).
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Details of the password reset operation.
    /// </summary>
    public PasswordReset? PasswordReset { get; set;} 
}