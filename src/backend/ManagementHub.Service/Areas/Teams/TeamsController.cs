using System;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Actions related to Teams.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ISocialAccountsProvider socialAccountsProvider;
	private readonly IUpdateUserAvatarCommand updateUserAvatarCommand;

	public TeamsController(
		ITeamContextProvider teamContextProvider,
		ISocialAccountsProvider socialAccountsProvider,
		IUpdateUserAvatarCommand updateUserAvatarCommand)
	{
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
	}

	/// <summary>
	/// Get all national teams across all NGBs.
	/// </summary>
	/// <summary>
	/// Get all national teams across all NGBs.
	/// </summary>
	/// <remarks>
	/// This endpoint materializes all teams before filtering to avoid LINQ translation issues.
	/// Since national teams are typically few in number (&lt;100), the performance impact is minimal.
	/// </remarks>
	[HttpGet("national")]
	[Tags("Team")]
	public async Task<Filtered<NgbTeamViewModel>> GetNationalTeams([FromQuery] FilteringParameters filtering)
	{
		var socialAccounts = await this.socialAccountsProvider.QueryTeamSocialAccounts(NgbConstraint.Any);
		var emptySocialAccounts = Enumerable.Empty<SocialAccount>();
		// Materialize the query before filtering to avoid LINQ translation issues
		// This is acceptable because national teams are few in number
		var teams = await this.teamContextProvider.GetTeams(NgbConstraint.Any).ToListAsync();
		return teams
			.Where(team => team.TeamData.GroupAffiliation == TeamGroupAffiliation.National)
			.Select(team => new NgbTeamViewModel
			{
				TeamId = team.TeamId,
				City = team.TeamData.City,
				GroupAffiliation = team.TeamData.GroupAffiliation,
				Name = team.TeamData.Name,
				Status = team.TeamData.Status,
				State = team.TeamData.State,
				Country = team.TeamData.Country,
				JoinedAt = DateOnly.FromDateTime(team.TeamData.JoinedAt),
				SocialAccounts = socialAccounts.GetValueOrDefault(team.TeamId, emptySocialAccounts),
				LogoUrl = team.TeamData.LogoUrl,
				Description = team.TeamData.Description,
				ContactEmail = team.TeamData.ContactEmail,
			}).AsFiltered();
	}

	/// <summary>
	/// Upload a logo for a team.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <param name="logoBlob">Logo image file</param>
	/// <returns>URL to access the uploaded logo</returns>
	[HttpPut("{teamId}/logo")]
	[Tags("Team")]
	[Authorize] // TODO: Add appropriate authorization policy for team managers and NGB admins
	public async Task<Uri> UploadTeamLogo([FromRoute] TeamIdentifier teamId, IFormFile logoBlob)
	{
		// Validate file is an image
		if (!logoBlob.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("File must be an image (image/*)");
		}

		// Validate file size (max 5 MB)
		const long maxSize = 5 * 1024 * 1024;
		if (logoBlob.Length > maxSize)
		{
			throw new ArgumentException($"File size must not exceed {maxSize / (1024 * 1024)} MB");
		}

		var logoUri = await this.updateUserAvatarCommand.UpdateTeamLogoAsync(
			teamId,
			logoBlob.ContentType,
			logoBlob.OpenReadStream(),
			this.HttpContext.RequestAborted);
		return logoUri;
	}

	/// <summary>
	/// Get details of a specific team including managers and members.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <returns>Team details with managers and members</returns>
	[HttpGet("{teamId}")]
	[Tags("Team")]
	[Authorize]
	public async Task<TeamDetailViewModel> GetTeamDetails([FromRoute] TeamIdentifier teamId)
	{
		// Get team details - any authenticated user can view team details
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		
		if (team == null)
		{
			throw new ArgumentException($"Team {teamId} not found");
		}

		// Get social accounts
		var socialAccounts = await this.socialAccountsProvider.QueryTeamSocialAccounts(NgbConstraint.Any);
		var emptySocialAccounts = Enumerable.Empty<SocialAccount>();

		// Get managers
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);
		
		// Get members
		var membersQuery = this.teamContextProvider.QueryTeamMembers(teamId, NgbConstraint.Any);
		var members = await membersQuery.ToListAsync();

		return new TeamDetailViewModel
		{
			TeamId = team.TeamId,
			Name = team.TeamData.Name,
			City = team.TeamData.City,
			State = team.TeamData.State,
			Country = team.TeamData.Country,
			Status = team.TeamData.Status,
			GroupAffiliation = team.TeamData.GroupAffiliation,
			JoinedAt = DateOnly.FromDateTime(team.TeamData.JoinedAt),
			LogoUrl = team.TeamData.LogoUrl,
			Description = team.TeamData.Description,
			ContactEmail = team.TeamData.ContactEmail,
			SocialAccounts = socialAccounts.GetValueOrDefault(team.TeamId, emptySocialAccounts),
			Managers = managers.Select(m => new TeamManagerViewModel
			{
				Id = m.UserId,
				Name = m.Name,
				Email = m.Email
			}),
			Members = members.Select(m => new TeamMemberViewModel
			{
				UserId = m.UserId,
				Name = m.Name
			})
		};
	}
}
