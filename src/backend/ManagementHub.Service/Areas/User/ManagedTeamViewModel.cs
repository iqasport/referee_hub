using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.User;

public class ManagedTeamViewModel
{
	public required TeamIdentifier TeamId { get; set; }
	public required string TeamName { get; set; }
	public required NgbIdentifier Ngb { get; set; }
	public TeamGroupAffiliation? GroupAffiliation { get; set; }
}
