using System;
using System.Collections.Generic;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tournament;

public class PublicTournamentSnapshotPayload
{
	public required DateTime GeneratedAtUtc { get; init; }
	public required IReadOnlyList<PublicTournamentSnapshotTournament> Tournaments { get; init; }
}

public class PublicTournamentSnapshotTournament
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required string Description { get; init; }
	public required DateOnly StartDate { get; init; }
	public required DateOnly EndDate { get; init; }
	public DateOnly? RegistrationEndsDate { get; init; }
	public required TournamentType Type { get; init; }
	public required string Country { get; init; }
	public required string City { get; init; }
	public string? Place { get; init; }
	public required string Organizer { get; init; }
	public required bool IsRegistrationOpen { get; init; }
}