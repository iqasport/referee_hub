using ManagementHub.Models.Enums;
using System;

namespace ManagementHub.Models.Domain.Tournament;

public class TournamentData
{
	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public TournamentType Type { get; set; }
	public string Country { get; set; } = null!;
	public string City { get; set; } = null!;
	public string? Place { get; set; }
	public string Organizer { get; set; } = null!;
	public bool IsPrivate { get; set; }
}
