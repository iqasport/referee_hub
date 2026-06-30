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
	private readonly ManagementHubDbContext dbContext;

	public RefereesController(
		IUserContextAccessor contextAccessor,
		IUpdateRefereeRoleCommand updateRefereeRoleCommand,
		IRefereeContextAccessor refereeContextAccessor,
		IUpdateUserDataCommand updateUserDataCommand,
		ITeamContextProvider teamContextProvider,
		INotificationService notificationService,
		ManagementHubDbContext dbContext)
	{
		this.contextAccessor = contextAccessor;
		this.updateRefereeRoleCommand = updateRefereeRoleCommand;
		this.refereeContextAccessor = refereeContextAccessor;
		this.updateUserDataCommand = updateUserDataCommand;
		this.teamContextProvider = teamContextProvider;
		this.notificationService = notificationService;
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
		
		var hasAnyPlayingTeamInviteRecord = requestedPlayingTeamId != null
			&& teamIdsWithInviteRecords.Contains(requestedPlayingTeamId.Value);
		var shouldCreatePlayingTeamRequest =
			requestedPlayingTeamId != null &&
			(currentPlayingTeamId == null || requestedPlayingTeamId != currentPlayingTeamId.Value || !hasAnyPlayingTeamInviteRecord);

		var hasAnyCoachingTeamInviteRecord = requestedCoachingTeamId != null
			&& teamIdsWithInviteRecords.Contains(requestedCoachingTeamId.Value);
		var shouldCreateCoachingTeamRequest =
			requestedCoachingTeamId != null &&
			(currentCoachingTeamId == null || requestedCoachingTeamId != currentCoachingTeamId.Value || !hasAnyCoachingTeamInviteRecord);

		await this.updateRefereeRoleCommand.UpdateRefereeRoleAsync(userContext.UserId, refereeRole => new RefereeRole
		{
			IsActive = refereeRole.IsActive,
			CoachingTeam = refereeUpdate.CoachingTeam?.Id,
			PlayingTeam = currentPlayingTeamId != null && refereeUpdate.PlayingTeam == null
				? null
				: refereeRole.PlayingTeam,
			NationalTeam = refereeUpdate.NationalTeam?.Id,
			PrimaryNgb = refereeUpdate.PrimaryNgb,
			SecondaryNgb = refereeUpdate.SecondaryNgb,
		}, this.HttpContext.RequestAborted);

		var statusResponse = new RefereeTeamUpdateStatusViewModel
		{
			PlayingTeam = requestedPlayingTeamId == null
				? null
				: new RefereeTeamRequestStatusViewModel
				{
					TeamId = new TeamIdentifier(requestedPlayingTeamId.Value).ToString(),
					Status = RefereeTeamRequestStatus.Applied,
					RequestCreated = false,
				},
			CoachingTeam = requestedCoachingTeamId == null
				? null
				: new RefereeTeamRequestStatusViewModel
				{
					TeamId = new TeamIdentifier(requestedCoachingTeamId.Value).ToString(),
					Status = RefereeTeamRequestStatus.Applied,
					RequestCreated = false,
				},
		};

		long? currentUserDbId = null;

		if (shouldCreatePlayingTeamRequest)
		{
			currentUserDbId ??= await this.ResolveCurrentUserDbIdAsync(userContext.UserId);
			var result = await this.CreateOrUpdateTeamInviteAsync(
				requestedPlayingTeamId!.Value,
				normalizedEmail,
				currentUserDbId.Value,
				userContext.UserId);
			if (result.ErrorResult != null)
			{
				return result.ErrorResult;
			}

			statusResponse.PlayingTeam = result.Status;
		}

		if (shouldCreateCoachingTeamRequest)
		{
			currentUserDbId ??= await this.ResolveCurrentUserDbIdAsync(userContext.UserId);
			var result = await this.CreateOrUpdateTeamInviteAsync(
				requestedCoachingTeamId!.Value,
				normalizedEmail,
				currentUserDbId.Value,
				userContext.UserId);
			if (result.ErrorResult != null)
			{
				return result.ErrorResult;
			}

			statusResponse.CoachingTeam = result.Status;
		}

		if (!shouldCreatePlayingTeamRequest && !shouldCreateCoachingTeamRequest)
		{
			return this.NoContent();
		}

		return this.Ok(statusResponse);
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

	private Task<long> ResolveCurrentUserDbIdAsync(UserIdentifier userId)
	{
		return this.dbContext.Users
			.WithIdentifier(userId)
			.Select(user => user.Id)
			.SingleAsync(this.HttpContext.RequestAborted);
	}

	/// <summary>
	/// Creates a team invitation for a referee.
	/// Handles duplicate request prevention and auditing.
	/// </summary>
	private async Task<TeamInviteRequestResult> CreateOrUpdateTeamInviteAsync(
		long requestedTeamId,
		string normalizedEmail,
		long currentUserDbId,
		UserIdentifier currentUserId)
	{
		var teamSettings = await this.dbContext.Teams
			.Where(team => team.Id == requestedTeamId)
			.Select(team => new { team.Id, team.Name, team.AutoApprovePlayerRequests })
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		if (teamSettings == null)
		{
			return new TeamInviteRequestResult
			{
				ErrorResult = this.BadRequest("Selected team was not found."),
			};
		}

		var hasPendingRequest = await this.dbContext.TeamInvitations
			.AnyAsync(invite =>
				invite.TeamId == requestedTeamId &&
				invite.Email.ToLower() == normalizedEmail &&
				invite.RevokedAt == null &&
				invite.AcceptedAt == null &&
				invite.DeclinedAt == null,
				this.HttpContext.RequestAborted);

		if (hasPendingRequest)
		{
			return new TeamInviteRequestResult
			{
				Status = new RefereeTeamRequestStatusViewModel
				{
					TeamId = new TeamIdentifier(requestedTeamId).ToString(),
					Status = RefereeTeamRequestStatus.Pending,
					RequestCreated = false,
				},
			};
		}

		var requestedAt = DateTime.UtcNow;
		var invitation = new ManagementHub.Models.Data.TeamInvitation
		{
			TeamId = requestedTeamId,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			CreatedAt = requestedAt,
		};
		this.dbContext.TeamInvitations.Add(invitation);

		this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
		{
			TeamId = requestedTeamId,
			UserId = currentUserDbId,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			ActivityType = TeamPlayerActivityType.InviteCreated,
			CreatedAt = requestedAt,
		});

		if (teamSettings.AutoApprovePlayerRequests)
		{
			var approvedAt = DateTime.UtcNow;
			invitation.AcceptedAt = approvedAt;
			invitation.RespondedByUserId = currentUserDbId;

			var existingPlayerMembership = await this.dbContext.RefereeTeams
				.FirstOrDefaultAsync(
					membership =>
						membership.RefereeId == currentUserDbId &&
						membership.AssociationType == RefereeTeamAssociationType.Player,
					this.HttpContext.RequestAborted);

			if (existingPlayerMembership == null)
			{
				this.dbContext.RefereeTeams.Add(new ManagementHub.Models.Data.RefereeTeam
				{
					AssociationType = RefereeTeamAssociationType.Player,
					RefereeId = currentUserDbId,
					TeamId = requestedTeamId,
					CreatedAt = approvedAt,
					UpdatedAt = approvedAt,
				});
			}
			else if (existingPlayerMembership.TeamId != requestedTeamId && existingPlayerMembership.TeamId.HasValue)
			{
				this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
				{
					TeamId = existingPlayerMembership.TeamId.Value,
					UserId = currentUserDbId,
					Email = normalizedEmail,
					InitiatorUserId = currentUserDbId,
					ActivityType = TeamPlayerActivityType.PlayerRemoved,
					CreatedAt = approvedAt,
				});

				existingPlayerMembership.TeamId = requestedTeamId;
				existingPlayerMembership.UpdatedAt = approvedAt;
			}

			this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
			{
				TeamId = requestedTeamId,
				UserId = currentUserDbId,
				Email = normalizedEmail,
				InitiatorUserId = currentUserDbId,
				ActivityType = TeamPlayerActivityType.InviteAccepted,
				CreatedAt = approvedAt,
			});

			await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);
			return new TeamInviteRequestResult
			{
				Status = new RefereeTeamRequestStatusViewModel
				{
					TeamId = new TeamIdentifier(requestedTeamId).ToString(),
					Status = RefereeTeamRequestStatus.Applied,
					RequestCreated = true,
				},
			};
		}

		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);

		var teamId = new TeamIdentifier(requestedTeamId);
		var teamName = teamSettings.Name ?? teamId.ToString();

		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);
		foreach (var manager in managers.Where(manager => manager.UserId != currentUserId))
		{
			await this.notificationService.CreateTeamInviteRequestNotificationForManagerAsync(
				manager.UserId,
				teamId,
				teamName,
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
