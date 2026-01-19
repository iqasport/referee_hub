using System.Text.Json.Serialization;

namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// Test DTO for deserializing managed team view models.
/// Matches ManagedTeamViewModel structure with serialized identifiers.
/// </summary>
public class ManagedTeamViewModelDto
{
	public required string TeamId { get; set; }
	public required string TeamName { get; set; }
	public required string Ngb { get; set; }
	public TeamGroupAffiliationDto? GroupAffiliation { get; set; }
}

/// <summary>
/// Test DTO for TeamGroupAffiliation enum.
/// Uses JsonStringEnumMemberConverter to match service serialization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TeamGroupAffiliationDto
{
	University = 0,
	Community = 1,
	Youth = 2,
	NotApplicable = 3,
	National = 4,
}
