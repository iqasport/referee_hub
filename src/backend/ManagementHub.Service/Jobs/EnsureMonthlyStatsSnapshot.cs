using Hangfire;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Service.Jobs;

public class EnsureMonthlyStatsSnapshot : BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		RecurringJob.AddOrUpdate<ICreateNgbStatsSnapshotCommand>(
			"CreateNgbStatsSnapshot",
			cmd => cmd.CreateNgbStatsSnapshot(NgbConstraint.Any, default),
			"0 3 1 * *", // run on the first day of every month at 3:00 UTC
			TimeZoneInfo.Utc);

		return Task.CompletedTask;
	}
}
