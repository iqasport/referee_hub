using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// The current approval state of a player transfer request.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TransferApprovalStatus
{
	/// <summary>
	/// No transfer required (player has no previous team — first-time join).
	/// Standard team approval flow applies.
	/// </summary>
	[EnumMember(Value = "notATransfer")]
	NotATransfer,

	/// <summary>
	/// Waiting for one or more NGBs to review the transfer.
	/// The team manager cannot act until NGB approval is granted.
	/// </summary>
	[EnumMember(Value = "pendingNgbApproval")]
	PendingNgbApproval,

	/// <summary>
	/// All required NGB approvals have been granted.
	/// The team manager can now accept or decline the player.
	/// </summary>
	[EnumMember(Value = "pendingTeamApproval")]
	PendingTeamApproval,

	/// <summary>
	/// Transfer was rejected by an NGB. The invitation has been revoked.
	/// </summary>
	[EnumMember(Value = "rejectedByNgb")]
	RejectedByNgb,

	/// <summary>
	/// Transfer was accepted by the team manager. The player has joined the team.
	/// </summary>
	[EnumMember(Value = "approved")]
	Approved,

	/// <summary>
	/// Transfer was declined or revoked without NGB rejection.
	/// </summary>
	[EnumMember(Value = "declined")]
	Declined,
}
