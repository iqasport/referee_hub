namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// View model for a team invitation.
/// </summary>
public class TeamInvitationViewModel
{
	/// <summary>
	/// Invitation identifier.
	/// </summary>
	public required string InvitationId { get; set; }

	/// <summary>
	/// Email address of the invitee.
	/// </summary>
	public required string Email { get; set; }

	/// <summary>
	/// Date when invitation was created.
	/// </summary>
	public required DateTime CreatedAt { get; set; }

	/// <summary>
	/// Name of the person who sent the invitation (if available).
	/// </summary>
	public string? InvitedByName { get; set; }
}
