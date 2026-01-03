using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class TournamentTeamRosterEntry : IIdentifiable
{
	public long Id { get; set; }
	public required long TournamentTeamParticipantId { get; set; }
	public required long UserId { get; set; }
	public required RosterRole Role { get; set; }
	public string? JerseyNumber { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public virtual TournamentTeamParticipant Participant { get; set; } = null!;
	public virtual User User { get; set; } = null!;
}
