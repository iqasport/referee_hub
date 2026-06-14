using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Exceptions;
using ManagementHub.Serialization.Tournaments;
using ManagementHub.Storage;
using ManagementHub.Storage.Commands.Tournament;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Service.Areas.Tournaments;

[ApiController]
[AllowAnonymous]
[Route("api/v2/public/tournaments")]
[Produces("application/json")]
public class PublicTournamentsController : ControllerBase
{
	private readonly ManagementHubDbContext dbContext;
	private readonly IRefreshPublicTournamentSnapshotCommand refreshPublicTournamentSnapshotCommand;
	private readonly ITournamentContextProvider tournamentContextProvider;
	private readonly ILogger<PublicTournamentsController> logger;

	public PublicTournamentsController(
		ManagementHubDbContext dbContext,
		IRefreshPublicTournamentSnapshotCommand refreshPublicTournamentSnapshotCommand,
		ITournamentContextProvider tournamentContextProvider,
		ILogger<PublicTournamentsController> logger)
	{
		this.dbContext = dbContext;
		this.refreshPublicTournamentSnapshotCommand = refreshPublicTournamentSnapshotCommand;
		this.tournamentContextProvider = tournamentContextProvider;
		this.logger = logger;
	}

	[HttpGet]
	[Tags("Tournament")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<object>> GetPublicTournaments()
	{
		var snapshot = await this.GetSnapshotOrDefaultAsync();
		this.ApplyCacheHeaders(snapshot.UpdatedAt);

		if (this.Request.Headers.IfNoneMatch == this.Response.Headers.ETag)
		{
			return this.StatusCode(StatusCodes.Status304NotModified);
		}

		return this.Ok(await this.MapPublicTournamentsAsync(snapshot.Payload.Tournaments));
	}

	[HttpGet("{tournamentId}")]
	[Tags("Tournament")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<object>> GetPublicTournamentById([FromRoute] TournamentIdentifier tournamentId)
	{
		var snapshot = await this.GetSnapshotOrDefaultAsync();

		var tournament = snapshot.Payload.Tournaments.FirstOrDefault(t => t.Id == tournamentId);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		this.ApplyCacheHeaders(snapshot.UpdatedAt, tournamentId.ToString());

		if (this.Request.Headers.IfNoneMatch == this.Response.Headers.ETag)
		{
			return this.StatusCode(StatusCodes.Status304NotModified);
		}

		return this.Ok(await this.MapPublicTournamentAsync(tournament));
	}

	private async Task<IReadOnlyList<TournamentViewModel>> MapPublicTournamentsAsync(
		IReadOnlyList<PublicTournamentSnapshotTournament> tournaments)
	{
		var bannerUrls = await this.tournamentContextProvider.GetTournamentBannerUrisAsync(
			tournaments.Select(tournament => tournament.Id),
			this.HttpContext.RequestAborted);

		return tournaments.Select(tournament => new TournamentViewModel
		{
			Id = tournament.Id,
			Name = tournament.Name,
			Description = tournament.Description,
			StartDate = tournament.StartDate,
			EndDate = tournament.EndDate,
			RegistrationEndsDate = tournament.RegistrationEndsDate,
			Type = tournament.Type,
			Country = tournament.Country,
			City = tournament.City,
			Place = tournament.Place,
			Organizer = tournament.Organizer,
			IsPrivate = false,
			IsRegistrationOpen = tournament.IsRegistrationOpen,
			BannerImageUrl = bannerUrls.TryGetValue(tournament.Id, out var uri) ? uri?.ToString() : null,
			IsCurrentUserInvolved = false,
		}).ToList();
	}

	private async Task<TournamentViewModel> MapPublicTournamentAsync(PublicTournamentSnapshotTournament tournament)
	{
		var bannerUri = await this.tournamentContextProvider
			.GetTournamentBannerUriAsync(tournament.Id, this.HttpContext.RequestAborted);

		return new TournamentViewModel
		{
			Id = tournament.Id,
			Name = tournament.Name,
			Description = tournament.Description,
			StartDate = tournament.StartDate,
			EndDate = tournament.EndDate,
			RegistrationEndsDate = tournament.RegistrationEndsDate,
			Type = tournament.Type,
			Country = tournament.Country,
			City = tournament.City,
			Place = tournament.Place,
			Organizer = tournament.Organizer,
			IsPrivate = false,
			IsRegistrationOpen = tournament.IsRegistrationOpen,
			BannerImageUrl = bannerUri?.ToString(),
			IsCurrentUserInvolved = false,
		};
	}

	private async Task<(Models.Domain.Tournament.PublicTournamentSnapshotPayload Payload, DateTime UpdatedAt)> GetSnapshotOrDefaultAsync()
	{
		var row = await this.TryGetSnapshotRowAsync();

		if (row == null)
		{
			this.logger.LogInformation("Public tournament snapshot was missing; triggering one-time refresh fallback.");
			await this.refreshPublicTournamentSnapshotCommand.RefreshPublicTournamentSnapshot(this.HttpContext.RequestAborted);
			row = await this.TryGetSnapshotRowAsync();
		}

		if (row == null)
		{
			return (new Models.Domain.Tournament.PublicTournamentSnapshotPayload
			{
				GeneratedAtUtc = DateTime.UtcNow,
				Tournaments = Array.Empty<Models.Domain.Tournament.PublicTournamentSnapshotTournament>(),
			}, DateTime.UtcNow);
		}

		var payload = PublicTournamentSnapshotJson.Deserialize(row.SnapshotJson);

		return (payload ?? new Models.Domain.Tournament.PublicTournamentSnapshotPayload
		{
			GeneratedAtUtc = row.UpdatedAt,
			Tournaments = Array.Empty<Models.Domain.Tournament.PublicTournamentSnapshotTournament>(),
		}, row.UpdatedAt);
	}

	private Task<PublicTournamentSnapshot?> TryGetSnapshotRowAsync()
	{
		return this.dbContext.PublicTournamentSnapshots
			.AsNoTracking()
			.SingleOrDefaultAsync(s => s.Key == RefreshPublicTournamentSnapshotCommand.SnapshotKey, this.HttpContext.RequestAborted);
	}

	private void ApplyCacheHeaders(DateTime updatedAt, string? scope = null)
	{
		var suffix = string.IsNullOrWhiteSpace(scope)
			? ""
			: $"-{scope.ToLowerInvariant()}";

		this.Response.Headers.CacheControl = "public,max-age=300,s-maxage=86400,stale-while-revalidate=600";
		this.Response.Headers.ETag = $"W/\"public-tournaments{suffix}-{updatedAt.ToFileTimeUtc()}\"";
		this.Response.Headers.LastModified = updatedAt.ToUniversalTime().ToString("R");
	}
}