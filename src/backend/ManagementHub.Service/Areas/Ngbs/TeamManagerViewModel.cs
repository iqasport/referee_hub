using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.Ngbs;

public class TeamManagerViewModel
{
	public required UserIdentifier Id { get; set; }
	public required string Name { get; set; }
	public string? Email { get; set; }
}
