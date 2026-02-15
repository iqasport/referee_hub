using System;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage;
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
	private readonly IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand;
	private readonly IUserContextAccessor contextAccessor;
	private readonly ManagementHubDbContext dbContext;

	public TeamsController(
		ITeamContextProvider teamContextProvider,
		ISocialAccountsProvider socialAccountsProvider,
		IUpdateUserAvatarCommand updateUserAvatarCommand,
		IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand,
		IUserContextAccessor contextAccessor,
		ManagementHubDbContext dbContext)
	{
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
		this.updateTeamManagerRoleCommand = updateTeamManagerRoleCommand;
		this.contextAccessor = contextAccessor;
		this.dbContext = dbContext;
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
	public async Task<Uri> UploadTeamLogo([FromRoute] TeamIdentifier teamId, [FromForm] IFormFile logoBlob)
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

		// Update the team's LogoUrl field so it's reflected in the team data
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team != null)
		{
			var updatedTeamData = new TeamData
			{
				Name = team.TeamData.Name,
				City = team.TeamData.City,
				State = team.TeamData.State,
				Country = team.TeamData.Country,
				Status = team.TeamData.Status,
				GroupAffiliation = team.TeamData.GroupAffiliation,
				JoinedAt = team.TeamData.JoinedAt,
				LogoUrl = logoUri.ToString(),
				Description = team.TeamData.Description,
				ContactEmail = team.TeamData.ContactEmail,
			};
			await this.teamContextProvider.UpdateTeamAsync(team.NgbId, teamId, updatedTeamData);
		}

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

		// Check if current user is a manager of this team
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		// Get social accounts for this specific team
		var socialAccounts = await this.socialAccountsProvider.GetTeamSocialAccounts(teamId);

		// Get managers
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);
		
		// Get members
		var membersQuery = this.teamContextProvider.QueryTeamMembers(teamId, NgbConstraint.Any);
		var members = await membersQuery.ToListAsync();

		return new TeamDetailViewModel
		{
			NgbId = team.NgbId,
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
			SocialAccounts = socialAccounts,
			Managers = managers.Select(m => new TeamManagerViewModel
			{
				Id = m.UserId,
				Name = m.Name,
				Email = m.Email
			}),
			Members = members.Select(m => new TeamMemberViewModel
			{
				UserId = m.UserId,
				Name = m.Name,
				PrimaryTeamName = m.PrimaryTeamName,
				PrimaryTeamId = m.PrimaryTeamId?.ToString()
			}),
			IsCurrentUserManager = isTeamManager
		};
	}

	/// <summary>
	/// Get team management data including managers, players, and pending invites.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <returns>Team management data</returns>
	[HttpGet("{teamId}/management")]
	[Tags("TeamManagement")]
	[Authorize(AuthorizationPolicies.TeamManagerPolicy)]
	public async Task<TeamManagementViewModel> GetTeamManagement([FromRoute] TeamIdentifier teamId)
	{
		// Verify user is a manager of this team
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!isTeamManager)
		{
			throw new UnauthorizedAccessException($"User is not a manager of team {teamId}");
		}

		// Get team details
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			throw new ArgumentException($"Team {teamId} not found");
		}

		// Get social accounts
		var socialAccounts = await this.socialAccountsProvider.GetTeamSocialAccounts(teamId);

		// Get managers
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);

		// Get members
		var membersQuery = this.teamContextProvider.QueryTeamMembers(teamId, NgbConstraint.Any);
		var members = await membersQuery.ToListAsync();

		// TODO: Get pending invites (Phase 3 - will be implemented separately)
		var pendingInvites = Enumerable.Empty<TeamInvitationViewModel>();

		return new TeamManagementViewModel
		{
			TeamId = team.TeamId,
			Name = team.TeamData.Name,
			City = team.TeamData.City,
			State = team.TeamData.State,
			Country = team.TeamData.Country,
			Status = team.TeamData.Status,
			GroupAffiliation = team.TeamData.GroupAffiliation,
			LogoUrl = team.TeamData.LogoUrl,
			Description = team.TeamData.Description,
			ContactEmail = team.TeamData.ContactEmail,
			SocialAccounts = socialAccounts,
			Managers = managers.Select(m => new TeamManagerViewModel
			{
				Id = m.UserId,
				Name = m.Name,
				Email = m.Email
			}),
			Members = members.Select(m => new TeamMemberViewModel
			{
				UserId = m.UserId,
				Name = m.Name,
				PrimaryTeamName = m.PrimaryTeamName,
				PrimaryTeamId = m.PrimaryTeamId?.ToString()
			}),
			PendingInvites = pendingInvites
		};
	}

	/// <summary>
	/// Promote a team player to team manager.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <param name="playerId">Player user identifier</param>
	/// <returns>Success status</returns>
	[HttpPost("{teamId}/players/{playerId}/make-manager")]
	[Tags("TeamManagement")]
	[Authorize(AuthorizationPolicies.TeamManagerPolicy)]
	public async Task<IActionResult> MakePlayerManager(
		[FromRoute] TeamIdentifier teamId,
		[FromRoute] UserIdentifier playerId)
	{
		// Verify user is a manager of this team
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!isTeamManager)
		{
			return Unauthorized($"User is not a manager of team {teamId}");
		}

		// Get the player's email to use for adding manager role
		// TODO: This requires looking up the user's email - for now, skip this implementation
		// The proper way would be to use UserContext to get the email
		return StatusCode(501, "Make player manager not yet fully implemented - needs user email lookup");
	}

	/// <summary>
	/// Add a user as a team manager.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <param name="request">Request containing user email</param>
	/// <returns>Success message</returns>
	[HttpPost("{teamId}/managers")]
	[Tags("TeamManagement")]
	[Authorize]
	public async Task<ActionResult<string>> AddTeamManager(
		[FromRoute] TeamIdentifier teamId,
		[FromBody] AddTeamManagerRequest request)
	{
		// Verify user is a manager of this team
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!isTeamManager)
		{
			return Forbid();
		}

		// Parse and validate email
		if (!Email.TryParse(request.Email, out var email))
		{
			return BadRequest("Invalid email address");
		}

		// Add team manager role (will handle user existence check and duplicate check)
		var result = await this.updateTeamManagerRoleCommand.AddTeamManagerRoleAsync(
			teamId,
			email,
			createUserIfNotExists: false,
			userContext.UserId);

		return result switch
		{
			IUpdateTeamManagerRoleCommand.AddRoleResult.UserDoesNotExist =>
				BadRequest("No user found with that email address"),
			IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded =>
				Ok("User successfully added as team manager"),
			IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole =>
				Ok("User created and added as team manager"),
			_ => StatusCode(500, "An unexpected error occurred")
		};
	}

	/// <summary>
	/// Remove a player from the team.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <param name="playerId">Player user identifier</param>
	/// <returns>Success status</returns>
	[HttpDelete("{teamId}/players/{playerId}")]
	[Tags("TeamManagement")]
	[Authorize]
	public async Task<IActionResult> RemovePlayer(
		[FromRoute] TeamIdentifier teamId,
		[FromRoute] UserIdentifier playerId)
	{
		// Verify user is a manager of this team
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!isTeamManager)
		{
			return Forbid();
		}

		// Remove the RefereeTeam association
		var deleted = await this.dbContext.RefereeTeams
			.Where(rt => rt.TeamId == teamId.Id && rt.RefereeId == playerId.Id)
			.ExecuteDeleteAsync();

		if (deleted == 0)
		{
			return NotFound("Player not found on this team");
		}

		return NoContent();
	}
}
