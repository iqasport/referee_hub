using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Service.Areas.User;

public class ManagedTeamViewModel
{
	public required TeamIdentifier TeamId { get; set; }
	public required string TeamName { get; set; }
	public required string NgbCountryCode { get; set; }
}
