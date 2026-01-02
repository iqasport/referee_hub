using System.Collections.Generic;

namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// DTO for updating tournament participant rosters.
/// Uses string for UserId to support JSON serialization in integration tests.
/// </summary>
public class UpdateRosterDto
{
	public required List<RosterPlayerDto> Players { get; set; }
	public required List<RosterStaffDto> Coaches { get; set; }
	public required List<RosterStaffDto> Staff { get; set; }
}

/// <summary>
/// DTO for roster staff members (coaches and staff).
/// </summary>
public class RosterStaffDto
{
	public required string UserId { get; set; }
}

/// <summary>
/// DTO for roster players.
/// Extends RosterStaffDto and adds jersey number and optional gender.
/// </summary>
public class RosterPlayerDto : RosterStaffDto
{
	public required string Number { get; set; }
	public string? Gender { get; set; }
}
