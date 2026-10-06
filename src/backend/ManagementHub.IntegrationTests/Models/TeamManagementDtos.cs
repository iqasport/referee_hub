using System.Collections.Generic;

namespace ManagementHub.IntegrationTests.Models;

public class TeamManagementViewModelDto
{
	public bool AutoApprovePlayerRequests { get; set; }
	public List<TeamMemberViewModelDto> Members { get; set; } = [];
	public List<TeamInvitationViewModelDto> PendingInvites { get; set; } = [];
	public List<TeamPlayerActivityViewModelDto> PlayerHistory { get; set; } = [];
}

public class TeamPlayerActivityViewModelDto
{
	public required string ActivityType { get; set; }
	public required string Email { get; set; }
}