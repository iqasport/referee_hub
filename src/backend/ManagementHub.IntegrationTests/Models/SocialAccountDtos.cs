using System;

namespace ManagementHub.IntegrationTests.Models;

/// <summary>
/// DTOs for testing social account operations
/// </summary>
public class NgbInfoDto
{
	public required string Name { get; set; }
	public string? Country { get; set; }
	public string? Acronym { get; set; }
	public string? Website { get; set; }
	public int PlayerCount { get; set; }
	public SocialAccountDto[] SocialAccounts { get; set; } = Array.Empty<SocialAccountDto>();
}

public class NgbUpdateModelDto
{
	public required string Name { get; set; }
	public string? Country { get; set; }
	public string? Acronym { get; set; }
	public string? Website { get; set; }
	public int PlayerCount { get; set; }
	public SocialAccountDto[] SocialAccounts { get; set; } = Array.Empty<SocialAccountDto>();
}

public class NgbTeamViewModelDto
{
	public string? TeamId { get; set; }
	public required string Name { get; set; }
	public required string City { get; set; }
	public string? State { get; set; }
	public required string Country { get; set; }
	public required string Status { get; set; }
	public required string GroupAffiliation { get; set; }
	public required string JoinedAt { get; set; }
	public SocialAccountDto[] SocialAccounts { get; set; } = Array.Empty<SocialAccountDto>();
}

public class SocialAccountDto
{
	public required string Url { get; set; }
	public required string Type { get; set; }
}
