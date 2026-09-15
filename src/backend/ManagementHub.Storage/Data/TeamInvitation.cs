using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class TeamInvitation : IIdentifiable
{
	public long Id { get; set; }
	public long TeamId { get; set; }
	public string Email { get; set; } = null!;
	public long InitiatorUserId { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? RevokedAt { get; set; }
	public DateTime? AcceptedAt { get; set; }
	public DateTime? DeclinedAt { get; set; }
	public long? RespondedByUserId { get; set; }

	/// <summary>
	/// The team the player was on (or most recently left) when this invite was created.
	/// Null for first-time team joins.
	/// </summary>
	public long? OriginTeamId { get; set; }

	/// <summary>
	/// True when the origin and destination teams belong to the same NGB.
	/// Null when there is no origin team (first-time join — no NGB approval required).
	/// </summary>
	public bool? IsInternalTransfer { get; set; }

	public virtual Team Team { get; set; } = null!;
	public virtual Team? OriginTeam { get; set; }
	public virtual User Initiator { get; set; } = null!;
	public virtual User? RespondedByUser { get; set; }
	public virtual ICollection<NgbTransferApproval> NgbTransferApprovals { get; set; } = new HashSet<NgbTransferApproval>();
}