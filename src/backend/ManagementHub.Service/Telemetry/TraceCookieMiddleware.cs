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
			var setCookie = false;
			// Setting cookies prevents CloudFlare from caching the requests.
			// So if the cookie is already set with the same value as current trace ID, don't set it again.
			// Unless on API requests - those are not cached anyway.
			// The cookie has Session lifetime, so it will be removed when the browser is closed.
			if (context.Request.Cookies.TryGetValue(TraceIdCookieName, out var incomingIdValue) && incomingIdValue != activity.TraceId.ToString())
			{
				setCookie = true;
			}
			else if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
			{
				setCookie = true;
			}

			if (setCookie)
			{
				context.Response.Cookies.Append(TraceIdCookieName, activity.TraceId.ToString());
			}
		}

		await next(context);
	}
}
