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

/// <summary>
/// Team detail view model with full information including managers and members.
/// </summary>
public class TeamDetailViewModelDto
{
	public string? TeamId { get; set; }
	public required string Name { get; set; }
	public required string City { get; set; }
	public string? State { get; set; }
	public required string Country { get; set; }
	public required string Status { get; set; }
	public required string GroupAffiliation { get; set; }
	public required string JoinedAt { get; set; }
	public string? LogoUrl { get; set; }
	public string? Description { get; set; }
	public string? ContactEmail { get; set; }
	public SocialAccountDto[] SocialAccounts { get; set; } = Array.Empty<SocialAccountDto>();
	public TeamManagerViewModelDto[] Managers { get; set; } = Array.Empty<TeamManagerViewModelDto>();
	public TeamMemberViewModelDto[] Members { get; set; } = Array.Empty<TeamMemberViewModelDto>();
	public bool IsCurrentUserManager { get; set; }
}
