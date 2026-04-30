using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage;
using ManagementHub.Storage.Commands.Tournament;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Tournaments;

[ApiController]
[AllowAnonymous]
[Route("api/v2/public/tournaments")]
[Produces("application/json")]
public class PublicTournamentsController : ControllerBase
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly ManagementHubDbContext dbContext;

	public PublicTournamentsController(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
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

		return this.Ok(snapshot.Payload.Tournaments);
	}

	[HttpGet("{tournamentId}")]
	[Tags("Tournament")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<object>> GetPublicTournamentById([FromRoute] string tournamentId)
	{
		var snapshot = await this.GetSnapshotOrDefaultAsync();
		this.ApplyCacheHeaders(snapshot.UpdatedAt);

		if (this.Request.Headers.IfNoneMatch == this.Response.Headers.ETag)
		{
			return this.StatusCode(StatusCodes.Status304NotModified);
		}

		var tournament = snapshot.Payload.Tournaments.FirstOrDefault(t =>
			string.Equals(t.Id, tournamentId, StringComparison.OrdinalIgnoreCase));

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId);
		}

		return this.Ok(tournament);
	}

	private async Task<(Models.Domain.Tournament.PublicTournamentSnapshotPayload Payload, DateTime UpdatedAt)> GetSnapshotOrDefaultAsync()
	{
		var row = await this.dbContext.PublicTournamentSnapshots
			.AsNoTracking()
			.SingleOrDefaultAsync(s => s.Key == RefreshPublicTournamentSnapshotCommand.SnapshotKey, this.HttpContext.RequestAborted);

		if (row == null)
		{
			return (new Models.Domain.Tournament.PublicTournamentSnapshotPayload
			{
				GeneratedAtUtc = DateTime.UtcNow,
				Tournaments = Array.Empty<Models.Domain.Tournament.PublicTournamentSnapshotTournament>(),
			}, DateTime.UtcNow);
		}

		var payload = JsonSerializer.Deserialize<Models.Domain.Tournament.PublicTournamentSnapshotPayload>(
			row.SnapshotJson,
			JsonSerializerOptions);

		return (payload ?? new Models.Domain.Tournament.PublicTournamentSnapshotPayload
		{
			GeneratedAtUtc = row.UpdatedAt,
			Tournaments = Array.Empty<Models.Domain.Tournament.PublicTournamentSnapshotTournament>(),
		}, row.UpdatedAt);
	}

	private void ApplyCacheHeaders(DateTime updatedAt)
	{
		this.Response.Headers.CacheControl = "public,max-age=300,s-maxage=86400,stale-while-revalidate=600";
		this.Response.Headers.ETag = $"W/\"public-tournaments-{updatedAt.ToFileTimeUtc()}\"";
		this.Response.Headers.LastModified = updatedAt.ToUniversalTime().ToString("R");
	}
}