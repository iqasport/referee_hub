using System.Collections.Generic;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.Tournaments;

public class UpdateRosterModel
{
	public required List<RosterPlayerModel> Players { get; set; }
	public required List<RosterStaffModel> Coaches { get; set; }
	public required List<RosterStaffModel> Staff { get; set; }
}

public class RosterStaffModel
{
	public required UserIdentifier UserId { get; set; }
}

public class RosterPlayerModel : RosterStaffModel
{
	public required string Number { get; set; }
	public string? Gender { get; set; }
}
