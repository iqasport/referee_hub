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

	public override IReadOnlyCollection<string> Fields => root.Fields;

	public override IEnumerable<KeyValuePair<string, string?>>? ExtractBaggage(object? carrier, PropagatorGetterCallback? getter)
	{
		return root.ExtractBaggage(carrier, getter);
	}

	public override void ExtractTraceIdAndState(object? carrier, PropagatorGetterCallback? getter, out string? traceId, out string? traceState)
	{
        root.ExtractTraceIdAndState(carrier, getter, out traceId, out traceState);

		if (carrier is IHeaderDictionary headers)
        {
            var cookies = CookieHeaderValue.ParseList(headers.Cookie);
            foreach (var c in cookies)
            {
                if (c.Name == "traceid")
                {
                    //TODO: first validate it a little
                    traceId = c.Value.ToString();
                    logger.LogInformation("Extracted traceId from cookie: {traceId}", traceId);
                }
            }
        }
	}

	public override void Inject(Activity? activity, object? carrier, PropagatorSetterCallback? setter)
	{
		root.Inject(activity, carrier, setter);
	}
}