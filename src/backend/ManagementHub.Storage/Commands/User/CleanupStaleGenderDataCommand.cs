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

		// Single query to find all stale gender records with their tournament participation status
		var staleGenderRecords = await this.context.UserDelicateInfos
			.Where(udi => udi.UpdatedAt < updateThreshold)
			.GroupJoin(
				this.context.TournamentTeamRosterEntries.Where(entry => entry.Role == RosterRole.Player)
					.Join(
						this.context.TournamentTeamParticipants,
						entry => entry.TournamentTeamParticipantId,
						participant => participant.Id,
						(entry, participant) => new { entry.UserId, participant.Tournament.EndDate }),
				udi => udi.UserId,
				entry => entry.UserId,
				(udi, tournaments) => new
				{
					udi.UserId,
					udi.UpdatedAt,
					HasRecentTournament = tournaments.Any(t => t.EndDate > DateOnly.FromDateTime(tournamentEndThreshold))
				})
			.Where(x => !x.HasRecentTournament)
			.ToListAsync(cancellationToken);

		this.logger.LogInformation(
			"Found {Count} stale gender records eligible for deletion",
			staleGenderRecords.Count);

		if (staleGenderRecords.Count > 0)
		{
			var userIdsToDelete = staleGenderRecords.Select(r => r.UserId).ToList();

			var deletedCount = await this.context.UserDelicateInfos
				.Where(udi => userIdsToDelete.Contains(udi.UserId))
				.ExecuteDeleteAsync(cancellationToken);

			this.logger.LogInformation(
				"Deleted {DeletedCount} stale gender records",
				deletedCount);
		}

		await transaction.CommitAsync(cancellationToken);
	}
}
