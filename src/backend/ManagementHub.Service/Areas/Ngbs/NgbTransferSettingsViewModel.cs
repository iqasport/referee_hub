namespace ManagementHub.Service.Areas.Ngbs;

/// <summary>Current NGB transfer settings.</summary>
public class NgbTransferSettingsViewModel
{
	/// <summary>
	/// When true, transfers between two teams within this NGB are automatically approved
	/// at the NGB level without requiring manual review.
	/// </summary>
	public required bool AutoApproveInternalTransfers { get; init; }
}
