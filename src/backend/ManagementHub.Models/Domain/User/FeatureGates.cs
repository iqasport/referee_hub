namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Feature gates for controlling frontend features.
/// </summary>
public class FeatureGates
{
	/// <summary>
	/// Example test flag for demonstration purposes.
	/// </summary>
	public bool IsTestFlag { get; set; }

	/// <summary>
	/// Controls whether to show detailed test results on the finish page.
	/// </summary>
	public bool ShowTestResultsOnFinish { get; set; }
}
