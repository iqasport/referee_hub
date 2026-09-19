namespace ManagementHub.Service.Areas.Ngbs;

/// <summary>Request body for updating NGB transfer settings.</summary>
public class NgbTransferSettingsRequest
{
	/// <summary>
	/// When true, transfers between two teams within this NGB are automatically approved
	/// at the NGB level without requiring manual review.
	/// International transfers are always excluded from auto-approval.
	/// </summary>
	public bool AutoApproveInternalTransfers { get; init; }
}
