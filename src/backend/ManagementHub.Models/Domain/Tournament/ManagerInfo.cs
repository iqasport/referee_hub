using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tournament;

public class ManagerInfo
{
	public required UserIdentifier UserId { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}
