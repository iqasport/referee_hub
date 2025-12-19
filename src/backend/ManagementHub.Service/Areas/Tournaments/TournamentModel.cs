using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentModel
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
