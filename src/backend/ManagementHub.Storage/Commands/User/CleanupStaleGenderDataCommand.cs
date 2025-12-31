using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Configuration;
using ManagementHub.Models.Enums;
using ManagementHub.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Storage.Commands.User;

public class CleanupStaleGenderDataCommand : ICleanupStaleGenderDataCommand
{
	private readonly ManagementHubDbContext context;
	private readonly IOptionsSnapshot<GenderDataRetentionSettings> settings;
	private readonly ILogger<CleanupStaleGenderDataCommand> logger;

	public CleanupStaleGenderDataCommand(
		ManagementHubDbContext context,
		IOptionsSnapshot<GenderDataRetentionSettings> settings,
		ILogger<CleanupStaleGenderDataCommand> logger)
	{
		this.context = context;
		this.settings = settings;
		this.logger = logger;
	}

	public async Task CleanupStaleGenderDataAsync(CancellationToken cancellationToken)
	{
		if (!this.settings.Value.EnableAutomaticDeletion)
		{
			this.logger.LogInformation("Automatic gender data deletion is disabled");
			return;
		}

		using var transaction = await this.context.Database.BeginTransactionAsync(cancellationToken);

		var now = DateTime.UtcNow;
		var updateThreshold = now.AddMonths(-this.settings.Value.NotUpdatedForMonths);
		var tournamentEndThreshold = now.AddMonths(-this.settings.Value.MonthsSinceLastTournamentEnded);

		// Find users with stale gender data
		var staleGenderData = await this.context.UserDelicateInfos
			.Where(udi => udi.UpdatedAt < updateThreshold)
			.Select(udi => new { udi.UserId, udi.UpdatedAt })
			.ToListAsync(cancellationToken);

		this.logger.LogInformation(
			"Found {Count} gender records not updated since {Threshold}",
			staleGenderData.Count,
			updateThreshold);

		var deletedCount = 0;

		foreach (var record in staleGenderData)
		{
			// Check if user has played in any tournament that ended recently
			var hasRecentTournament = await this.context.TournamentTeamRosterEntries
				.Where(rosterEntry =>
					rosterEntry.UserId == record.UserId &&
					rosterEntry.Role == RosterRole.Player)
				.Join(
					this.context.TournamentTeamParticipants,
					rosterEntry => rosterEntry.TournamentTeamParticipantId,
					participant => participant.Id,
					(rosterEntry, participant) => participant.Tournament)
				.AnyAsync(tournament =>
					tournament.EndDate > DateOnly.FromDateTime(tournamentEndThreshold),
					cancellationToken);

			if (!hasRecentTournament)
			{
				// Gender data is stale - delete it
				await this.context.UserDelicateInfos
					.Where(udi => udi.UserId == record.UserId)
					.ExecuteDeleteAsync(cancellationToken);

				deletedCount++;

				this.logger.LogInformation(
					"Deleted stale gender data for user ID {UserId} (last updated: {UpdatedAt})",
					record.UserId,
					record.UpdatedAt);
			}
		}

		this.logger.LogInformation(
			"Deleted {DeletedCount} stale gender records out of {TotalCount} candidates",
			deletedCount,
			staleGenderData.Count);

		await transaction.CommitAsync(cancellationToken);
	}
}
