using System;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tournament;

public class InviteInfo
{
	public required TournamentIdentifier TournamentId { get; set; }
	public required ParticipantType ParticipantType { get; set; }
	public required string ParticipantId { get; set; }
	public required string ParticipantName { get; set; }
	public required UserIdentifier InitiatorUserId { get; set; }
	public required DateTime CreatedAt { get; set; }
	public required ApprovalStatus TournamentManagerApproval { get; set; }
	public DateTime? TournamentManagerApprovalDate { get; set; }
	public required ApprovalStatus ParticipantApproval { get; set; }
	public DateTime? ParticipantApprovalDate { get; set; }

	public InviteStatus GetStatus()
	{
		if (TournamentManagerApproval == ApprovalStatus.Rejected || ParticipantApproval == ApprovalStatus.Rejected)
			return InviteStatus.Rejected;

		if (TournamentManagerApproval == ApprovalStatus.Approved && ParticipantApproval == ApprovalStatus.Approved)
			return InviteStatus.Approved;

		return InviteStatus.Pending;
	}
}

