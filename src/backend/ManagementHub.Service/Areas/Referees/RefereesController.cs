using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Service.Services;
using ManagementHub.Storage;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Referees;

/// <summary>
/// Actions related to users with the referee role.
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class RefereesController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IUpdateRefereeRoleCommand updateRefereeRoleCommand;
	private readonly IRefereeContextAccessor refereeContextAccessor;
	private readonly IUpdateUserDataCommand updateUserDataCommand;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly INotificationService notificationService;
	private readonly ICreateTeamInviteRequestCommand createTeamInviteRequestCommand;
	private readonly ManagementHubDbContext dbContext;

	public RefereesController(
		IUserContextAccessor contextAccessor,
		IUpdateRefereeRoleCommand updateRefereeRoleCommand,
		IRefereeContextAccessor refereeContextAccessor,
		IUpdateUserDataCommand updateUserDataCommand,
		ITeamContextProvider teamContextProvider,
		INotificationService notificationService,
		ICreateTeamInviteRequestCommand createTeamInviteRequestCommand,
		ManagementHubDbContext dbContext)
	{
		this.contextAccessor = contextAccessor;
		this.updateRefereeRoleCommand = updateRefereeRoleCommand;
		this.refereeContextAccessor = refereeContextAccessor;
		this.updateUserDataCommand = updateUserDataCommand;
		this.teamContextProvider = teamContextProvider;
		this.notificationService = notificationService;
		this.createTeamInviteRequestCommand = createTeamInviteRequestCommand;
		this.dbContext = dbContext;
	}

	/// <summary>
	/// Updates the referees metadata (Ngb, Team).
	/// </summary>
	[HttpPut("me")]
	[Tags("Referee", "User")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<IActionResult> UpdateCurrentReferee([FromBody] RefereeUpdateViewModel refereeUpdate)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var (currentPlayingTeamId, currentCoachingTeamId) = await this.GetCurrentRefereeTeamIdsAsync(userContext.UserId);

		var requestedPlayingTeamId = refereeUpdate.PlayingTeam?.Id.Id;
		var requestedCoachingTeamId = refereeUpdate.CoachingTeam?.Id.Id;
		var normalizedEmail = userContext.UserData.Email.Value.Trim().ToLowerInvariant();
		var requestedTeamIds = new[] { requestedPlayingTeamId, requestedCoachingTeamId }
			.Where(teamId => teamId != null)
			.Select(teamId => teamId!.Value)
			.Distinct()
			.ToArray();
		var teamIdsWithInviteRecords = await this.GetTeamIdsWithInviteRecordAsync(requestedTeamIds, normalizedEmail);
		
		var shouldCreatePlayingTeamRequest = ShouldCreateTeamRequest(
			requestedPlayingTeamId, currentPlayingTeamId, teamIdsWithInviteRecords);
		var shouldCreateCoachingTeamRequest = ShouldCreateTeamRequest(
			requestedCoachingTeamId, currentCoachingTeamId, teamIdsWithInviteRecords);

		await this.updateRefereeRoleCommand.UpdateRefereeRoleAsync(userContext.UserId, refereeRole => new RefereeRole
		{
			IsActive = refereeRole.IsActive,
			CoachingTeam = refereeUpdate.CoachingTeam?.Id,
			// Keep existing membership unless explicitly clearing; non-null requests are handled through invite workflow below.
			PlayingTeam = refereeUpdate.PlayingTeam == null ? null : refereeRole.PlayingTeam,
			NationalTeam = refereeUpdate.NationalTeam?.Id,
			PrimaryNgb = refereeUpdate.PrimaryNgb,
			SecondaryNgb = refereeUpdate.SecondaryNgb,
		}, this.HttpContext.RequestAborted);

		if (!shouldCreatePlayingTeamRequest && !shouldCreateCoachingTeamRequest)
		{
			return this.NoContent();
		}

		var (playingError, playingStatus) = await this.TryProcessTeamRequestAsync(
			requestedPlayingTeamId,
			shouldCreatePlayingTeamRequest,
			normalizedEmail,
			userContext.UserId,
			RefereeTeamAssociationType.Player);
		if (playingError != null)
		{
			return playingError;
		}

		var (coachingError, coachingStatus) = await this.TryProcessTeamRequestAsync(
			requestedCoachingTeamId,
			shouldCreateCoachingTeamRequest,
			normalizedEmail,
			userContext.UserId,
			RefereeTeamAssociationType.Coach);
		if (coachingError != null)
		{
			return coachingError;
		}

		return this.Ok(new RefereeTeamUpdateStatusViewModel
		{
			PlayingTeam = playingStatus,
			CoachingTeam = coachingStatus,
		});
	}

	private static bool ShouldCreateTeamRequest(long? requestedId, long? currentId, HashSet<long> idsWithInvites)
	{
		if (requestedId == null)
		{
			return false;
		}

		return currentId == null
			|| requestedId.Value != currentId.Value
			|| !idsWithInvites.Contains(requestedId.Value);
	}

	private async Task<(IActionResult? Error, RefereeTeamRequestStatusViewModel? Status)> TryProcessTeamRequestAsync(
		long? requestedTeamId,
		bool shouldCreate,
		string normalizedEmail,
		UserIdentifier currentUserId,
		RefereeTeamAssociationType requestedAssociationType)
	{
		if (requestedTeamId == null)
		{
			return (null, null);
		}

		if (!shouldCreate)
		{
			return (null, new RefereeTeamRequestStatusViewModel
			{
				TeamId = new TeamIdentifier(requestedTeamId.Value).ToString(),
				Status = RefereeTeamRequestStatus.Applied,
				RequestCreated = false,
			});
		}

		var result = await this.CreateOrUpdateTeamInviteAsync(
			requestedTeamId.Value,
			normalizedEmail,
			currentUserId,
			requestedAssociationType);
		return (result.ErrorResult, result.Status);
	}

	private async Task<(long? PlayingTeamId, long? CoachingTeamId)> GetCurrentRefereeTeamIdsAsync(UserIdentifier userId)
	{
		var teamRows = await this.dbContext.Users
			.WithIdentifier(userId)
			.SelectMany(user => this.dbContext.RefereeTeams
				.Where(team => team.RefereeId == user.Id &&
					(team.AssociationType == RefereeTeamAssociationType.Player || team.AssociationType == RefereeTeamAssociationType.Coach))
				.Select(team => new
				{
					team.AssociationType,
					team.TeamId,
				}))
			.ToListAsync(this.HttpContext.RequestAborted);

		var playingTeamId = teamRows
			.Where(team => team.AssociationType == RefereeTeamAssociationType.Player)
			.Select(team => team.TeamId)
			.FirstOrDefault();
		var coachingTeamId = teamRows
			.Where(team => team.AssociationType == RefereeTeamAssociationType.Coach)
			.Select(team => team.TeamId)
			.FirstOrDefault();

		return (
			playingTeamId == 0 ? null : playingTeamId,
			coachingTeamId == 0 ? null : coachingTeamId);
	}

	private async Task<HashSet<long>> GetTeamIdsWithInviteRecordAsync(IEnumerable<long> teamIds, string normalizedEmail)
	{
		var requestedTeamIds = teamIds.ToArray();
		if (requestedTeamIds.Length == 0)
		{
			return [];
		}

		var teamIdsWithInvites = await this.dbContext.TeamInvitations
			.Where(invite =>
				requestedTeamIds.Contains(invite.TeamId) &&
				invite.Email.ToLower() == normalizedEmail)
			.Select(invite => invite.TeamId)
			.Distinct()
			.ToListAsync(this.HttpContext.RequestAborted);

		return teamIdsWithInvites.ToHashSet();
	}

	/// <summary>
	/// Delegates invite-request creation to the command layer and handles notification dispatch.
	/// </summary>
	private async Task<TeamInviteRequestResult> CreateOrUpdateTeamInviteAsync(
		long requestedTeamId,
		string normalizedEmail,
		UserIdentifier currentUserId,
		RefereeTeamAssociationType requestedAssociationType)
	{
		var teamId = new TeamIdentifier(requestedTeamId);
		var commandResult = await this.createTeamInviteRequestCommand.CreateTeamInviteRequestAsync(
			teamId,
			normalizedEmail,
			currentUserId,
			requestedAssociationType,
			this.HttpContext.RequestAborted);

		return commandResult.Code switch
		{
			ICreateTeamInviteRequestCommand.CreateResultCode.TeamNotFound =>
				new TeamInviteRequestResult { ErrorResult = this.BadRequest("Selected team was not found.") },

			ICreateTeamInviteRequestCommand.CreateResultCode.AlreadyPending =>
				new TeamInviteRequestResult
				{
					Status = new RefereeTeamRequestStatusViewModel
					{
						TeamId = teamId.ToString(),
						Status = RefereeTeamRequestStatus.Pending,
						RequestCreated = false,
					},
				},

			ICreateTeamInviteRequestCommand.CreateResultCode.AutoApproved =>
				new TeamInviteRequestResult
				{
					Status = new RefereeTeamRequestStatusViewModel
					{
						TeamId = teamId.ToString(),
						Status = RefereeTeamRequestStatus.Applied,
						RequestCreated = true,
					},
				},

			ICreateTeamInviteRequestCommand.CreateResultCode.RequestCreated =>
				await this.NotifyApproversAndBuildResultAsync(teamId, commandResult, currentUserId),

			_ => new TeamInviteRequestResult { ErrorResult = this.StatusCode(500) },
		};
	}

	private async Task<TeamInviteRequestResult> NotifyApproversAndBuildResultAsync(
		TeamIdentifier teamId,
		ICreateTeamInviteRequestCommand.CreateResult commandResult,
		UserIdentifier currentUserId)
	{
		var resolvedTeamName = commandResult.TeamName ?? teamId.ToString();
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);
		foreach (var manager in managers.Where(m => m.UserId != currentUserId))
		{
			await this.notificationService.CreateTeamInviteRequestNotificationForManagerAsync(
				manager.UserId,
				teamId,
				resolvedTeamName,
				this.HttpContext.RequestAborted);
		}

		if (commandResult.InvitationId.HasValue && commandResult.PendingNgbApprovals is { Count: > 0 })
		{
			await this.notificationService.CreateNgbTransferApprovalNotificationsAsync(
				commandResult.InvitationId.Value,
				commandResult.PendingNgbApprovals,
				this.HttpContext.RequestAborted);
		}

		return new TeamInviteRequestResult
		{
			Status = new RefereeTeamRequestStatusViewModel
			{
				TeamId = teamId.ToString(),
				Status = RefereeTeamRequestStatus.Pending,
				RequestCreated = true,
			},
		};
	}

	private sealed class TeamInviteRequestResult
	{
		public IActionResult? ErrorResult { get; init; }

		public RefereeTeamRequestStatusViewModel? Status { get; init; }
	}

	/// <summary>
	/// Get the referee profile for the current user.
	/// </summary>
	[HttpGet("me")]
	[Tags("Referee", "UserInfo")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<RefereeViewModel> GetCurrentReferee()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var context = await this.refereeContextAccessor.GetRefereeViewContextForCurrentUserAsync();
		return MapRefereeViewContextToViewModel(context, GetViewerPerimissionConstraint(userContext));
	}

	/// <summary>
	/// Get the referee profile for another user.
	/// </summary>
	[HttpGet("{userId}")]
	[Tags("Referee", "UserInfo")]
	[Authorize]
	public async Task<RefereeViewModel> GetReferee([FromRoute] UserIdentifier userId)
	{
		if (userId == default)
		{
			throw new ArgumentException("User identifier has not been provided.", nameof(userId));
		}

		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var context = await this.refereeContextAccessor.GetRefereeViewContextAsync(userId);
		return MapRefereeViewContextToViewModel(context, GetViewerPerimissionConstraint(userContext));
	}

	/// <summary>
	/// Gets the referee profiles for all users (limited by viewer permissions).
	/// </summary>
	[HttpGet]
	[Tags("Referee")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)]
	public async Task<Filtered<RefereeViewModel>> GetReferees([FromQuery] FilteringParameters filtering)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var collection = await this.refereeContextAccessor.GetRefereeViewContextListAsync();
		var viewerPermissionConstraint = GetViewerPerimissionConstraint(userContext);
		return collection.Select(x => MapRefereeViewContextToViewModel(x, viewerPermissionConstraint)).AsFiltered();
	}

	/// <summary>
	/// Gets the referee profiles for all users from a given NGB (limited by viewer permissions).
	/// </summary>
	[HttpGet("/api/v2/Ngbs/{ngb}/referees")]
	[Tags("Referee")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)]
	public async Task<Filtered<RefereeViewModel>> GetNgbReferees([FromRoute] NgbIdentifier ngb, [FromQuery] FilteringParameters filtering)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var collection = await this.refereeContextAccessor.GetRefereeViewContextListAsync(ngb);
		var viewerPermissionConstraint = GetViewerPerimissionConstraint(userContext);
		return collection.Select(x => MapRefereeViewContextToViewModel(x, viewerPermissionConstraint)).AsFiltered();
	}

	/// <summary>
	/// Updates a referee's name (admin operation - no NGB scope restrictions).
	/// </summary>
	[HttpPatch("{userId}/name")]
	[Tags("Referee", "UserInfo")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateRefereeNameAdmin(
		[FromRoute] UserIdentifier userId,
		[FromBody] UpdateRefereeNameRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName))
		{
			return this.BadRequest("At least one of FirstName or LastName must be provided.");
		}

		// Verify the user exists
		var userExists = await this.dbContext.Users.WithIdentifier(userId)
			.AnyAsync(u => true, this.HttpContext.RequestAborted);

		if (!userExists)
		{
			return this.NotFound();
		}

		await this.updateUserDataCommand.UpdateUserDataAsync(userId, data =>
		{
			var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? data.FirstName : request.FirstName;
			var lastName = string.IsNullOrWhiteSpace(request.LastName) ? data.LastName : request.LastName;
			return new ManagementHub.Models.Domain.User.ExtendedUserData(data.Email, firstName, lastName)
			{
				Bio = data.Bio,
				ExportName = data.ExportName,
				Pronouns = data.Pronouns,
				ShowPronouns = data.ShowPronouns,
				UserLang = data.UserLang,
			};
		}, this.HttpContext.RequestAborted);

		return this.NoContent();
	}

	private static NgbConstraint GetViewerPerimissionConstraint(IUserContext userContext) =>
		userContext.Roles.OfType<RefereeViewerRole>().FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();

	private static RefereeViewModel MapRefereeViewContextToViewModel(IRefereeViewContext context, NgbConstraint viewerPermissionSet)
	{
		return new RefereeViewModel
		{
			AcquiredCertifications = context.AcquiredCertifications,
			CoachingTeam = context.CoachingTeam == null ? null : new TeamIndicator
			{
				Id = context.CoachingTeam.Value,
				Name = context.TeamContext[context.CoachingTeam.Value].TeamData.Name,
			},
			Name = context.DisplayName,
			PlayingTeam = context.PlayingTeam == null ? null : new TeamIndicator
			{
				Id = context.PlayingTeam.Value,
				Name = context.TeamContext[context.PlayingTeam.Value].TeamData.Name,
			},
			NationalTeam = context.NationalTeam == null ? null : new TeamIndicator
			{
				Id = context.NationalTeam.Value,
				Name = context.TeamContext[context.NationalTeam.Value].TeamData.Name,
			},
			PrimaryNgb = context.PrimaryNgb,
			SecondaryNgb = context.SecondaryNgb,
			UserId = context.UserId,
			Attributes = context.Attributes.GetPrefixedByConstraint(viewerPermissionSet),
		};
	}
}
