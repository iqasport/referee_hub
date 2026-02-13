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
	public required List<PlayerViewModel> Players { get; set; }
	public required List<StaffViewModel> Coaches { get; set; }
	public required List<StaffViewModel> Staff { get; set; }
}

public class StaffViewModel
{
	public required UserIdentifier UserId { get; set; }
	public required string UserName { get; set; }
}

public class PlayerViewModel : StaffViewModel
{
	public required string Number { get; set; }
	public string? Gender { get; set; }
}

public class RosterEntryViewModel
{
	public string? Name { get; set; }
	public string? Pronouns { get; set; }
	public string? Gender { get; set; }
	public string? JerseyNumber { get; set; }
	public RosterRole Role { get; set; }
	public string? MaxCertification { get; set; }
	public DateTime? MaxCertificationDate { get; set; }
}

