using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.User;

public class Invitation
{
    /// <summary>
    /// High entropy token to be used in the link for the invitation email.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Timestamp when invitation has been created.
    /// </summary>
    public DateTime? CreateAt { get; set; }

    /// <summary>
    /// Timestamp when invitation has been sent.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Timestamp when invitation has been accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Id of the user who invited this user.
    /// </summary>
    public IIdentifiable? InvitedBy { get; set; }
}
