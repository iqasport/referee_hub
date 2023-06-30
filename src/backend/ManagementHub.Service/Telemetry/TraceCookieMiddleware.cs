using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ManagementHub.Service.Telemetry;

public class TraceCookieMiddleware : IMiddleware
{
	public const string TraceIdCookieName = "refhub_diagnosticId";

	[SuppressMessage("Security", "SCS0008:The cookie is missing 'Secure' flag.", Justification = "HTTP is used for local development - TODO: apply secure flag based on request")]
	[SuppressMessage("Security", "SCS0009:The cookie is missing 'HttpOnly' flag.", Justification = "The diagnostic ID value may be read with JS to provide user means of identification with support.")]
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
