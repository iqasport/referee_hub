using System.Diagnostics;
using Microsoft.Net.Http.Headers;

namespace ManagementHub.Service.Telemetry;

public class CookieTraceContextPropagator : DistributedContextPropagator
{
	private readonly DistributedContextPropagator root = DistributedContextPropagator.CreateDefaultPropagator();
	private readonly ILogger<CookieTraceContextPropagator> logger;

	public CookieTraceContextPropagator(ILogger<CookieTraceContextPropagator> logger)
	{
		this.logger = logger;
	}

	public override IReadOnlyCollection<string> Fields => this.root.Fields;

	public override IEnumerable<KeyValuePair<string, string?>>? ExtractBaggage(object? carrier, PropagatorGetterCallback? getter)
	{
		return this.root.ExtractBaggage(carrier, getter);
	}

	public override void ExtractTraceIdAndState(object? carrier, PropagatorGetterCallback? getter, out string? traceId, out string? traceState)
	{
		this.root.ExtractTraceIdAndState(carrier, getter, out traceId, out traceState);

		if (carrier is IHeaderDictionary headers)
		{
			var cookies = CookieHeaderValue.ParseList(headers.Cookie);
			foreach (var c in cookies)
			{
				const int traceIdLength = 32; // 16 bytes trace id
				if (c.Name.Equals(TraceCookieMiddleware.TraceIdCookieName, StringComparison.OrdinalIgnoreCase) &&
					c.Value.Length == traceIdLength)
				{
					// in the cookie we are storing only the trace id
					// but the code in ASP.NET Core expects a full W3C activity id
					// so we're making one with a parent span id being constant 1 (all zeroes doesn't work)
					traceId = $"00-{c.Value}-0000000000000001-01";
					this.logger.LogDebug(-0x1dbab00, "Extracted traceId from '{cookieName}' cookie: {traceId}", TraceCookieMiddleware.TraceIdCookieName, c.Value);
				}
			}
		}
	}

	public override void Inject(Activity? activity, object? carrier, PropagatorSetterCallback? setter)
	{
		this.root.Inject(activity, carrier, setter);
	}
}
