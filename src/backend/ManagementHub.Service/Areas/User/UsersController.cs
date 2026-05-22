using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Abstraction.Services;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Areas.Teams;
using ManagementHub.Service.Areas.Tournaments;
using ManagementHub.Service.Services;
using ManagementHub.Storage;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Service.Areas.User;

/// <summary>
/// Actions related to users.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IUpdateUserDataCommand updateUserDataCommand;
	private readonly IUpdateUserAvatarCommand updateUserAvatarCommand;
	private readonly ISetUserAttributeCommand setUserAttributeCommand;
	private readonly IUserDelicateInfoService userDelicateInfoService;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ISendTeamInviteEmail sendTeamInviteEmail;
	private readonly INotificationService notificationService;
	private readonly ManagementHubDbContext dbContext;
	private readonly IContextualOptions<FeatureGates> featureGatesOptions;
	private readonly ILogger<UsersController> logger;

	public UsersController(
		IUserContextAccessor contextAccessor,
		IUpdateUserDataCommand updateUserDataCommand,
		IUpdateUserAvatarCommand updateUserAvatarCommand,
		ISetUserAttributeCommand setUserAttributeCommand,
		IUserDelicateInfoService userDelicateInfoService,
		ITeamContextProvider teamContextProvider,
		ISendTeamInviteEmail sendTeamInviteEmail,
		INotificationService notificationService,
		ManagementHubDbContext dbContext,
		IContextualOptions<FeatureGates> featureGatesOptions,
		ILogger<UsersController> logger)
	{
		this.contextAccessor = contextAccessor;
		this.updateUserDataCommand = updateUserDataCommand;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
		this.setUserAttributeCommand = setUserAttributeCommand;
		this.userDelicateInfoService = userDelicateInfoService;
		this.teamContextProvider = teamContextProvider;
		this.sendTeamInviteEmail = sendTeamInviteEmail;
		this.notificationService = notificationService;
		this.dbContext = dbContext;
		this.featureGatesOptions = featureGatesOptions;
		this.logger = logger;
	}

	/// <summary>
	/// Retrieves identity information about the currently signed-in user.
	/// </summary>
	[HttpGet("me")]
	[Tags("User")]
	public async Task<CurrentUserViewModel> GetCurrentUser()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var userAttributes = await this.contextAccessor.GetUserAttributesAsync(userContext.UserId);

		return new CurrentUserViewModel(userContext, userAttributes);
	}

	/// <summary>
	/// Retrieves feature gates for the currently signed-in user.
	/// </summary>
	[HttpGet("me/featuregates")]
	[Tags("User")]
	public async Task<FeatureGates> GetCurrentUserFeatureGates()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var featureGates = await this.featureGatesOptions.GetAsync(
			new FeatureGatesContext { UserId = userContext.UserId.ToString() },
			this.HttpContext.RequestAborted);

		return featureGates;
	}

	/// <summary>
	/// Retrieves personal information about the currently signed-in user.
	/// </summary>
	[HttpGet("me/info")]
	[Tags("UserInfo")]
	public async Task<UserDataViewModel> GetCurrentUserData()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var userDataContext = await this.contextAccessor.GetUserDataContextAsync(userContext.UserId);
		return new UserDataViewModel(userDataContext, isCurrentUser: true);
	}

	/// <summary>
	/// Retrieves personal information about another user.
	/// </summary>
	[HttpGet("{userId}/info")]
	[Tags("UserInfo")]
	public async Task<UserDataViewModel> GetUserData([FromRoute] UserIdentifier userId)
	{
		var userDataContext = await this.contextAccessor.GetUserDataContextAsync(userId);
		return new UserDataViewModel(userDataContext, isCurrentUser: false);
	}

	/// <summary>
	/// Updates the personal information of the currently signed-in user.
	/// </summary>
	/// <param name="userData">A partial model of user data.</param>
	[HttpPatch("me/info")]
	[Tags("UserInfo")]
	public async Task UpdateCurrentUserData(UserDataViewModel userData)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// TODO: move it to a processor
		await this.updateUserDataCommand.UpdateUserDataAsync(userContext.UserId, (data) =>
		{
			static string? DefaultIfEmpty(string? value, string? defaultValue) => string.IsNullOrWhiteSpace(value) ? defaultValue : value;

			var firstName = DefaultIfEmpty(userData.FirstName, data.FirstName);
			var lastName = DefaultIfEmpty(userData.LastName, data.LastName);
			var bio = userData.Bio ?? data.Bio;
			var pronouns = userData.Pronouns ?? data.Pronouns;
			var showPronouns = userData.ShowPronouns ?? data.ShowPronouns;
			var exportName = userData.ExportName ?? data.ExportName;
			var lang = userData.Language ?? data.UserLang;
			return new ExtendedUserData(data.Email, firstName!, lastName!)
			{
				Bio = bio,
				ExportName = exportName,
				Pronouns = pronouns,
				ShowPronouns = showPronouns,
				UserLang = lang,
			};
		}, this.HttpContext.RequestAborted);
	}

	/// <summary>
	/// Retrieves the url of the avatar of the currently signed-in user.
	/// </summary>
	/// <returns><c>null</c> if user has no avatar, a uri to the image otherwise</returns>
	[HttpGet("me/avatar")]
	[Tags("UserAvatar")]
	[ProducesResponseType(typeof(Uri), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<Uri?> GetCurrentUserAvatar()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var avatarContext = await this.contextAccessor.GetUserAvatarContextAsync(userContext.UserId);
		return avatarContext.AvatarUri;
	}

	/// <summary>
	/// Retrieves the url of the avatar of another user.
	/// </summary>
	/// <returns><c>null</c> if user has no avatar, a uri to the image otherwise</returns>
	[HttpGet("{userId}/avatar")]
	[Tags("UserAvatar")]
	[ProducesResponseType(typeof(Uri), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<Uri?> GetUserAvatar([FromRoute] UserIdentifier userId)
	{
		var avatarContext = await this.contextAccessor.GetUserAvatarContextAsync(userId);
		return avatarContext.AvatarUri;
	}

	/// <summary>
	/// Updates the avatar of the currently signed-in user.
	/// </summary>
	/// <param name="avatarBlob">Avatar image file contents (streamable).</param>
	/// <returns>A url to download the uploaded avatar from.</returns>
	[HttpPut("me/avatar")]
	[Tags("UserAvatar")]
	public async Task<Uri> UpdateCurrentUserAvatar(IFormFile avatarBlob)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		// TODO: move it to a processor
		var avatarUri = await this.updateUserAvatarCommand.UpdateUserAvatarAsync(
			userContext.UserId,
			avatarBlob.ContentType,
			avatarBlob.OpenReadStream(),
			this.HttpContext.RequestAborted);
		return avatarUri;
	}

	/// <summary>
	/// Sets a root attribute on the users account.
	/// Those can be used by the Management Hub to make decisions about showing the user certain experiences or not.
	/// </summary>
	[HttpPut("{userId}/attributes/root/{key}")]
	[Tags("User")]
	[Authorize(AuthorizationPolicies.TechAdminPolicy)] // TODO: IQA admin?
	public async Task PutRootUserAttribute(
		[FromRoute] UserIdentifier userId,
		[FromRoute] string key,
		[FromBody] JsonDocument attribute)
	{
		await this.setUserAttributeCommand.SetRootUserAttributeAsync(userId, key, attribute, this.HttpContext.RequestAborted);
	}

	/// <summary>
	/// Sets an NGB owned attribute on the users account.
	/// Those attributes can be used by NGB to correlate data from their own systems, or to aid managing referees.
	/// Attribute value has 4kB limit (any valid JSON object).
	/// </summary>
	[HttpPut("{userId}/attributes/{ngb}/{key}")]
	[Tags("User")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)] // TODO: require higher permissions here
	public async Task PutUserAttribute(
		[FromRoute] UserIdentifier userId,
		[FromRoute] NgbIdentifier ngb,
		[FromRoute][MaxLength(128)] string key,
		[FromBody] JsonDocument attribute)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		var refereeViewerRole = userContext.Roles.OfType<RefereeViewerRole>().First();
		if (!refereeViewerRole.Ngb.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		await this.setUserAttributeCommand.SetUserAttributeAsync(userId, ngb, key, attribute, this.HttpContext.RequestAborted);
	}

	// Phase 4: User gender management

	/// <summary>
	/// Get current user's gender data and tournaments where it is referenced.
	/// </summary>
	[HttpGet("me/gender")]
	[Tags("User")]
	public async Task<UserGenderViewModel> GetMyGender()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		// Single query to get gender and tournaments where user is rostered as a player
		var result = await this.dbContext.Users
			.WithIdentifier(currentUser.UserId)
			.GroupJoin(
				this.dbContext.UserDelicateInfos,
				u => u.Id,
				udi => udi.UserId,
				(u, genders) => new
				{
					UserId = u.Id,
					Gender = genders.Select(g => g.Gender).FirstOrDefault()
				})
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		var tournaments = new List<TournamentReferenceViewModel>();

		if (result != null)
		{
			// Get tournaments where this user is on a roster as a player
			tournaments = await this.dbContext.TournamentTeamRosterEntries
				.Where(entry => entry.UserId == result.UserId && entry.Role == RosterRole.Player)
				.Join(
					this.dbContext.TournamentTeamParticipants,
					entry => entry.TournamentTeamParticipantId,
					participant => participant.Id,
					(entry, participant) => participant.Tournament)
				.Select(t => new TournamentReferenceViewModel
				{
					Id = t.UniqueId,
					Name = t.Name,
					StartDate = t.StartDate,
					EndDate = t.EndDate
				})
				.ToListAsync(this.HttpContext.RequestAborted);
		}

		return new UserGenderViewModel
		{
			Gender = result?.Gender,
			ReferencedInTournaments = tournaments
		};
	}

	/// <summary>
	/// Delete current user's gender data.
	/// </summary>
	[HttpDelete("me/gender")]
	[Tags("User")]
	public async Task<IActionResult> DeleteMyGender()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		// Single query to delete gender data using join with Users.WithIdentifier
		await this.dbContext.UserDelicateInfos
			.Join(
				this.dbContext.Users.WithIdentifier(currentUser.UserId),
				udi => udi.UserId,
				u => u.Id,
				(udi, u) => udi)
			.ExecuteDeleteAsync(this.HttpContext.RequestAborted);

		return this.Ok();
	}

	/// <summary>
	/// Get teams managed by the current user.
	/// Returns team IDs, team names, and NGB country codes for all teams the user is an explicit manager of.
	/// </summary>
	[HttpGet("me/managedTeams")]
	[Tags("User")]
	public async Task<List<ManagedTeamViewModel>> GetManagedTeams()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		// Get teams where user is an explicit team manager
		var explicitManagedTeams = await this.dbContext.TeamManagers
			.Join(
				this.dbContext.Users.WithIdentifier(currentUser.UserId),
				tm => tm.UserId,
				u => u.Id,
				(tm, u) => tm)
			.Join(
				this.dbContext.Teams,
				tm => tm.TeamId,
				t => t.Id,
				(tm, t) => new { tm, t })
			.Join(
				this.dbContext.NationalGoverningBodies,
				combined => combined.t.NationalGoverningBodyId,
				ngb => ngb.Id,
				(combined, ngb) => new ManagedTeamViewModel
				{
					TeamId = new TeamIdentifier(combined.t.Id),
					TeamName = combined.t.Name,
					Ngb = new NgbIdentifier(ngb.CountryCode),
					GroupAffiliation = combined.t.GroupAffiliation
				})
			.ToListAsync(this.HttpContext.RequestAborted);

		return explicitManagedTeams;
	}

	/// <summary>
	/// Get pending team invitations for the currently signed-in user.
	/// </summary>
	[HttpGet("me/teamInvites")]
	[Tags("User")]
	public async Task<List<CurrentUserTeamInviteViewModel>> GetMyTeamInvites()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
		var currentUserDbId = await this.GetCurrentUserDbIdAsync(currentUser.UserId);
		var normalizedEmail = TeamInviteHelpers.NormalizeEmail(currentUser.UserData.Email.Value);

		var pendingInvites = await this.GetPendingTeamInvitationsForEmailAsync(normalizedEmail);
		var teamLogoUris = await this.GetTeamLogoUrisAsync(pendingInvites.Select(invite => invite.TeamId));

		return pendingInvites
			.Select(invite => new CurrentUserTeamInviteViewModel
			{
				InvitationId = invite.Id.ToString(),
				TeamId = new TeamIdentifier(invite.TeamId),
				TeamName = invite.Team.Name,
				TeamLogoUri = teamLogoUris.GetValueOrDefault(invite.TeamId),
				Email = invite.Email,
				CreatedAt = invite.CreatedAt,
				InvitedByName = TeamInviteHelpers.BuildDisplayName(invite.Initiator.FirstName, invite.Initiator.LastName),
				CanRespond = invite.InitiatorUserId != currentUserDbId
			})
			.ToList();
	}

	/// <summary>
	/// Cancel a pending team join request that the current user initiated.
	/// </summary>
	[HttpDelete("me/teamInvites/{invitationId:long}")]
	[Tags("User")]
	public async Task<IActionResult> CancelMyTeamInvite([FromRoute] long invitationId)
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
		var currentUserDbId = await this.GetCurrentUserDbIdAsync(currentUser.UserId);
		var normalizedEmail = TeamInviteHelpers.NormalizeEmail(currentUser.UserData.Email.Value);

		// Only allow cancelling requests the player themselves initiated (CanRespond == false case)
		var invitation = await this.GetPendingTeamInvitationAsync(invitationId, normalizedEmail);

		if (invitation == null || invitation.InitiatorUserId != currentUserDbId)
		{
			return this.NotFound();
		}

		invitation.RevokedAt = DateTime.UtcNow;
		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);
		return this.NoContent();
	}

	/// <summary>
	/// Accept or decline a pending team invitation for the current user.
	/// </summary>
	[HttpPost("me/teamInvites/{invitationId:long}")]
	[Tags("User")]
	public async Task<IActionResult> RespondToTeamInvite([FromRoute] long invitationId, [FromBody] InviteResponseModel response)
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
		var currentUserDbId = await this.GetCurrentUserDbIdAsync(currentUser.UserId);
		var normalizedEmail = TeamInviteHelpers.NormalizeEmail(currentUser.UserData.Email.Value);

		var invitation = await this.GetPendingTeamInvitationAsync(invitationId, normalizedEmail);

		if (invitation == null)
		{
			return this.NotFound(new { error = "No pending invite found" });
		}

		var canSelfApproveInvite = currentUser.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(new TeamIdentifier(invitation.TeamId)));

		if (RequiresManagerApproval(invitation, currentUserDbId, canSelfApproveInvite))
		{
			return this.BadRequest(new { error = "This request is waiting for team manager approval." });
		}

		var existingPlayerMembership = await this.GetExistingPlayerMembershipAsync(currentUserDbId);

		if (response.Approved && existingPlayerMembership?.TeamId == invitation.TeamId)
		{
			return this.BadRequest(new { error = "User is already a team member" });
		}

		var respondedAt = DateTime.UtcNow;
		this.ApplyTeamInviteResponse(invitation, response.Approved, currentUserDbId, existingPlayerMembership, respondedAt);
		this.AddTeamInviteActivity(invitation, response.Approved, currentUserDbId, respondedAt);

		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);

		var responderName = string.Join(" ", new[] { currentUser.UserData.FirstName, currentUser.UserData.LastName }
			.Where(part => !string.IsNullOrWhiteSpace(part)));

		await this.SendTeamInviteResponseEmailAsync(invitation, invitationId, response.Approved, responderName);

		if (invitation.InitiatorUserId != currentUserDbId)
		{
			var initiatorUser = await this.dbContext.Users
				.Where(user => user.Id == invitation.InitiatorUserId)
				.Select(user => new { user.Id, user.UniqueId })
				.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

			if (initiatorUser != null)
			{
				var initiatorUserId = !string.IsNullOrWhiteSpace(initiatorUser.UniqueId)
					? UserIdentifier.Parse(initiatorUser.UniqueId)
					: UserIdentifier.FromLegacyUserId(initiatorUser.Id);

				await this.notificationService.CreateTeamInviteResponseNotificationForManagerAsync(
					initiatorUserId,
					new TeamIdentifier(invitation.TeamId),
					invitation.Team.Name,
					response.Approved,
					this.HttpContext.RequestAborted);
			}
		}

		return this.Ok();
	}

	private async Task<TeamInvitation?> GetPendingTeamInvitationAsync(long invitationId, string normalizedEmail)
	{
		return await this.dbContext.TeamInvitations
			.Include(invite => invite.Team)
			.FirstOrDefaultAsync(invite =>
				invite.Id == invitationId &&
				invite.Email.ToLower() == normalizedEmail &&
				invite.RevokedAt == null &&
				invite.AcceptedAt == null &&
				invite.DeclinedAt == null,
				this.HttpContext.RequestAborted);
	}
	private async Task<List<TeamInvitation>> GetPendingTeamInvitationsForEmailAsync(string normalizedEmail)
	{
		return await this.dbContext.TeamInvitations
			.Include(invite => invite.Team)
			.Include(invite => invite.Initiator)
			.Where(invite => invite.Email.ToLower() == normalizedEmail && invite.RevokedAt == null && invite.AcceptedAt == null && invite.DeclinedAt == null)
			.OrderByDescending(invite => invite.CreatedAt)
			.ToListAsync(this.HttpContext.RequestAborted);
	}
	private static bool RequiresManagerApproval(TeamInvitation invitation, long currentUserDbId, bool canSelfApproveInvite)
	{
		if (invitation.InitiatorUserId != currentUserDbId)
		{
			return false;
		}

		return !canSelfApproveInvite;
	}

	private async Task<RefereeTeam?> GetExistingPlayerMembershipAsync(long currentUserDbId)
	{
		return await this.dbContext.RefereeTeams
			.FirstOrDefaultAsync(
				membership =>
					membership.RefereeId == currentUserDbId &&
					membership.AssociationType == RefereeTeamAssociationType.Player,
				this.HttpContext.RequestAborted);
	}

	private void ApplyTeamInviteResponse(TeamInvitation invitation, bool approved, long currentUserDbId, RefereeTeam? existingPlayerMembership, DateTime respondedAt)
	{
		invitation.RespondedByUserId = currentUserDbId;

		if (!approved)
		{
			invitation.DeclinedAt = respondedAt;
			return;
		}

		invitation.AcceptedAt = respondedAt;
		if (existingPlayerMembership == null)
		{
			this.dbContext.RefereeTeams.Add(new RefereeTeam
			{
				AssociationType = RefereeTeamAssociationType.Player,
				RefereeId = currentUserDbId,
				TeamId = invitation.TeamId,
				CreatedAt = respondedAt,
				UpdatedAt = respondedAt,
			});
			return;
		}

		if (existingPlayerMembership.TeamId != invitation.TeamId && existingPlayerMembership.TeamId.HasValue)
		{
			this.dbContext.TeamPlayerActivities.Add(new TeamPlayerActivity
			{
				TeamId = existingPlayerMembership.TeamId.Value,
				UserId = currentUserDbId,
				Email = invitation.Email,
				InitiatorUserId = currentUserDbId,
				ActivityType = TeamPlayerActivityType.PlayerRemoved,
				CreatedAt = respondedAt,
			});
		}

		existingPlayerMembership.TeamId = invitation.TeamId;
		existingPlayerMembership.UpdatedAt = respondedAt;
	}

	private void AddTeamInviteActivity(TeamInvitation invitation, bool approved, long currentUserDbId, DateTime respondedAt)
	{
		this.dbContext.TeamPlayerActivities.Add(new TeamPlayerActivity
		{
			TeamId = invitation.TeamId,
			UserId = currentUserDbId,
			Email = invitation.Email,
			InitiatorUserId = currentUserDbId,
			ActivityType = approved ? TeamPlayerActivityType.InviteAccepted : TeamPlayerActivityType.InviteDeclined,
			CreatedAt = respondedAt,
		});
	}

	private async Task SendTeamInviteResponseEmailAsync(TeamInvitation invitation, long invitationId, bool approved, string responderName)
	{
		try
		{
			await this.sendTeamInviteEmail.SendTeamInviteResponseEmailAsync(
				new TeamIdentifier(invitation.TeamId),
				invitation.Email,
				string.IsNullOrWhiteSpace(responderName) ? null : responderName,
				approved,
				this.GetHostBaseUri(),
				this.HttpContext.RequestAborted);
		}
		catch (Exception ex)
		{
			this.logger.LogError(ex, "Failed to send team invite response email for invitation {InvitationId}", invitationId);
		}
	}

	private async Task<long> GetCurrentUserDbIdAsync(UserIdentifier userId)
	{
		return await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(user => user.Id)
			.SingleAsync(this.HttpContext.RequestAborted);
	}

	private Uri GetHostBaseUri() => new($"{this.Request.Scheme}://{this.Request.Host}");

	/// <summary>
	/// Get team transfer history for the currently signed-in user.
	/// Includes team joins and leaves ordered from newest to oldest.
	/// </summary>
	[HttpGet("me/teamHistory")]
	[Tags("User")]
	public async Task<List<TeamPlayerActivityViewModel>> GetMyTeamHistory()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
		var currentUserDbId = await this.GetCurrentUserDbIdAsync(currentUser.UserId);

		var historyRows = await this.dbContext.TeamPlayerActivities
			.Where(activity =>
				activity.UserId == currentUserDbId &&
				(activity.ActivityType == TeamPlayerActivityType.InviteAccepted || activity.ActivityType == TeamPlayerActivityType.PlayerRemoved))
			.OrderByDescending(activity => activity.CreatedAt)
			.Take(50)
			.Select(activity => new
			{
				activity.TeamId,
				TeamName = activity.Team.Name,
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
			.ToListAsync(this.HttpContext.RequestAborted);

		var teamLogoUris = await this.GetTeamLogoUrisAsync(historyRows.Select(activity => activity.TeamId));

		return historyRows
			.Select(activity => new TeamPlayerActivityViewModel
			{
				TeamId = new TeamIdentifier(activity.TeamId),
				TeamName = activity.TeamName,
				TeamLogoUri = teamLogoUris.GetValueOrDefault(activity.TeamId),
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
	}

	/// <summary>
	/// Get team transfer history for a specific user (authorized for managers viewing team members).
	/// Includes team joins and leaves ordered from newest to oldest.
	/// </summary>
	[HttpGet("{userId}/teamHistory")]
	[Tags("User")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task<List<TeamPlayerActivityViewModel>> GetUserTeamHistory([FromRoute] UserIdentifier userId)
	{
		var targetUserDbId = await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(u => (long?)u.Id)
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		if (targetUserDbId == null)
		{
			return new List<TeamPlayerActivityViewModel>();
		}

		var historyRows = await this.dbContext.TeamPlayerActivities
			.Where(activity =>
				activity.UserId == targetUserDbId &&
				(activity.ActivityType == TeamPlayerActivityType.InviteAccepted || activity.ActivityType == TeamPlayerActivityType.PlayerRemoved))
			.OrderByDescending(activity => activity.CreatedAt)
			.Take(50)
			.Select(activity => new
			{
				activity.TeamId,
				TeamName = activity.Team.Name,
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
			.ToListAsync(this.HttpContext.RequestAborted);

		var teamLogoUris = await this.GetTeamLogoUrisAsync(historyRows.Select(activity => activity.TeamId));

		return historyRows
			.Select(activity => new TeamPlayerActivityViewModel
			{
				TeamId = new TeamIdentifier(activity.TeamId),
				TeamName = activity.TeamName,
				TeamLogoUri = teamLogoUris.GetValueOrDefault(activity.TeamId),
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
	}

	private async Task<Dictionary<long, string?>> GetTeamLogoUrisAsync(IEnumerable<long> teamIds)
	{
		var logoUris = new Dictionary<long, string?>();
		foreach (var teamId in teamIds.Distinct())
		{
			var logoUri = await this.teamContextProvider.GetTeamLogoUriAsync(new TeamIdentifier(teamId), this.HttpContext.RequestAborted);
			logoUris[teamId] = logoUri?.ToString();
		}

		return logoUris;
	}

	/// <summary>
	/// Get upcoming tournaments for the currently signed-in user based on team roster entries.
	/// Returns tournaments where the user is on a team roster and the tournament start date is today or in the future.
	/// </summary>
	[HttpGet("me/upcomingTournaments")]
	[Tags("User")]
	public async Task<List<TournamentReferenceViewModel>> GetMyUpcomingTournaments()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		var dbUserId = await this.dbContext.Users
			.WithIdentifier(currentUser.UserId)
			.Select(u => u.Id)
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		if (dbUserId == 0)
		{
			return [];
		}

		return await this.dbContext.TournamentTeamRosterEntries
			.Where(entry => entry.UserId == dbUserId)
			.Select(entry => entry.Participant.Tournament)
			.Where(t => t.StartDate >= today)
			.Distinct()
			.OrderBy(t => t.StartDate)
			.Select(t => new TournamentReferenceViewModel
			{
				Id = t.UniqueId,
				Name = t.Name,
				StartDate = t.StartDate,
				EndDate = t.EndDate
			})
			.ToListAsync(this.HttpContext.RequestAborted);
	}
}
