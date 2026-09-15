using System;
using System.Collections.Generic;
using FluentAssertions;
using ManagementHub.Models.Data;
using ManagementHub.Service.Areas.Teams;
using Xunit;

namespace ManagementHub.UnitTests.Areas.Teams;

public class TeamInviteHelpersTests
{
	[Fact]
	public void ComputeTransferStatus_WhenNotTransfer_ReturnsNotATransfer()
	{
		var result = TeamInviteHelpers.ComputeTransferStatus(false, Array.Empty<NgbTransferApproval>(), false, false);

		result.Should().Be(TransferApprovalStatus.NotATransfer);
	}

	[Fact]
	public void ComputeTransferStatus_WhenAccepted_ReturnsApproved()
	{
		var approvals = new[] { new NgbTransferApproval { RejectedAt = DateTime.UtcNow } };

		var result = TeamInviteHelpers.ComputeTransferStatus(true, approvals, true, false);

		result.Should().Be(TransferApprovalStatus.Approved);
	}

	[Fact]
	public void ComputeTransferStatus_WhenAnyNgbRejects_ReturnsRejectedByNgb()
	{
		var approvals = new[] { new NgbTransferApproval { RejectedAt = DateTime.UtcNow } };

		var result = TeamInviteHelpers.ComputeTransferStatus(true, approvals, false, false);

		result.Should().Be(TransferApprovalStatus.RejectedByNgb);
	}

	[Fact]
	public void ComputeTransferStatus_WhenDeclinedOrRevoked_ReturnsDeclined()
	{
		var result = TeamInviteHelpers.ComputeTransferStatus(true, Array.Empty<NgbTransferApproval>(), false, true);

		result.Should().Be(TransferApprovalStatus.Declined);
	}

	[Fact]
	public void ComputeTransferStatus_WhenAllNgbsApprove_ReturnsPendingTeamApproval()
	{
		var approvals = new[] { new NgbTransferApproval { ApprovedAt = DateTime.UtcNow } };

		var result = TeamInviteHelpers.ComputeTransferStatus(true, approvals, false, false);

		result.Should().Be(TransferApprovalStatus.PendingTeamApproval);
	}

	[Fact]
	public void ComputeTransferStatus_WhenNgbApprovalIsMissing_ReturnsPendingNgbApproval()
	{
		var approvals = new List<NgbTransferApproval>
		{
			new() { ApprovedAt = DateTime.UtcNow },
			new(),
		};

		var result = TeamInviteHelpers.ComputeTransferStatus(true, approvals, false, false);

		result.Should().Be(TransferApprovalStatus.PendingNgbApproval);
	}
}
