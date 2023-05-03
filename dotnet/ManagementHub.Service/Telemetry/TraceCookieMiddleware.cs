using System.Diagnostics;

namespace ManagementHub.Service.Telemetry;

public class TraceCookieMiddleware : IMiddleware
{
	public const string TraceIdCookieName = "refhub_diagnosticId";
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
        var activity = Activity.Current;
        if (activity is not null && activity.TraceId != default)
        {
            context.Response.Cookies.Append(TraceIdCookieName, activity.TraceId.ToString());
        }

        await next(context);
	}
}
