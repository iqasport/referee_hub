using System.Diagnostics;
using Hangfire.Server;
using ManagementHub.Models.Misc;

namespace ManagementHub.Service.Telemetry;

public class HangfireTraceFilter : IServerFilter
{
	public void OnPerformed(PerformedContext ctx)
	{
		if (ctx.Items.TryGetValue("activity", out var obj) && obj is Activity activity)
		{
			activity.Dispose();
		}
	}

	public void OnPerforming(PerformingContext ctx)
	{
		var jobId = ctx.BackgroundJob.Id;
		var activity = ActivityExtensions.Source.CreateActivity(
			"BackgroundJob",
			ActivityKind.Internal,
			jobId.Replace("-",""),
			tags: new Dictionary<string, object?>
			{
				["JobName"] = ctx.BackgroundJob.Job.Method.Name,
				["JobId"] = jobId,
			},
			idFormat: ActivityIdFormat.W3C);
		ctx.Items.Add("activity", activity);
	}
}
