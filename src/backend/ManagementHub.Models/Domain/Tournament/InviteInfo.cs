using System;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tournament;

public class InviteInfo
{
	public required long Id { get; set; }
	public required TournamentIdentifier TournamentId { get; set; }
	public required string ParticipantType { get; set; }
	public required string ParticipantId { get; set; }
	public required string ParticipantName { get; set; }
	public required UserIdentifier InitiatorUserId { get; set; }
	public required DateTime CreatedAt { get; set; }
	public required ApprovalStatus TournamentManagerApproval { get; set; }
	public DateTime? TournamentManagerApprovalDate { get; set; }
	public required ApprovalStatus ParticipantApproval { get; set; }
	public DateTime? ParticipantApprovalDate { get; set; }
}

public class ParticipantInfo
{
	public required TeamIdentifier TeamId { get; set; }
	public required string TeamName { get; set; }
	public required DateTime CreatedAt { get; set; }
}
