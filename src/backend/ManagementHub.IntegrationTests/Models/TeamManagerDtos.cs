using System.Text.Json.Serialization;

namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// Test DTO for deserializing team manager view models with string Id.
/// The service uses strongly-typed UserIdentifier, but JSON serialization converts it to string.
/// </summary>
public class TeamManagerViewModelDto
{
	public required string Id { get; set; }
	public required string Name { get; set; }
	public string? Email { get; set; }
}

/// <summary>
/// Test DTO for creating team managers.
/// </summary>
public class TeamManagerCreationModelDto
{
	public required string Email { get; set; }
	public bool CreateAccountIfNotExists { get; set; }
}

/// <summary>
/// Status returned when creating a team manager.
/// Uses JsonStringEnumMemberConverter to match service serialization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TeamManagerCreationStatusDto
{
	InvalidEmail = 0,
	UserDoesNotExist = 1,
	ManagerRoleAdded = 2,
	ManagerUserCreated = 3,
}
