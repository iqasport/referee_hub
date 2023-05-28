using System;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Data related to confirming user's identity.
/// </summary>
public class UserConfirmation
{
	/// <summary>
	/// High entropy token to be used in the link for email confirmation.
	/// </summary>
	public string? EmailConfirmationToken { get; set; }

	/// <summary>
	/// Timestamp when confirmation email was sent to the user. 
	/// </summary>
	public DateTime? EmailSentAt { get; set; }

	/// <summary>
	/// Timestamp when user navigated to the confirmation link.
	/// </summary>
	public DateTime? EmailConfirmedAt { get; set; }
}
