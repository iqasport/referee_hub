using System;

namespace ManagementHub.Models.Data;

/// <summary>
/// Tracks an NGB's approval or rejection of a player transfer.
/// One record is created per NGB involved in a transfer (origin NGB and/or destination NGB).
/// </summary>
public partial class NgbTransferApproval
{
	public long Id { get; set; }
	public long TeamInvitationId { get; set; }
	public long NgbId { get; set; }

	/// <summary>
	/// True when this record belongs to the NGB of the origin (source) team.
	/// False when it belongs to the destination team's NGB.
	/// </summary>
	public bool IsOriginNgb { get; set; }

	public DateTime? ApprovedAt { get; set; }
	public DateTime? RejectedAt { get; set; }
	public long? ReviewedByUserId { get; set; }
	public DateTime CreatedAt { get; set; }

	public virtual TeamInvitation TeamInvitation { get; set; } = null!;
	public virtual NationalGoverningBody Ngb { get; set; } = null!;
	public virtual User? ReviewedByUser { get; set; }
}
