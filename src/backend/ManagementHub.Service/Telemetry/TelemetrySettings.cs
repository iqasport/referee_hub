namespace ManagementHub.Service.Telemetry;

public class TelemetrySettings
{
	/// <summary>
	/// Name of the exporter to be used. Currently either "Otlp" or nothing.
	/// </summary>
	public string? Exporter { get; set; }

	/// <summary>
	/// Endpoint to be used for OTLP gRPC.
	/// </summary>
	public Uri? OtlpEndpoint { get; set; }
}
