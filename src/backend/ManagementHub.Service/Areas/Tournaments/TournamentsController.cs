using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User.Roles;
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
	private readonly ITournamentContextProvider tournamentContextProvider;
	private readonly IUpdateTournamentBannerCommand updateTournamentBannerCommand;

	public TournamentsController(
		IUserContextAccessor contextAccessor,
		ITournamentContextProvider tournamentContextProvider,
		IUpdateTournamentBannerCommand updateTournamentBannerCommand)
	{
		this.contextAccessor = contextAccessor;
		this.tournamentContextProvider = tournamentContextProvider;
		this.updateTournamentBannerCommand = updateTournamentBannerCommand;
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
		var query = this.tournamentContextProvider.QueryTournaments(userContext.UserId);

		var tournaments = query.AsFiltered();

		// Fetch banner URLs for all tournaments
		var tournamentIds = tournaments.Items.Select(t => t.Id).ToList();
		var bannerUrls = new Dictionary<TournamentIdentifier, Uri?>();
		foreach (var tournamentId in tournamentIds)
		{
			var bannerUri = await this.tournamentContextProvider
				.GetTournamentBannerUriAsync(tournamentId, this.HttpContext.RequestAborted);
			bannerUrls[tournamentId] = bannerUri;
		}

		// Map to view models - IsCurrentUserInvolved is already computed at DB level
		var viewModels = tournaments.Items.Select(t => new TournamentViewModel
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
		});

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

		// Check access to private tournament - this is enforced at database level
		// GetTournamentContextAsync already filters private tournaments based on IsCurrentUserInvolved
		if (tournament.IsPrivate && !tournament.IsCurrentUserInvolved)
		{
			return this.NotFound();
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
}
