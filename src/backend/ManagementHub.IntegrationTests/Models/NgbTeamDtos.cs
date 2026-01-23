using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// Test DTO for NgbTeamViewModel.
/// Matches the structure from ManagementHub.Service.Areas.Ngbs.NgbTeamViewModel.
/// </summary>
public class NgbTeamViewModelDto
{
	public string? TeamId { get; set; }
	public required string Name { get; set; }
	public required string City { get; set; }
	public string? State { get; set; }
	public required string Country { get; set; }
	public required TeamStatusDto Status { get; set; }
	public required TeamGroupAffiliationDto GroupAffiliation { get; set; }
	public required DateOnly JoinedAt { get; set; }
	public required IEnumerable<SocialAccountDto> SocialAccounts { get; set; }
}

/// <summary>
/// Test DTO for TeamStatus enum.
/// Uses JsonStringEnumMemberConverter to match service serialization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TeamStatusDto
{
	Competitive = 0,
	Developing = 1,
	Inactive = 2,
	Suspended = 3,
	SuspendedByIqa = 4,
}

/// <summary>
/// Test DTO for SocialAccount.
/// </summary>
public class SocialAccountDto
{
	public required string AccountType { get; set; }
	public required string AccountUrl { get; set; }
}
