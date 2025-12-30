using System;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentInviteViewModel
{
	public required ParticipantType ParticipantType { get; set; }
	public required string ParticipantId { get; set; }
	public required string ParticipantName { get; set; }
	public required InviteStatus Status { get; set; }
	public required UserIdentifier InitiatorUserId { get; set; }
	public required DateTime CreatedAt { get; set; }
	public required ApprovalStatusViewModel TournamentManagerApproval { get; set; }
	public required ApprovalStatusViewModel ParticipantApproval { get; set; }
}

public class ApprovalStatusViewModel
{
	public required ApprovalStatus Status { get; set; }
	public DateTime? Date { get; set; }
}

public class CreateInviteModel
{
	public required ParticipantType ParticipantType { get; set; }
	public required string ParticipantId { get; set; }
}

public class InviteResponseModel
{
	public bool Approved { get; set; }
}

public class TournamentParticipantViewModel
{
	public required TeamIdentifier TeamId { get; set; }
	public required string TeamName { get; set; }
}

