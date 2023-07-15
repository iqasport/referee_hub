namespace ManagementHub.Service.Telemetry;

public class TelemetrySettings
{
	/// <summary>
	/// Name of the exporter to be used. Currently either "Otlp", "Azure" or nothing.
	/// </summary>
	public string? Exporter { get; set; }

	/// <summary>
	/// Endpoint to be used for OTLP gRPC.
	/// </summary>
	public Uri? OtlpEndpoint { get; set; }

	/// <summary>
	/// Connection string for the Application Insights.
	/// </summary>
	public string? AzureConnectionString { get; set; }
}
