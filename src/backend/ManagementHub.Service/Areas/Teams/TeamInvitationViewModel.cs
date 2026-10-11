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

	/// <summary>
	/// True when this pending item is a player join request awaiting manager decision.
	/// </summary>
	public bool RequiresManagerDecision { get; set; }

	/// <summary>
	/// Current transfer approval status. Null when this is not a transfer (first-time join).
	/// </summary>
	public TransferApprovalStatus? TransferStatus { get; set; }

	/// <summary>
	/// Name of the team the player is transferring from. Null for first-time joins.
	/// </summary>
	public string? OriginTeamName { get; set; }

	/// <summary>
	/// Whether this is an internal transfer (same NGB) or international.
	/// </summary>
	public bool? IsInternalTransfer { get; set; }
}
