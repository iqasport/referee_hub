using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Notification;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Areas.Tournaments;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Service.Services;
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
	private readonly ISendTeamInviteEmail sendTeamInviteEmail;
	private readonly INotificationService notificationService;
	private readonly ManagementHubDbContext dbContext;
	private readonly IAttachmentRepository attachmentRepository;
	private readonly IAccessFileCommand accessFileCommand;
	private readonly ILogger<TeamsController> logger;

	public TeamsController(
		ITeamContextProvider teamContextProvider,
		ISocialAccountsProvider socialAccountsProvider,
		IUserContextAccessor contextAccessor,
		IUpdateUserAvatarCommand updateUserAvatarCommand,
		IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand,
		ISendTeamInviteEmail sendTeamInviteEmail,
		INotificationService notificationService,
		ManagementHubDbContext dbContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFileCommand,
		ILogger<TeamsController> logger)
	{
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
		this.contextAccessor = contextAccessor;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
		this.updateTeamManagerRoleCommand = updateTeamManagerRoleCommand;
		this.sendTeamInviteEmail = sendTeamInviteEmail;
		this.notificationService = notificationService;
		this.dbContext = dbContext;
		this.attachmentRepository = attachmentRepository;
		this.accessFileCommand = accessFileCommand;
		this.logger = logger;
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
	[Authorize]
	[IgnoreAntiforgeryToken] // API uses bearer token authentication, not cookies, so CSRF is not a concern
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
				Email = m.Email,
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
	[Authorize]
	public async Task<ActionResult<NgbTeamViewModel>> UpdateTeam([FromRoute] TeamIdentifier teamId, [FromBody] NgbTeamViewModel viewModel)
	{
		try
		{
			// Validate team ID mismatch BEFORE authorization so BadRequest takes precedence over Forbidden
			if (viewModel.TeamId != teamId)
			{
				return this.BadRequest("Team id mismatch between URL and request body.");
			}

			// Authorization: user must be a team manager for the specified team or an NGB admin for the team's NGB
			var existingTeam = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
			if (existingTeam == null)
			{
				throw new ManagementHub.Models.Exceptions.NotFoundException($"Team {teamId} not found");
			}

			var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
			var isTeamManager = userContext.Roles
				.OfType<TeamManagerRole>()
				.Any(role => role.Team.AppliesTo(teamId));
			var isNgbAdmin = userContext.Roles
				.OfType<NgbAdminRole>()
				.Any(role => role.Ngb.AppliesTo(existingTeam.NgbId));

			if (!isTeamManager && !isNgbAdmin)
			{
				return this.Forbid();
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
	[Authorize]
	public async Task<ActionResult<TeamManagementViewModel>> GetTeamManagement([FromRoute] TeamIdentifier teamId)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// Get team details
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			return this.NotFound();
		}

		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));
		var isNgbAdmin = userContext.Roles
			.OfType<NgbAdminRole>()
			.Any(role => role.Ngb.AppliesTo(team.NgbId));

		if (!isTeamManager && !isNgbAdmin)
		{
			return this.Forbid();
		}

		// Get social accounts
		var socialAccounts = await this.socialAccountsProvider.GetTeamSocialAccounts(teamId);

		// Get managers
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);

		// Get members
		var membersQuery = this.teamContextProvider.QueryTeamMembers(teamId, NgbConstraint.Any);
		var members = await membersQuery.ToListAsync();

		var pendingInviteRows = await this.dbContext.TeamInvitations
			.Where(i => i.TeamId == teamId.Id && i.RevokedAt == null && i.AcceptedAt == null && i.DeclinedAt == null)
			.OrderByDescending(i => i.CreatedAt)
			.Select(i => new
			{
				i.Id,
				i.Email,
				i.CreatedAt,
				InitiatorFirstName = i.Initiator.FirstName,
				InitiatorLastName = i.Initiator.LastName,
				InitiatorEmail = i.Initiator.Email
			})
			.ToListAsync();

		var pendingInvites = pendingInviteRows
			.Select(i => new TeamInvitationViewModel
			{
				InvitationId = i.Id.ToString(),
				Email = i.Email,
				CreatedAt = i.CreatedAt,
				InvitedByName = TeamInviteHelpers.BuildDisplayName(i.InitiatorFirstName, i.InitiatorLastName),
				RequiresManagerDecision = string.Equals(i.InitiatorEmail, i.Email, StringComparison.OrdinalIgnoreCase)
			})
			.ToList();

		var playerHistoryRows = await this.dbContext.TeamPlayerActivities
			.Where(activity => activity.TeamId == teamId.Id)
			.OrderByDescending(activity => activity.CreatedAt)
			.Take(25)
			.Select(activity => new
			{
				activity.TeamId,
				activity.ActivityType,
				activity.Email,
				activity.UserId,
				UserUniqueId = activity.User != null ? activity.User.UniqueId : null,
				UserFirstName = activity.User != null ? activity.User.FirstName : null,
				UserLastName = activity.User != null ? activity.User.LastName : null,
				InitiatorFirstName = activity.Initiator.FirstName,
				InitiatorLastName = activity.Initiator.LastName,
				activity.CreatedAt,
			})
			.ToListAsync();

		var playerHistory = playerHistoryRows
			.Select(activity => new TeamPlayerActivityViewModel
			{
				TeamId = new TeamIdentifier(activity.TeamId),
				ActivityType = activity.ActivityType,
				Email = activity.Email,
				UserId = activity.UserId == null
					? null
					: !string.IsNullOrWhiteSpace(activity.UserUniqueId)
						? UserIdentifier.Parse(activity.UserUniqueId)
						: UserIdentifier.FromLegacyUserId(activity.UserId.Value),
				UserName = TeamInviteHelpers.BuildDisplayName(activity.UserFirstName, activity.UserLastName),
				InitiatorName = TeamInviteHelpers.BuildDisplayName(activity.InitiatorFirstName, activity.InitiatorLastName),
				CreatedAt = activity.CreatedAt,
			})
			.ToList();

		return this.Ok(new TeamManagementViewModel
		{
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
			AutoApprovePlayerRequests = team.TeamData.AutoApprovePlayerRequests,
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
				Email = m.Email,
				PrimaryTeamName = m.PrimaryTeamName,
				PrimaryTeamId = m.PrimaryTeamId?.ToString()
			}),
			PendingInvites = pendingInvites,
			PlayerHistory = playerHistory
		});
	}

	[HttpPut("{teamId}/autoApprovePlayerRequests")]
	[Tags("TeamManagement")]
	[Authorize]
	public async Task<IActionResult> SetAutoApprovePlayerRequests(
		[FromRoute] TeamIdentifier teamId,
		[FromBody] SetTeamAutoApprovePlayerRequestsRequest request)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var authError = await this.ValidateTeamManagementAccessAsync(teamId, allowNgbAdmin: true);
		if (authError != null)
		{
			return authError;
		}

		var team = await this.dbContext.Teams
			.FirstOrDefaultAsync(t => t.Id == teamId.Id, this.HttpContext.RequestAborted);

		if (team == null)
		{
			return this.NotFound();
		}

		team.AutoApprovePlayerRequests = request.IsEnabled;

		if (!request.IsEnabled)
		{
			await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);
			return this.NoContent();
		}

		var currentUserDbId = await this.GetCurrentUserDbIdAsync(userContext.UserId);
		var pendingRequests = await this.dbContext.TeamInvitations
			.Include(invitation => invitation.Initiator)
			.Where(invitation =>
				invitation.TeamId == teamId.Id &&
				invitation.RevokedAt == null &&
				invitation.AcceptedAt == null &&
				invitation.DeclinedAt == null &&
				invitation.Initiator.Email.ToLower() == invitation.Email.ToLower())
			.ToListAsync(this.HttpContext.RequestAborted);

		var usersToNotify = new List<InvitedUserLookup>();
		foreach (var invitation in pendingRequests)
		{
			var invitationEmail = TeamInviteHelpers.NormalizeEmail(invitation.Email);
			var invitedUser = await this.dbContext.Users
				.Where(dbUser => dbUser.Email.ToLower() == invitationEmail)
				.Select(dbUser => new InvitedUserLookup(dbUser.Id, dbUser.Email, dbUser.UniqueId))
				.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

			var respondedAt = DateTime.UtcNow;
			var approvalError = await this.TryApplyApprovedInviteResponseAsync(
				teamId,
				invitation,
				invitedUser,
				currentUserDbId,
				respondedAt);

			if (approvalError != null)
			{
				continue;
			}

			this.RecordInviteResponse(invitation, approved: true, currentUserDbId, respondedAt);
			this.AddInviteResponseActivity(teamId, invitation, invitedUser?.Id, currentUserDbId, approved: true, respondedAt);

			if (invitedUser != null && invitedUser.Id != currentUserDbId)
			{
				usersToNotify.Add(invitedUser);
			}
		}

		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);

		foreach (var invitedUser in usersToNotify.GroupBy(user => user.Id).Select(group => group.First()))
		{
			await this.notificationService.CreateTeamInviteResponseNotificationForPlayerAsync(
				invitedUser.ToUserIdentifier(),
				teamId,
				team.Name,
				approved: true,
				this.HttpContext.RequestAborted);
		}

		return this.NoContent();
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
	public async Task<ActionResult<string>> AddTeamManagerToTeam(
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

		if (result is IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded or IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole)
		{
			var user = await this.dbContext.Users
				.WithEmail(email)
				.Select(u => new { u.Id, u.UniqueId })
				.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

			if (user != null)
			{
				var userId = user.UniqueId != null ? UserIdentifier.Parse(user.UniqueId) : UserIdentifier.FromLegacyUserId(user.Id);
				await this.notificationService.CreateTeamManagerAssignmentNotificationAsync(
					userId,
					teamId,
					cancellationToken: this.HttpContext.RequestAborted);
			}
		}

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
	/// Create a pending invitation for a player to join the team.
	/// </summary>
	[HttpPost("{teamId}/invites")]
	[Tags("TeamManagement")]
	[Authorize]
	public async Task<ActionResult<TeamInvitationViewModel>> InvitePlayer(
		[FromRoute] TeamIdentifier teamId,
		[FromBody] InvitePlayerRequest request)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var authError = await this.ValidateTeamManagementAccessAsync(teamId);
		if (authError != null)
		{
			if (authError is NotFoundResult)
			{
				return this.NotFound();
			}

			return this.Forbid();
		}

		if (!TryNormalizeInviteEmail(request?.Email, out var normalizedEmail))
		{
			return this.BadRequest("Invalid email address");
		}

		if (!await this.TeamExistsAsync(teamId))
		{
			return this.NotFound();
		}

		if (await this.TeamHasMemberWithEmailAsync(teamId, normalizedEmail))
		{
			return this.BadRequest("User is already a team member");
		}

		if (await this.HasPendingInviteAsync(teamId, normalizedEmail))
		{
			return this.BadRequest("A pending invitation already exists for that email address");
		}

		var currentUserDbId = await this.GetCurrentUserDbIdAsync(userContext.UserId);
		var invitation = await this.CreatePendingInviteAsync(teamId, normalizedEmail, currentUserDbId);
		var invitedByName = TeamInviteHelpers.BuildDisplayName(userContext.UserData.FirstName, userContext.UserData.LastName);
		var teamName = await this.GetTeamNameAsync(teamId);
		var invitedUser = await this.dbContext.Users
			.Where(user => user.Email.ToLower() == normalizedEmail)
			.Select(user => new { user.Id, user.UniqueId })
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		await this.TrySendInviteEmailAsync(teamId, invitation.Email, invitedByName);

		if (invitedUser != null && invitedUser.Id != currentUserDbId)
		{
			var invitedUserId = !string.IsNullOrWhiteSpace(invitedUser.UniqueId)
				? UserIdentifier.Parse(invitedUser.UniqueId)
				: UserIdentifier.FromLegacyUserId(invitedUser.Id);
			await this.notificationService.CreateTeamInviteNotificationForPlayerAsync(
				invitedUserId,
				teamId,
				teamName,
				this.HttpContext.RequestAborted);
		}

		return this.CreatedAtAction(
			nameof(GetTeamManagement),
			new { teamId },
			new TeamInvitationViewModel
			{
				InvitationId = invitation.Id.ToString(),
				Email = invitation.Email,
				CreatedAt = invitation.CreatedAt,
				InvitedByName = invitedByName
			});
	}

	/// <summary>
	/// Revoke a pending player invitation for the team.
	/// </summary>
	[HttpDelete("{teamId}/invites/{invitationId:long}")]
	[Tags("TeamManagement")]
	[Authorize]
	public async Task<IActionResult> RevokeInvite(
		[FromRoute] TeamIdentifier teamId,
		[FromRoute] long invitationId)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var authError = await this.ValidateTeamManagementAccessAsync(teamId, allowNgbAdmin: true);
		if (authError != null)
		{
			return authError;
		}

		var invitation = await this.GetPendingTeamInvitationAsync(teamId, invitationId);

		if (invitation == null)
		{
			return this.NotFound();
		}

		var currentUserDbId = await this.GetCurrentUserDbIdAsync(userContext.UserId);
		var revokedAt = DateTime.UtcNow;
		invitation.RevokedAt = revokedAt;
		this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
		{
			TeamId = teamId.Id,
			Email = invitation.Email,
			InitiatorUserId = currentUserDbId,
			ActivityType = TeamPlayerActivityType.InviteRevoked,
			CreatedAt = revokedAt,
		});
		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);

		return this.NoContent();
	}

	/// <summary>
	/// Accept or reject a pending player join request for the team.
	/// </summary>
	[HttpPost("{teamId}/invites/{invitationId:long}/response")]
	[Tags("TeamManagement")]
	[Authorize]
	public async Task<IActionResult> RespondToPendingInvite(
		[FromRoute] TeamIdentifier teamId,
		[FromRoute] long invitationId,
		[FromBody] InviteResponseModel response)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			return this.NotFound();
		}

		var authError = await this.ValidateTeamManagementAccessAsync(teamId, allowNgbAdmin: true);
		if (authError != null)
		{
			return authError;
		}

		var invitation = await this.GetPendingTeamInvitationAsync(teamId, invitationId);

		if (invitation == null)
		{
			return this.NotFound();
		}

		var currentUserDbId = await this.GetCurrentUserDbIdAsync(userContext.UserId);
		var invitationEmail = TeamInviteHelpers.NormalizeEmail(invitation.Email);
		var invitedUser = await this.dbContext.Users
			.Where(user => user.Email.ToLower() == invitationEmail)
			.Select(user => new InvitedUserLookup(user.Id, user.Email, user.UniqueId))
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		var respondedAt = DateTime.UtcNow;

		if (response.Approved)
		{
			var approvalError = await this.TryApplyApprovedInviteResponseAsync(
				teamId,
				invitation,
				invitedUser,
				currentUserDbId,
				respondedAt);

			if (approvalError != null)
			{
				return approvalError;
			}
		}

		this.RecordInviteResponse(invitation, response.Approved, currentUserDbId, respondedAt);
		this.AddInviteResponseActivity(teamId, invitation, invitedUser?.Id, currentUserDbId, response.Approved, respondedAt);

		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);

		if (invitedUser != null && invitedUser.Id != currentUserDbId)
		{
			await this.notificationService.CreateTeamInviteResponseNotificationForPlayerAsync(
				invitedUser.ToUserIdentifier(),
				teamId,
				team.TeamData.Name,
				response.Approved,
				this.HttpContext.RequestAborted);
		}

		return this.NoContent();
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
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var authError = await this.ValidateTeamManagementAccessAsync(teamId);
		if (authError != null)
		{
			return authError;
		}

		var userDbId = await this.dbContext.Users
			.WithIdentifier(playerId)
			.Select(u => (long?)u.Id)
			.FirstOrDefaultAsync();

		if (userDbId == null)
		{
			return this.NotFound("Player not found");
		}

		// Remove the RefereeTeam association
		var membership = await this.dbContext.RefereeTeams
			.Include(rt => rt.Referee)
			.FirstOrDefaultAsync(rt => rt.TeamId == teamId.Id && rt.RefereeId == userDbId);

		if (membership == null)
		{
			return this.NotFound("Player not found on this team");
		}

		if (membership.Referee == null)
		{
			return this.BadRequest("Player details are unavailable");
		}

		var currentUserDbId = await this.GetCurrentUserDbIdAsync(userContext.UserId);
		var removedAt = DateTime.UtcNow;
		this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
		{
			TeamId = teamId.Id,
			UserId = membership.RefereeId,
			Email = membership.Referee.Email,
			InitiatorUserId = currentUserDbId,
			ActivityType = TeamPlayerActivityType.PlayerRemoved,
			CreatedAt = removedAt,
		});
		this.dbContext.RefereeTeams.Remove(membership);
		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);

		return this.NoContent();
	}

	private async Task<IActionResult?> ValidateTeamManagementAccessAsync(TeamIdentifier teamId, bool allowNgbAdmin = false)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var isTeamManager = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (isTeamManager)
		{
			return null;
		}

		if (!allowNgbAdmin)
		{
			return this.Forbid();
		}

		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			return this.NotFound();
		}

		var isNgbAdmin = userContext.Roles
			.OfType<NgbAdminRole>()
			.Any(role => role.Ngb.AppliesTo(team.NgbId));

		return isNgbAdmin ? null : this.Forbid();
	}

	private async Task<long> GetCurrentUserDbIdAsync(UserIdentifier userId)
	{
		return await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(user => user.Id)
			.SingleAsync(this.HttpContext.RequestAborted);
	}

	private static bool TryNormalizeInviteEmail(string? rawEmail, out string normalizedEmail)
	{
		normalizedEmail = string.Empty;
		if (string.IsNullOrWhiteSpace(rawEmail))
		{
			return false;
		}

		if (!Email.TryParse(rawEmail, out _))
		{
			return false;
		}

		normalizedEmail = rawEmail.Trim().ToLowerInvariant();
		return !string.IsNullOrWhiteSpace(normalizedEmail);
	}

	private async Task<bool> TeamExistsAsync(TeamIdentifier teamId)
	{
		return await this.dbContext.Teams.AnyAsync(t => t.Id == teamId.Id, this.HttpContext.RequestAborted);
	}

	private async Task<string> GetTeamNameAsync(TeamIdentifier teamId)
	{
		return await this.dbContext.Teams
			.Where(team => team.Id == teamId.Id)
			.Select(team => team.Name)
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted) ?? teamId.ToString();
	}

	private async Task<bool> TeamHasMemberWithEmailAsync(TeamIdentifier teamId, string normalizedEmail)
	{
		return await this.dbContext.RefereeTeams
			.Where(rt => rt.TeamId == teamId.Id)
			.AnyAsync(rt => rt.Referee != null && rt.Referee.Email.ToLower() == normalizedEmail, this.HttpContext.RequestAborted);
	}

	private async Task<bool> HasPendingInviteAsync(TeamIdentifier teamId, string normalizedEmail)
	{
		return await this.dbContext.TeamInvitations
			.AnyAsync(i =>
				i.TeamId == teamId.Id &&
				i.RevokedAt == null &&
				i.AcceptedAt == null &&
				i.DeclinedAt == null &&
				i.Email.ToLower() == normalizedEmail,
				this.HttpContext.RequestAborted);
	}

	private async Task<ManagementHub.Models.Data.TeamInvitation?> GetPendingTeamInvitationAsync(TeamIdentifier teamId, long invitationId)
	{
		return await this.dbContext.TeamInvitations
			.FirstOrDefaultAsync(
				i =>
					i.Id == invitationId &&
					i.TeamId == teamId.Id &&
					i.RevokedAt == null &&
					i.AcceptedAt == null &&
					i.DeclinedAt == null,
				this.HttpContext.RequestAborted);
	}

	private async Task<IActionResult?> TryApplyApprovedInviteResponseAsync(
		TeamIdentifier teamId,
		ManagementHub.Models.Data.TeamInvitation invitation,
		InvitedUserLookup? invitedUser,
		long currentUserDbId,
		DateTime respondedAt)
	{
		if (invitedUser == null)
		{
			return this.BadRequest("Cannot approve request because the player account was not found.");
		}

		var invitedUserId = invitedUser.Id;
		var existingPlayerMembership = await this.dbContext.RefereeTeams
			.FirstOrDefaultAsync(
				rt =>
					rt.RefereeId == invitedUserId &&
					rt.AssociationType == RefereeTeamAssociationType.Player,
				this.HttpContext.RequestAborted);

		if (existingPlayerMembership == null)
		{
			this.dbContext.RefereeTeams.Add(new ManagementHub.Models.Data.RefereeTeam
			{
				AssociationType = RefereeTeamAssociationType.Player,
				RefereeId = invitedUserId,
				TeamId = teamId.Id,
				CreatedAt = respondedAt,
				UpdatedAt = respondedAt,
			});
			return null;
		}

		if (existingPlayerMembership.TeamId != teamId.Id && existingPlayerMembership.TeamId.HasValue)
		{
			this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
			{
				TeamId = existingPlayerMembership.TeamId.Value,
				UserId = invitedUserId,
				Email = invitation.Email,
				InitiatorUserId = currentUserDbId,
				ActivityType = TeamPlayerActivityType.PlayerRemoved,
				CreatedAt = respondedAt,
			});

			existingPlayerMembership.TeamId = teamId.Id;
			existingPlayerMembership.UpdatedAt = respondedAt;
		}

		return null;
	}

	private void RecordInviteResponse(
		ManagementHub.Models.Data.TeamInvitation invitation,
		bool approved,
		long currentUserDbId,
		DateTime respondedAt)
	{
		invitation.RespondedByUserId = currentUserDbId;
		if (approved)
		{
			invitation.AcceptedAt = respondedAt;
			return;
		}

		invitation.DeclinedAt = respondedAt;
	}

	private void AddInviteResponseActivity(
		TeamIdentifier teamId,
		ManagementHub.Models.Data.TeamInvitation invitation,
		long? invitedUserId,
		long currentUserDbId,
		bool approved,
		DateTime respondedAt)
	{
		this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
		{
			TeamId = teamId.Id,
			UserId = invitedUserId,
			Email = invitation.Email,
			InitiatorUserId = currentUserDbId,
			ActivityType = approved ? TeamPlayerActivityType.InviteAccepted : TeamPlayerActivityType.InviteDeclined,
			CreatedAt = respondedAt,
		});
	}

	private sealed record InvitedUserLookup(long Id, string Email, string? UniqueId)
	{
		public UserIdentifier ToUserIdentifier() =>
			!string.IsNullOrWhiteSpace(this.UniqueId)
				? UserIdentifier.Parse(this.UniqueId)
				: UserIdentifier.FromLegacyUserId(this.Id);
	}

	private async Task<ManagementHub.Models.Data.TeamInvitation> CreatePendingInviteAsync(
		TeamIdentifier teamId,
		string normalizedEmail,
		long currentUserDbId)
	{
		var invitation = new ManagementHub.Models.Data.TeamInvitation
		{
			TeamId = teamId.Id,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			CreatedAt = DateTime.UtcNow,
		};

		this.dbContext.TeamInvitations.Add(invitation);
		this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
		{
			TeamId = teamId.Id,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			ActivityType = TeamPlayerActivityType.InviteCreated,
			CreatedAt = invitation.CreatedAt,
		});

		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);
		return invitation;
	}

	private async Task TrySendInviteEmailAsync(TeamIdentifier teamId, string inviteeEmail, string? invitedByName)
	{
		try
		{
			await this.sendTeamInviteEmail.SendTeamInviteEmailAsync(
				teamId,
				inviteeEmail,
				invitedByName,
				this.GetHostBaseUri(),
				this.HttpContext.RequestAborted);
		}
		catch (Exception ex)
		{
			this.logger.LogError(ex, "Failed to send team invite email for team {TeamId}", SanitizeForLog(teamId.ToString()));
		}
	}

	private static string SanitizeForLog(string value)
	{
		return value
			.Replace("\r", string.Empty)
			.Replace("\n", string.Empty);
	}

	private Uri GetHostBaseUri() => new($"{this.Request.Scheme}://{this.Request.Host}");

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
