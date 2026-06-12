using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using ManagementHub.Models.Abstraction.Commands;

namespace ManagementHub.Service.Jobs;

public class EnsureDailyPublicTournamentSnapshot : BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		RecurringJob.AddOrUpdate<IRefreshPublicTournamentSnapshotCommand>(
			"RefreshPublicTournamentSnapshot",
			cmd => cmd.RefreshPublicTournamentSnapshot(default),
			Cron.Daily,
			TimeZoneInfo.Utc);

		return Task.CompletedTask;
	}
}