using System;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface ITournamentContext
{
	TournamentIdentifier Id { get; }
	string Name { get; }
	string Description { get; }
	DateOnly StartDate { get; }
	DateOnly EndDate { get; }
	TournamentType Type { get; }
	string Country { get; }
	string City { get; }
	string? Place { get; }
	string Organizer { get; }
	bool IsPrivate { get; }
}
