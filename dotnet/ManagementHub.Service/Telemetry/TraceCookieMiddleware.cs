using System.Diagnostics;

namespace ManagementHub.Service.Telemetry;

public class TraceCookieMiddleware : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
        var activity = Activity.Current;
        if (activity is not null && activity.TraceId != default)
            context.Response.Cookies.Append("traceid", activity.TraceId.ToString());
		
        await next(context);
	}
}
