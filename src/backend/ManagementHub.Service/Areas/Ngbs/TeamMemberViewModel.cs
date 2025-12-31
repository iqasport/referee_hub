using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.Ngbs;

public class TeamMemberViewModel
{
	public required UserIdentifier UserId { get; set; }
	public required string Name { get; set; }
}
