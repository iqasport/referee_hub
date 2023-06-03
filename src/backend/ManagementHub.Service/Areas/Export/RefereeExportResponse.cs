namespace ManagementHub.Service.Areas.Export;

public class RefereeExportResponse
{
	/// <summary>
	/// Id of the background job scheduled to process this task.
	/// </summary>
	public required string JobId { get; set; }
}
