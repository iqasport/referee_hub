using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.Tournament;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Tournament;

public class RefreshPublicTournamentSnapshotCommand : IRefreshPublicTournamentSnapshotCommand
{
	public const string SnapshotKey = "public_tournaments";

	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<RefreshPublicTournamentSnapshotCommand> logger;

	public RefreshPublicTournamentSnapshotCommand(
		ManagementHubDbContext dbContext,
		ILogger<RefreshPublicTournamentSnapshotCommand> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public async Task RefreshPublicTournamentSnapshot(CancellationToken cancellationToken)
	{
		var utcNow = DateTime.UtcNow;

		var tournaments = await this.dbContext.Tournaments
			.AsNoTracking()
			.Where(t => t.DeletedAt == null && !t.IsPrivate)
			.OrderBy(t => t.StartDate)
			.ThenBy(t => t.EndDate)
			.ThenBy(t => t.Name)
			.Select(t => new PublicTournamentSnapshotTournament
			{
				Id = t.UniqueId,
				Name = t.Name,
				Description = t.Description,
				StartDate = t.StartDate,
				EndDate = t.EndDate,
				RegistrationEndsDate = t.RegistrationEndsDate,
				Type = t.Type,
				Country = t.Country,
				City = t.City,
				Place = t.Place,
				Organizer = t.Organizer,
				IsRegistrationOpen = t.IsRegistrationOpen,
			})
			.ToListAsync(cancellationToken);

		var payload = new PublicTournamentSnapshotPayload
		{
			GeneratedAtUtc = utcNow,
			Tournaments = tournaments,
		};

		var snapshotJson = JsonSerializer.Serialize(payload, JsonSerializerOptions);

		var existingSnapshot = await this.dbContext.PublicTournamentSnapshots
			.SingleOrDefaultAsync(s => s.Key == SnapshotKey, cancellationToken);

		if (existingSnapshot == null)
		{
			existingSnapshot = new Models.Data.PublicTournamentSnapshot
			{
				Key = SnapshotKey,
				SnapshotJson = snapshotJson,
				CreatedAt = utcNow,
				UpdatedAt = utcNow,
			};

			this.dbContext.PublicTournamentSnapshots.Add(existingSnapshot);
		}
		else
		{
			existingSnapshot.SnapshotJson = snapshotJson;
			existingSnapshot.UpdatedAt = utcNow;
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Refreshed public tournament snapshot with {TournamentCount} tournament(s)", tournaments.Count);
	}
}