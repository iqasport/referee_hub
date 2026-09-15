using System.Collections.Generic;
using System.Linq;
using ManagementHub.Models.Data;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Shared helpers for team invite-related controllers.
/// </summary>
internal static class TeamInviteHelpers
{
	/// <summary>
	/// Computes the transfer approval status for a pending invitation based on its NGB approvals.
	/// </summary>
	/// <param name="isTransfer">True when the invitation has an origin team (i.e. it is a transfer).</param>
	/// <param name="ngbApprovals">All NgbTransferApproval records linked to this invitation.</param>
	/// <param name="isAccepted">Whether the invitation was already accepted by the team.</param>
	/// <param name="isDeclinedOrRevoked">Whether the invitation was declined or revoked.</param>
	public static TransferApprovalStatus ComputeTransferStatus(
		bool isTransfer,
		IEnumerable<NgbTransferApproval> ngbApprovals,
		bool isAccepted,
		bool isDeclinedOrRevoked)
	{
		if (!isTransfer)
		{
			return TransferApprovalStatus.NotATransfer;
		}

		if (isAccepted)
		{
			return TransferApprovalStatus.Approved;
		}

		var approvalList = ngbApprovals.ToList();

		if (approvalList.Any(a => a.RejectedAt != null))
		{
			return TransferApprovalStatus.RejectedByNgb;
		}

		if (isDeclinedOrRevoked)
		{
			return TransferApprovalStatus.Declined;
		}

		// All NGB records must be approved before the team manager can act.
		if (approvalList.All(a => a.ApprovedAt != null))
		{
			return TransferApprovalStatus.PendingTeamApproval;
		}

		return TransferApprovalStatus.PendingNgbApproval;
	}

	/// <summary>
	/// Builds a display name from first and last name, returning null if both are empty.
	/// </summary>
	public static string? BuildDisplayName(string? firstName, string? lastName)
	{
		var displayName = string.Join(" ", new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
		return string.IsNullOrWhiteSpace(displayName) ? null : displayName;
	}

	/// <summary>
	/// Normalizes an email address for case-insensitive in-memory comparison.
	/// Note: Use <c>string.ToLower()</c> directly in EF Core query expressions,
	/// as <c>ToLowerInvariant()</c> is not translated to SQL by EF Core.
	/// </summary>
	public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
