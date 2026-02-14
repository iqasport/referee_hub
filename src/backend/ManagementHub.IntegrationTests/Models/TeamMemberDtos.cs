namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// Test DTO for deserializing team member view models with string UserId.
/// The service uses strongly-typed UserIdentifier, but JSON serialization converts it to string.
/// </summary>
public class TeamMemberViewModelDto
{
	public required string UserId { get; set; }
	public required string Name { get; set; }
	public string? PrimaryTeamName { get; set; }
	public string? PrimaryTeamId { get; set; }
}
