using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Team;

public class TeamData
{
	public required string Name { get; set; }
	public required string City { get; set; }
	public string? State { get; set; }
	public required string Country { get; set; }
	public required TeamStatus Status { get; set; }
	public required TeamGroupAffiliation GroupAffiliation { get; set; }
	public required DateTime JoinedAt { get; set; }
	public string? LogoUrl { get; set; }
	public string? Description { get; set; }
	public string? ContactEmail { get; set; }
}
