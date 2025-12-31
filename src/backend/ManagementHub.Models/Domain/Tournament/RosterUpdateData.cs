using System.Collections.Generic;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tournament;

public class RosterUpdateData
{
	public required List<RosterPlayerData> Players { get; set; }
	public required List<RosterStaffData> Coaches { get; set; }
	public required List<RosterStaffData> Staff { get; set; }
}

public class RosterPlayerData
{
	public required UserIdentifier UserId { get; set; }
	public required string JerseyNumber { get; set; }
	public string? Gender { get; set; }
}

public class RosterStaffData
{
	public required UserIdentifier UserId { get; set; }
}
