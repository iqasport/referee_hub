using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentModel
{
	public required string Name { get; set; }
	public required string Description { get; set; }
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public DateOnly? RegistrationEndsDate { get; set; }
	public TournamentType Type { get; set; }
	public required string Country { get; set; }
	public required string City { get; set; }
	public string? Place { get; set; }
	public required string Organizer { get; set; }
	public bool IsPrivate { get; set; }
}
