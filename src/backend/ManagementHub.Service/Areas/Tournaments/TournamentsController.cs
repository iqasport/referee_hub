using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Tournaments;

/// <summary>
/// Actions related to Tournaments.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class TournamentsController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IUserContextProvider userContextProvider;
	private readonly ITournamentContextProvider tournamentContextProvider;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly IUpdateTournamentBannerCommand updateTournamentBannerCommand;
	private readonly ManagementHubDbContext dbContext;

	public TournamentsController(
		IUserContextAccessor contextAccessor,
		IUserContextProvider userContextProvider,
		ITournamentContextProvider tournamentContextProvider,
		ITeamContextProvider teamContextProvider,
		IUpdateTournamentBannerCommand updateTournamentBannerCommand,
		ManagementHubDbContext dbContext)
	{
		this.contextAccessor = contextAccessor;
		this.userContextProvider = userContextProvider;
		this.tournamentContextProvider = tournamentContextProvider;
		this.teamContextProvider = teamContextProvider;
		this.updateTournamentBannerCommand = updateTournamentBannerCommand;
		this.dbContext = dbContext;
	}

	/// <summary>
	/// List tournaments in the Hub.
	/// Private tournament filtering and IsCurrentUserInvolved computation is done at the database level via joins.
	/// </summary>
	[HttpGet]
	[Tags("Tournament")]
	public async Task<Filtered<TournamentViewModel>> GetTournaments([FromQuery] FilteringParameters filtering)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// QueryTournaments performs all filtering and computation at the database level via joins:
		// - Filters private tournaments (only shows those the user manages)
		// - Computes IsCurrentUserInvolved based on tournament manager status
		// Phase 3 will extend: also check if user is a participant via team manager role
		// Phase 4 will extend: also check if user is on a roster
		var tournaments = this.tournamentContextProvider.QueryTournaments(userContext.UserId).ToList();

		// Fetch banner URLs for all tournaments
		var tournamentIds = tournaments.Select(t => t.Id).ToList();
		var bannerUrls = new Dictionary<TournamentIdentifier, Uri?>();
		foreach (var tournamentId in tournamentIds)
		{
			var bannerUri = await this.tournamentContextProvider
				.GetTournamentBannerUriAsync(tournamentId, this.HttpContext.RequestAborted);
			bannerUrls[tournamentId] = bannerUri;
		}

		// Map to view models - IsCurrentUserInvolved is already computed at DB level
		var viewModels = tournaments.Select(t => new TournamentViewModel
		{
			Id = t.Id,
			Name = t.Name,
			Description = t.Description,
			StartDate = t.StartDate,
			EndDate = t.EndDate,
			Type = t.Type,
			Country = t.Country,
			City = t.City,
			Place = t.Place,
			Organizer = t.Organizer,
			IsPrivate = t.IsPrivate,
			BannerImageUrl = bannerUrls.TryGetValue(t.Id, out var uri) ? uri?.ToString() : null,
			IsCurrentUserInvolved = t.IsCurrentUserInvolved
		}).ToList();

		// AsFiltered wraps the list in a Filtered<T> container once, allowing the MVC filtering
		// system to apply pagination metadata. This ensures correct pagination behavior.
		return viewModels.AsFiltered();
	}

	/// <summary>
	/// Get tournament details.
	/// </summary>
	[HttpGet("{tournamentId}")]
	[Tags("Tournament")]
	public async Task<ActionResult<TournamentViewModel>> GetTournament([FromRoute] TournamentIdentifier tournamentId)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var tournament = await this.tournamentContextProvider
			.GetTournamentContextAsync(tournamentId, userContext.UserId, this.HttpContext.RequestAborted);

		// Check access to private tournament - database layer already enforces this
		// but we keep this check for consistency and to provide proper NotFound response
		if (tournament.IsPrivate && !tournament.IsCurrentUserInvolved)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		var bannerUri = await this.tournamentContextProvider
			.GetTournamentBannerUriAsync(tournamentId, this.HttpContext.RequestAborted);

		return new TournamentViewModel
		{
			Id = tournament.Id,
			Name = tournament.Name,
			Description = tournament.Description,
			StartDate = tournament.StartDate,
			EndDate = tournament.EndDate,
			Type = tournament.Type,
			Country = tournament.Country,
			City = tournament.City,
			Place = tournament.Place,
			Organizer = tournament.Organizer,
			IsPrivate = tournament.IsPrivate,
			BannerImageUrl = bannerUri?.ToString(),
			IsCurrentUserInvolved = tournament.IsCurrentUserInvolved
		};
	}

	/// <summary>
	/// Create a new tournament.
	/// </summary>
	[HttpPost]
	[Tags("Tournament")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TournamentIdResponse>> CreateTournament([FromBody] TournamentModel model)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		var tournamentData = new TournamentData
		{
			Name = model.Name,
			Description = model.Description,
			StartDate = model.StartDate,
			EndDate = model.EndDate,
			Type = model.Type,
			Country = model.Country,
			City = model.City,
			Place = model.Place,
			Organizer = model.Organizer,
			IsPrivate = model.IsPrivate
		};

		var tournamentId = await this.tournamentContextProvider
			.CreateTournamentAsync(tournamentData, userContext.UserId, this.HttpContext.RequestAborted);

		return this.Ok(new TournamentIdResponse { Id = tournamentId.ToString() });
	}

	/// <summary>
	/// Update tournament details.
	/// </summary>
	[HttpPut("{tournamentId}")]
	[Tags("Tournament")]
	[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TournamentIdResponse>> UpdateTournament(
		[FromRoute] TournamentIdentifier tournamentId,
		[FromBody] TournamentModel model)
	{
		var tournamentData = new TournamentData
		{
			Name = model.Name,
			Description = model.Description,
			StartDate = model.StartDate,
			EndDate = model.EndDate,
			Type = model.Type,
			Country = model.Country,
			City = model.City,
			Place = model.Place,
			Organizer = model.Organizer,
			IsPrivate = model.IsPrivate
		};

		await this.tournamentContextProvider
			.UpdateTournamentAsync(tournamentId, tournamentData, this.HttpContext.RequestAborted);

		return this.Ok(new TournamentIdResponse { Id = tournamentId.ToString() });
	}

	/// <summary>
	/// Upload tournament banner image.
	/// </summary>
	[HttpPut("{tournamentId}/banner")]
	[Tags("Tournament")]
	[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Uri> UpdateTournamentBanner(
		[FromRoute] TournamentIdentifier tournamentId,
		IFormFile bannerBlob)
	{
		var bannerUri = await this.updateTournamentBannerCommand.UpdateTournamentBannerAsync(
			tournamentId,
			bannerBlob.ContentType,
			bannerBlob.OpenReadStream(),
			this.HttpContext.RequestAborted);
		return bannerUri;
	}

	/// <summary>
	/// Get tournament managers.
	/// </summary>
	[HttpGet("{tournamentId}/managers")]
	[Tags("Tournament")]
	[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
	public async Task<IEnumerable<TournamentManagerViewModel>> GetTournamentManagers(
		[FromRoute] TournamentIdentifier tournamentId)
	{
		var managers = await this.tournamentContextProvider.GetTournamentManagersAsync(
			tournamentId, this.HttpContext.RequestAborted);

		return managers.Select(m => new TournamentManagerViewModel
		{
			Id = m.UserId,
			Name = m.Name,
			Email = m.Email
		});
	}

	/// <summary>
	/// Add a tournament manager.
	/// </summary>
	[HttpPost("{tournamentId}/managers")]
	[Tags("Tournament")]
	[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AddTournamentManager(
		[FromRoute] TournamentIdentifier tournamentId,
		[FromBody] AddTournamentManagerModel model)
	{
		// Validate email is provided
		if (string.IsNullOrWhiteSpace(model.Email))
		{
			return this.BadRequest(new { error = "Email is required" });
		}

		// Parse and validate email
		if (!Email.TryParse(model.Email, out var email))
		{
			return this.BadRequest(new { error = "Invalid email format" });
		}

		// Look up user by email
		var userId = await this.userContextProvider.GetUserIdByEmailAsync(email, this.HttpContext.RequestAborted);
		if (!userId.HasValue)
		{
			return this.NotFound(new { error = "User not found" });
		}

		// Get current user for audit trail
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		// Add manager
		await this.tournamentContextProvider.AddTournamentManagerAsync(
			tournamentId, userId.Value, currentUser.UserId, this.HttpContext.RequestAborted);

		return this.Ok();
	}

	/// <summary>
	/// Remove a tournament manager.
	/// </summary>
	[HttpDelete("{tournamentId}/managers/{userId}")]
	[Tags("Tournament")]
	[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RemoveTournamentManager(
		[FromRoute] TournamentIdentifier tournamentId,
		[FromRoute] UserIdentifier userId)
	{
		try
		{
			var removed = await this.tournamentContextProvider.RemoveTournamentManagerAsync(
				tournamentId, userId, this.HttpContext.RequestAborted);

			return removed ? this.Ok() : this.NotFound(new { error = "User is not a manager" });
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("last manager"))
		{
			return this.BadRequest(new { error = "Cannot remove the last manager of a tournament" });
		}
	}

	// Phase 3: Invite endpoints

	/// <summary>
	/// Get tournament invites.
	/// </summary>
	[HttpGet("{tournamentId}/invites")]
	[Tags("Tournament")]
	[Authorize]
	public async Task<IEnumerable<TournamentInviteViewModel>> GetTournamentInvites(
		[FromRoute] TournamentIdentifier tournamentId)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
			.Any(r => r.Tournament.AppliesTo(tournamentId));

		var isTeamManager = userContext.Roles.OfType<TeamManagerRole>().Any();

		// Only tournament managers or team managers can view invites
		if (!isTournamentManager && !isTeamManager)
		{
			return Array.Empty<TournamentInviteViewModel>();
		}

		// Managers see all invites, team managers see only their own
		UserIdentifier? filterByParticipant = isTournamentManager ? null : userContext.UserId;

		var invites = await this.tournamentContextProvider.GetTournamentInvitesAsync(
			tournamentId, filterByParticipant, this.HttpContext.RequestAborted);

		return invites.Select(i => new TournamentInviteViewModel
		{
			ParticipantType = i.ParticipantType,
			ParticipantId = i.ParticipantId,
			ParticipantName = i.ParticipantName,
			Status = i.GetStatus(),
			InitiatorUserId = i.InitiatorUserId,
			CreatedAt = i.CreatedAt,
			TournamentManagerApproval = new ApprovalStatusViewModel
			{
				Status = i.TournamentManagerApproval,
				Date = i.TournamentManagerApprovalDate
			},
			ParticipantApproval = new ApprovalStatusViewModel
			{
				Status = i.ParticipantApproval,
				Date = i.ParticipantApprovalDate
			}
		});
	}

	/// <summary>
	/// Create a tournament invite.
	/// </summary>
	[HttpPost("{tournamentId}/invites")]
	[Tags("Tournament")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<TournamentInviteViewModel>> CreateInvite(
		[FromRoute] TournamentIdentifier tournamentId,
		[FromBody] CreateInviteModel model)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// Validate and parse participant
		var validationError = this.ValidateInviteParticipant(model, out var teamId);
		if (validationError != null)
		{
			return validationError;
		}

		// Check authorization
		if (!this.IsAuthorizedToCreateInvite(userContext, tournamentId, teamId))
		{
			return this.Forbid();
		}

		// Get tournament and validate
		var tournament = await this.tournamentContextProvider
			.GetTournamentContextAsync(tournamentId, userContext.UserId, this.HttpContext.RequestAborted);

		var tournamentValidation = this.ValidateTournamentForInvite(tournament);
		if (tournamentValidation != null)
		{
			return tournamentValidation;
		}

		// Check for existing participant or invite
		var existingCheck = await this.CheckExistingParticipantOrInvite(tournamentId, teamId);
		if (existingCheck != null)
		{
			return existingCheck;
		}

		// Validate team existence and compatibility
		var teamValidation = await this.ValidateTeamCompatibility(teamId, tournament);
		if (teamValidation != null)
		{
			return teamValidation;
		}

		// Create invite and handle auto-approval
		var invite = await this.tournamentContextProvider.CreateTeamInviteAsync(
			tournamentId,
			teamId,
			userContext.UserId,
			this.HttpContext.RequestAborted);

		await this.HandleAutoApproval(invite, tournamentId, teamId);

		var viewModel = MapInviteToViewModel(invite);

		return this.CreatedAtAction(nameof(GetTournamentInvites),
			new { tournamentId = tournamentId.ToString() },
			viewModel);
	}

	private ActionResult? ValidateInviteParticipant(CreateInviteModel model, out TeamIdentifier teamId)
	{
		teamId = default!;

		if (model.ParticipantType != ParticipantType.Team)
		{
			return this.BadRequest(new { error = "Only team participants supported" });
		}

		if (!TeamIdentifier.TryParse(model.ParticipantId, out teamId))
		{
			return this.BadRequest(new { error = "Invalid participant ID" });
		}

		return null;
	}

	private bool IsAuthorizedToCreateInvite(
		IUserContext userContext,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId)
	{
		var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
			.Any(r => r.Tournament.AppliesTo(tournamentId));
		var isTeamManager = userContext.Roles.OfType<TeamManagerRole>()
			.Any(r => r.Team.AppliesTo(teamId));

		return isTournamentManager || isTeamManager;
	}

	private ActionResult? ValidateTournamentForInvite(ITournamentContext tournament)
	{
		if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
		{
			return this.BadRequest(new { error = "Cannot modify archived tournament" });
		}

		return null;
	}

	private async Task<ActionResult?> CheckExistingParticipantOrInvite(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId)
	{
		var participants = await this.tournamentContextProvider
			.GetTournamentTeamParticipantsAsync(tournamentId, this.HttpContext.RequestAborted);
		if (participants.Any(p => p.TeamId.Equals(teamId)))
		{
			return this.BadRequest(new { error = "Team is already a participant" });
		}

		var existingInvite = await this.tournamentContextProvider
			.GetTeamInviteAsync(tournamentId, teamId, this.HttpContext.RequestAborted);
		if (existingInvite != null && existingInvite.GetStatus() == InviteStatus.Pending)
		{
			return this.BadRequest(new { error = "Pending invite already exists" });
		}

		return null;
	}

	private async Task<ActionResult?> ValidateTeamCompatibility(
		TeamIdentifier teamId,
		ITournamentContext tournament)
	{
		var teamContext = await this.dbContext.Teams
			.Where(t => t.Id == teamId.Id)
			.Select(t => new { t.GroupAffiliation })
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		if (teamContext == null)
		{
			return this.BadRequest(new { error = "Team not found" });
		}

		if (!CanTeamJoinTournament(tournament.Type, teamContext.GroupAffiliation))
		{
			return this.BadRequest(new { error = "Team type incompatible with tournament" });
		}

		return null;
	}

	private async Task HandleAutoApproval(InviteInfo invite, TournamentIdentifier tournamentId, TeamIdentifier teamId)
	{
		if (invite.TournamentManagerApproval == ApprovalStatus.Approved &&
			invite.ParticipantApproval == ApprovalStatus.Approved)
		{
			await this.tournamentContextProvider.AddTeamParticipantAsync(
				tournamentId, teamId, this.HttpContext.RequestAborted);
		}
	}

	private static TournamentInviteViewModel MapInviteToViewModel(InviteInfo invite)
	{
		return new TournamentInviteViewModel
		{
			ParticipantType = invite.ParticipantType,
			ParticipantId = invite.ParticipantId,
			ParticipantName = invite.ParticipantName,
			Status = invite.GetStatus(),
			InitiatorUserId = invite.InitiatorUserId,
			CreatedAt = invite.CreatedAt,
			TournamentManagerApproval = new ApprovalStatusViewModel
			{
				Status = invite.TournamentManagerApproval,
				Date = invite.TournamentManagerApprovalDate
			},
			ParticipantApproval = new ApprovalStatusViewModel
			{
				Status = invite.ParticipantApproval,
				Date = invite.ParticipantApprovalDate
			}
		};
	}

	/// <summary>
	/// Respond to a tournament invite.
	/// </summary>
	[HttpPost("{tournamentId}/invites/{participantId}")]
	[Tags("Tournament")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RespondToInvite(
		[FromRoute] TournamentIdentifier tournamentId,
		[FromRoute] string participantId,
		[FromBody] InviteResponseModel response)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// Parse team ID
		if (!TeamIdentifier.TryParse(participantId, out var teamId))
		{
			return this.BadRequest(new { error = "Invalid participant ID" });
		}

		// Get pending invite
		var invite = await this.tournamentContextProvider
			.GetTeamInviteAsync(tournamentId, teamId, this.HttpContext.RequestAborted);

		if (invite == null || invite.GetStatus() != InviteStatus.Pending)
		{
			return this.NotFound(new { error = "No pending invite found" });
		}

		// Check tournament not archived
		var tournament = await this.tournamentContextProvider
			.GetTournamentContextAsync(tournamentId, userContext.UserId, this.HttpContext.RequestAborted);
		if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
		{
			return this.BadRequest(new { error = "Cannot modify archived tournament" });
		}

		// Check authorization and determine which approval to update
		var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
			.Any(r => r.Tournament.AppliesTo(tournamentId));

		var isParticipant = userContext.Roles.OfType<TeamManagerRole>()
			.Any(r => r.Team.AppliesTo(teamId));

		// Must be either tournament manager (with pending manager approval)
		// or participant (with pending participant approval)
		var canApproveAsManager = isTournamentManager &&
			invite.TournamentManagerApproval == ApprovalStatus.Pending;
		var canApproveAsParticipant = isParticipant &&
			invite.ParticipantApproval == ApprovalStatus.Pending;

		if (!canApproveAsManager && !canApproveAsParticipant)
		{
			return this.Forbid();
		}

		// Update approval
		await this.tournamentContextProvider.UpdateInviteApprovalAsync(
			tournamentId,
			teamId,
			isTournamentManager,
			response.Approved,
			this.HttpContext.RequestAborted);

		// Reload to check if fully approved
		var updatedInvite = await this.tournamentContextProvider
			.GetTeamInviteAsync(tournamentId, teamId, this.HttpContext.RequestAborted);

		// If both approved, create participant
		if (updatedInvite != null && updatedInvite.GetStatus() == InviteStatus.Approved)
		{
			await this.tournamentContextProvider.AddTeamParticipantAsync(
				tournamentId, teamId, this.HttpContext.RequestAborted);
		}

		return this.Ok();
	}

	// Phase 3: Participant endpoints

	/// <summary>
	/// Get tournament participants.
	/// </summary>
	[HttpGet("{tournamentId}/participants")]
	[Tags("Tournament")]
	[Authorize]
	public async Task<IEnumerable<TournamentParticipantViewModel>> GetParticipants(
		[FromRoute] TournamentIdentifier tournamentId)
	{
		// Check access: public tournament or user is manager/participant
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var tournament = await this.tournamentContextProvider
			.GetTournamentContextAsync(tournamentId, userContext.UserId, this.HttpContext.RequestAborted);

		if (tournament.IsPrivate && !tournament.IsCurrentUserInvolved)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		var participants = await this.tournamentContextProvider
			.GetTournamentTeamParticipantsAsync(tournamentId, this.HttpContext.RequestAborted);

		return participants.Select(p => new TournamentParticipantViewModel
		{
			TeamId = p.TeamId,
			TeamName = p.TeamName
		});
	}

	/// <summary>
	/// Remove a tournament participant.
	/// </summary>
	[HttpDelete("{tournamentId}/participants/{teamId}")]
	[Tags("Tournament")]
	[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RemoveParticipant(
		[FromRoute] TournamentIdentifier tournamentId,
		[FromRoute] TeamIdentifier teamId)
	{
		// Check tournament not archived
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var tournament = await this.tournamentContextProvider
			.GetTournamentContextAsync(tournamentId, userContext.UserId, this.HttpContext.RequestAborted);
		if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
		{
			return this.BadRequest(new { error = "Cannot modify archived tournament" });
		}

		await this.tournamentContextProvider.RemoveTeamParticipantAsync(
			tournamentId, teamId, this.HttpContext.RequestAborted);

		return this.Ok();
	}

	// Helper methods

	private static bool CanTeamJoinTournament(TournamentType tournamentType, TeamGroupAffiliation? teamAffiliation)
	{
		return tournamentType switch
		{
			TournamentType.Club => teamAffiliation is TeamGroupAffiliation.University or TeamGroupAffiliation.Community,
			TournamentType.National => teamAffiliation is TeamGroupAffiliation.National,
			TournamentType.Youth => teamAffiliation is TeamGroupAffiliation.Youth,
			TournamentType.Fantasy => true, // Any team can join
			_ => false
		};
	}
}
