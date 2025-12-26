using System;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Domain.Tournament;

public class TeamParticipantInfo
{
	public required TeamIdentifier TeamId { get; set; }
	public required string TeamName { get; set; }
	public required DateTime CreatedAt { get; set; }
}
