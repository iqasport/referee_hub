using Hangfire;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Debug;

/// <summary>
/// Actions related to exporting users with the referee role.
/// </summary>
[ApiController]
[Route("api/debug/statsjob")]
[Authorize(AuthorizationPolicies.TechAdminPolicy)]
public class HistoricalStatsJobController
{
	private readonly ILogger logger;
	private readonly IBackgroundJobClient backgroundJobClient;

	public HistoricalStatsJobController(ILogger<HistoricalStatsJobController> logger, IBackgroundJobClient backgroundJobClient)
	{
		this.logger = logger;
		this.backgroundJobClient = backgroundJobClient;
	}

	[HttpPost("run/{ngb}")]
	[Tags("Debug")]
	public Task RunStatsJob([FromRoute] NgbConstraint ngb)
	{
		this.backgroundJobClient.Enqueue<ICreateNgbStatsSnapshotCommand>(this.logger, cmd => cmd.CreateNgbStatsSnapshot(ngb, default));
		return Task.CompletedTask;
	}

	[HttpPost("schedule")]
	[Tags("Debug")]
	public Task ScheduleStatsJob()
	{
		RecurringJob.AddOrUpdate<ICreateNgbStatsSnapshotCommand>(
			"CreateNgbStatsSnapshot",
			cmd => cmd.CreateNgbStatsSnapshot(NgbConstraint.Any, default),
			"0 3 1 * *", // run on the first day of every month at 3:00 UTC
			TimeZoneInfo.Utc);
		return Task.CompletedTask;
	}
}
