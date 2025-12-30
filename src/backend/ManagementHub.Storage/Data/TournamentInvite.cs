using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class TournamentInvite : IIdentifiable
{
	public long Id { get; set; }
	public long TournamentId { get; set; }
	public string ParticipantType { get; set; } = null!;
	public string ParticipantId { get; set; } = null!;
	public long InitiatorUserId { get; set; }
	public DateTime CreatedAt { get; set; }
	public ApprovalStatus TournamentManagerApproval { get; set; }
	public DateTime? TournamentManagerApprovalDate { get; set; }
	public ApprovalStatus ParticipantApproval { get; set; }
	public DateTime? ParticipantApprovalDate { get; set; }

	public virtual Tournament Tournament { get; set; } = null!;
	public virtual User Initiator { get; set; } = null!;
}
