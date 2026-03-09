using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// Test DTO for deserializing tournament ID response with string ID.
/// </summary>
public class TournamentIdResponseDto
{
	public required string Id { get; set; }
}

/// <summary>
/// Test DTO for deserializing tournament view models with string ID (since JSON serializer converts TournamentIdentifier to string).
/// </summary>
public class TournamentViewModelDto
{
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required DateOnly StartDate { get; set; }
	public required DateOnly EndDate { get; set; }
	public required TournamentType Type { get; set; }
	public required string Country { get; set; }
	public required string City { get; set; }
	public string? Place { get; set; }
	public required string Organizer { get; set; }
	public required bool IsPrivate { get; set; }
	public string? BannerImageUrl { get; set; }
	public bool IsCurrentUserInvolved { get; set; }
}
