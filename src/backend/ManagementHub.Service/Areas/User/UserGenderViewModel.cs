using System;
using System.Collections.Generic;

namespace ManagementHub.Service.Areas.User;

public class UserGenderViewModel
{
	public string? Gender { get; set; }
	public required List<TournamentReferenceViewModel> ReferencedInTournaments { get; set; }
}

public class TournamentReferenceViewModel
{
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required DateOnly StartDate { get; set; }
	public required DateOnly EndDate { get; set; }
}
