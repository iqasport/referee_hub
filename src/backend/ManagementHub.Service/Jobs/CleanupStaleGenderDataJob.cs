using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using ManagementHub.Models.Abstraction.Commands;
using Microsoft.Extensions.Hosting;

namespace ManagementHub.Service.Jobs;

public class CleanupStaleGenderDataJob : BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		RecurringJob.AddOrUpdate<ICleanupStaleGenderDataCommand>(
			"CleanupStaleGenderData",
			cmd => cmd.CleanupStaleGenderDataAsync(default),
			Cron.Daily, // Run daily
			TimeZoneInfo.Utc);

		return Task.CompletedTask;
	}
}
