using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
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
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
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
	private readonly IUserContextAccessor contextAccessor;
	private readonly IUpdateUserAvatarCommand updateUserAvatarCommand;
	private readonly IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand;
	private readonly ManagementHubDbContext dbContext;
	private readonly IAttachmentRepository attachmentRepository;
	private readonly IAccessFileCommand accessFileCommand;

	public TeamsController(
		ITeamContextProvider teamContextProvider,
		ISocialAccountsProvider socialAccountsProvider,
		IUserContextAccessor contextAccessor,
		IUpdateUserAvatarCommand updateUserAvatarCommand,
		IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand,
		ManagementHubDbContext dbContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFileCommand)
	{
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
		this.contextAccessor = contextAccessor;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
		this.updateTeamManagerRoleCommand = updateTeamManagerRoleCommand;
		this.dbContext = dbContext;
		this.attachmentRepository = attachmentRepository;
		this.accessFileCommand = accessFileCommand;
	}

	/// <summary>
	/// Get all national teams across all NGBs.
	/// </summary>
	[HttpGet("national")]
	[Tags("Team")]
	public async Task<Filtered<NgbTeamViewModel>> GetNationalTeams([FromQuery] FilteringParameters filtering)
	{
		var socialAccounts = await this.socialAccountsProvider.QueryTeamSocialAccounts(NgbConstraint.Any);
		var emptySocialAccounts = Enumerable.Empty<SocialAccount>();
		// Filter national teams directly in SQL using the groupAffiliation parameter
		var teams = await this.teamContextProvider.GetTeams(NgbConstraint.Any, TeamGroupAffiliation.National).ToListAsync();
		var logoUris = new Dictionary<TeamIdentifier, Uri?>();
		foreach (var team in teams)
		{
			logoUris[team.TeamId] = await this.teamContextProvider.GetTeamLogoUriAsync(team.TeamId, this.HttpContext.RequestAborted);
		}

		return teams
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
				LogoUri = logoUris.GetValueOrDefault(team.TeamId),
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
	[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
	public async Task<ActionResult<Uri>> UploadTeamLogo([FromRoute] TeamIdentifier teamId, [FromForm] IFormFile logoBlob)
	{
		// Authorization: user must be a team manager OR an NGB admin of the team's NGB
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles.OfType<TeamManagerRole>().Any(r => r.Team.AppliesTo(teamId));
		if (!isTeamManager)
		{
			var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
			var isNgbAdmin = team != null && userContext.Roles.OfType<NgbAdminRole>().Any(r => r.Ngb.AppliesTo(team.NgbId));
			if (!isNgbAdmin)
			{
				return this.Forbid();
			}
		}

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

		await this.updateUserAvatarCommand.UpdateTeamLogoAsync(
			teamId,
			logoBlob.ContentType,
			logoBlob.OpenReadStream(),
			this.HttpContext.RequestAborted);

		// Return a fresh URI generated from the attachment — temporary URLs are NOT stored in team data
		var logoUri = await this.teamContextProvider.GetTeamLogoUriAsync(teamId, this.HttpContext.RequestAborted);
		return logoUri ?? throw new InvalidOperationException($"Logo for team {teamId} was uploaded but could not be accessed");
	}

	/// <summary>
	/// Get details of a specific team including managers and members.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <returns>Team details with managers and members</returns>
	[HttpGet("{teamId}")]
	[Tags("Team")]
	[Authorize]
	public async Task<ActionResult<TeamDetailViewModel>> GetTeamDetails([FromRoute] TeamIdentifier teamId)
	{
		// Authorization: check IQA admin and team manager status before any DB lookup
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isIqaAdmin = userContext.Roles.OfType<IqaAdminRole>().Any();
		var isTeamManager = userContext.Roles.OfType<TeamManagerRole>().Any(r => r.Team.AppliesTo(teamId));
		var isNgbAdmin = false;

		ITeamContext? team = null;
		if (!isIqaAdmin && !isTeamManager)
		{
			// Need to load the team to check if the user is an NGB admin for its NGB
			team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
			if (team == null)
			{
				return this.NotFound();
			}

			isNgbAdmin = userContext.Roles.OfType<NgbAdminRole>().Any(r => r.Ngb.AppliesTo(team.NgbId));
			if (!isNgbAdmin)
			{
				return this.Forbid();
			}
		}

		// Load team if not already loaded above
		team ??= await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			return this.NotFound();
		}

		// Get social accounts for this specific team
		var socialAccounts = await this.socialAccountsProvider.GetTeamSocialAccounts(teamId);

		// Get managers
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);

		// Get members — visible to team managers, IQA admins, and NGB admins (but not regular authenticated users)
		IEnumerable<TeamMemberInfo> members = Enumerable.Empty<TeamMemberInfo>();
		if (isTeamManager || isIqaAdmin || isNgbAdmin)
		{
			members = await this.teamContextProvider.QueryTeamMembers(teamId, NgbConstraint.Any).ToListAsync();
		}

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
			LogoUri = await this.GetTeamLogoUriAsync(teamId),
			Description = team.TeamData.Description,
			ContactEmail = team.TeamData.ContactEmail,
			SocialAccounts = socialAccounts,
			Managers = managers.Select(m => new TeamManagerViewModel
			{
				Id = m.UserId,
				Name = m.Name,
				// Email is intentionally omitted here — use the team management endpoint for full access.
			}),
			// Members are only visible to team managers, IQA admins, and NGB admins for privacy reasons —
			// any authenticated user could otherwise join a team and view other members' personal info.
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
	/// Update team details.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <param name="viewModel">Updated team data</param>
	/// <returns>Updated team data</returns>
	[HttpPut("{teamId}")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.TeamManagerPolicy)]
	public async Task<ActionResult<NgbTeamViewModel>> UpdateTeam([FromRoute] TeamIdentifier teamId, [FromBody] NgbTeamViewModel viewModel)
	{
		try
		{
			// Validate team ID mismatch BEFORE authorization so BadRequest takes precedence over Forbidden
			if (viewModel.TeamId != teamId)
			{
				return this.BadRequest("Team id mismatch between URL and request body.");
			}

			// Authorization: user must be a team manager for the specified team
			var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
			var isTeamManager = userContext.Roles
				.OfType<TeamManagerRole>()
				.Any(role => role.Team.AppliesTo(teamId));

			if (!isTeamManager)
			{
				return this.Forbid();
			}

			// Get the team to find its NGB
			var existingTeam = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
			if (existingTeam == null)
			{
				throw new ManagementHub.Models.Exceptions.NotFoundException($"Team {teamId} not found");
			}

			var teamData = new TeamData
			{
				Name = viewModel.Name,
				City = viewModel.City,
				State = viewModel.State,
				Country = viewModel.Country,
				Status = viewModel.Status,
				GroupAffiliation = viewModel.GroupAffiliation,
				JoinedAt = viewModel.JoinedAt.ToDateTime(default, DateTimeKind.Utc),
				Description = viewModel.Description,
				ContactEmail = viewModel.ContactEmail,
			};

			var team = await this.teamContextProvider.UpdateTeamAsync(existingTeam.NgbId, teamId, teamData);
			var socialAccounts = await this.socialAccountsProvider.UpdateTeamSocialAccounts(team.TeamId, viewModel.SocialAccounts);

			return new NgbTeamViewModel
			{
				TeamId = team.TeamId,
				City = team.TeamData.City,
				GroupAffiliation = team.TeamData.GroupAffiliation,
				Name = team.TeamData.Name,
				Status = team.TeamData.Status,
				State = team.TeamData.State,
				Country = team.TeamData.Country,
				JoinedAt = DateOnly.FromDateTime(team.TeamData.JoinedAt),
				SocialAccounts = socialAccounts,
				LogoUri = await this.teamContextProvider.GetTeamLogoUriAsync(teamId, this.HttpContext.RequestAborted),
				Description = team.TeamData.Description,
				ContactEmail = team.TeamData.ContactEmail,
			};
		}
		catch (ManagementHub.Models.Exceptions.NotFoundException)
		{
			return this.NotFound();
		}
		catch (ArgumentException)
		{
			return this.BadRequest();
		}
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
			LogoUri = await this.GetTeamLogoUriAsync(teamId),
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
	/// Add a user as a team manager.
	/// </summary>
	/// <param name="teamId">Team identifier</param>
	/// <param name="request">Request containing user email</param>
	/// <returns>Success message</returns>
	[HttpPost("{teamId}/managers")]
	[Tags("TeamManagement")]
	[Authorize(AuthorizationPolicies.TeamManagerPolicy)]
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
			return this.Forbid();
		}

		// Parse and validate email
		if (!Email.TryParse(request.Email, out var email))
		{
			return this.BadRequest("Invalid email address");
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
				this.BadRequest("No user found with that email address"),
			IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded =>
				this.Ok("User successfully added as team manager"),
			IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole =>
				this.Ok("User created and added as team manager"),
			_ => this.StatusCode(500, "An unexpected error occurred")
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
	[Authorize(AuthorizationPolicies.TeamManagerPolicy)]
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
			return this.Forbid();
		}

		// Resolve the UserIdentifier to the actual database User.Id
		// (UserIdentifier.Id uses ToLegacyUserId() which is only correct for legacy users,
		//  so we must use WithIdentifier to handle modern users with real GUIDs as well)
		var userDbId = await this.dbContext.Users
			.WithIdentifier(playerId)
			.Select(u => (long?)u.Id)
			.FirstOrDefaultAsync();

		if (userDbId == null)
		{
			return this.NotFound("Player not found");
		}

		// Remove the RefereeTeam association
		var deleted = await this.dbContext.RefereeTeams
			.Where(rt => rt.TeamId == teamId.Id && rt.RefereeId == userDbId)
			.ExecuteDeleteAsync();

		if (deleted == 0)
		{
			return this.NotFound("Player not found on this team");
		}

		return this.NoContent();
	}

	private async Task<Uri?> GetTeamLogoUriAsync(TeamIdentifier teamId)
	{
		const string attachmentName = "logo";
		var attachment = await this.attachmentRepository.GetAttachmentAsync(teamId, attachmentName, this.HttpContext.RequestAborted);
		if (attachment == null)
		{
			return null;
		}

		return await this.accessFileCommand.GetFileAccessUriAsync(attachment.Blob.Key, TimeSpan.FromMinutes(5), this.HttpContext.RequestAborted);
	}
}
