using System;
using ManagementHub.Service.Areas.Teams;

namespace ManagementHub.Service.Areas.Ngbs;

/// <summary>
/// Represents a player transfer visible to an NGB admin.
/// </summary>
public class NgbTransferViewModel
{
	/// <summary>Transfer invitation identifier (prefixed string).</summary>
	public required string InvitationId { get; init; }

	/// <summary>Player email address.</summary>
	public required string PlayerEmail { get; init; }

	/// <summary>Player display name (if known).</summary>
	public string? PlayerName { get; init; }

	/// <summary>ID of the destination (joining) team.</summary>
	public required string DestinationTeamId { get; init; }

	/// <summary>Name of the destination team.</summary>
	public required string DestinationTeamName { get; init; }

	/// <summary>Logo of the destination team, when available.</summary>
	public Uri? DestinationTeamLogoUri { get; init; }

	/// <summary>Country code of the destination team's NGB.</summary>
	public string? DestinationNgbCode { get; init; }

	/// <summary>ID of the origin (leaving) team, when known.</summary>
	public string? OriginTeamId { get; init; }

	/// <summary>Name of the origin team, when known.</summary>
	public string? OriginTeamName { get; init; }

	/// <summary>Logo of the origin team, when available.</summary>
	public Uri? OriginTeamLogoUri { get; init; }

	/// <summary>Country code of the origin team's NGB, when known.</summary>
	public string? OriginNgbCode { get; init; }

	/// <summary>True when both teams belong to this NGB (internal transfer).</summary>
	public bool IsInternalTransfer { get; init; }

	/// <summary>When this NGB approved the transfer. Null if not yet reviewed.</summary>
	public DateTime? ApprovedAt { get; init; }

	/// <summary>When this NGB rejected the transfer. Null if not yet reviewed.</summary>
	public DateTime? RejectedAt { get; init; }

	/// <summary>When the invite was created.</summary>
	public DateTime CreatedAt { get; init; }

	/// <summary>Current transfer approval status from this NGB's perspective.</summary>
	public required TransferApprovalStatus Status { get; init; }
}
