using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Teams;

public class TeamPlayerActivityViewModel
{
	public required TeamIdentifier TeamId { get; set; }
	public required TeamPlayerActivityType ActivityType { get; set; }
	public required string Email { get; set; }
	public string? TeamName { get; set; }
	public UserIdentifier? UserId { get; set; }
	public string? UserName { get; set; }
	public string? InitiatorName { get; set; }
	public required DateTime CreatedAt { get; set; }
}

public class CurrentUserTeamInviteViewModel
{
	public required string InvitationId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
	public required string TeamName { get; set; }
	public required string Email { get; set; }
	public required DateTime CreatedAt { get; set; }
	public string? InvitedByName { get; set; }
	public bool CanRespond { get; set; }
}