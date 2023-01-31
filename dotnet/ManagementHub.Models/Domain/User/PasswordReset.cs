using System;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Details of password reset status.
/// </summary>
public class PasswordReset
{
    /// <summary>
    /// High entropy password reset token.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Timestamp when the password reset was requested.
    /// </summary>
    public DateTime? EmailSentAt { get; set; }
}