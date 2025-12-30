using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentManagerViewModel
{
	public required UserIdentifier Id { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}
