using Microsoft.Extensions.Telemetry.Enrichment;
using OpenTelemetry;

namespace ManagementHub.Service.Telemetry;

public class BaggageLogEnricher : ILogEnricher
{
	private readonly string baggageKey;

	public BaggageLogEnricher(string baggageKey)
	{
		this.baggageKey = baggageKey;
	}

	public void Enrich(IEnrichmentPropertyBag bag)
	{
		var value = Baggage.GetBaggage(this.baggageKey);
		if (value != null)
		{
			bag.Add(this.baggageKey, value);
		}
	}
}
