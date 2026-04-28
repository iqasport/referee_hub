using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using ManagementHub.Service.Services;
using Microsoft.Extensions.Hosting;

namespace ManagementHub.Service.Jobs;

/// <summary>
/// Background job that archives notifications older than 30 days.
/// Runs daily at midnight UTC.
/// </summary>
public class NotificationArchivalJob : BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		RecurringJob.AddOrUpdate<INotificationService>(
			"ArchiveOldNotifications",
			service => service.ArchiveOldNotificationsAsync(default),
			Cron.Daily, // Run daily at midnight
			TimeZoneInfo.Utc);

		return Task.CompletedTask;
	}
}
