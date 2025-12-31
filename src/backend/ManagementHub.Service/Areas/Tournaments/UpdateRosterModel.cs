using System.Collections.Generic;

namespace ManagementHub.Service.Areas.Tournaments;

public class UpdateRosterModel
{
	public required List<RosterPlayerModel> Players { get; set; }
	public required List<RosterStaffModel> Coaches { get; set; }
	public required List<RosterStaffModel> Staff { get; set; }
}

public class RosterPlayerModel
{
	public required string UserId { get; set; }
	public required string Number { get; set; }
	public string? Gender { get; set; }
}

public class RosterStaffModel
{
	public required string UserId { get; set; }
}
