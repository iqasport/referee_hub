namespace ManagementHub.Service.Areas.Referees;

public class RefereeTeamUpdateStatusViewModel
{
	public string TeamRequestTrackingPath { get; set; } = "/api/v2/Users/me/teamInvites";

	public RefereeTeamRequestStatusViewModel? PlayingTeam { get; set; }

	public RefereeTeamRequestStatusViewModel? CoachingTeam { get; set; }
}

public class RefereeTeamRequestStatusViewModel
{
	public required string TeamId { get; init; }

	public required string Status { get; init; }

	public required bool RequestCreated { get; init; }
}

public static class RefereeTeamRequestStatus
{
	public const string Applied = "applied";

	public const string Pending = "pending";
}
