using System;

namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentInviteViewModel
{
	public long Id { get; set; }
	public string ParticipantType { get; set; } = null!;
	public string ParticipantId { get; set; } = null!;
	public string ParticipantName { get; set; } = null!;
	public string Status { get; set; } = null!;
	public string InitiatorUserId { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public ApprovalStatusViewModel TournamentManagerApproval { get; set; } = null!;
	public ApprovalStatusViewModel ParticipantApproval { get; set; } = null!;
}

public class ApprovalStatusViewModel
{
	public string Status { get; set; } = null!;
	public DateTime? Date { get; set; }
}

public class CreateInviteModel
{
	public string ParticipantType { get; set; } = null!;
	public string ParticipantId { get; set; } = null!;
}

public class InviteResponseModel
{
	public bool Approved { get; set; }
}

public class TournamentParticipantViewModel
{
	public string TeamId { get; set; } = null!;
	public string TeamName { get; set; } = null!;
	public string Type { get; set; } = null!;
}
