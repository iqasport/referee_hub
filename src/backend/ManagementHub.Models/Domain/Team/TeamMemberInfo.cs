using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Team;

public class TeamMemberInfo
{
	public required UserIdentifier UserId { get; set; }
	public required string Name { get; set; }
	public string? PrimaryTeamName { get; set; }
	public TeamIdentifier? PrimaryTeamId { get; set; }
}
