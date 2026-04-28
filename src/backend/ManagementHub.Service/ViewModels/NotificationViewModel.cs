using System;

namespace ManagementHub.Service.ViewModels;

/// <summary>
/// Represents a notification for API responses.
/// </summary>
public class NotificationViewModel
{
	/// <summary>
	/// Strongly-typed notification identifier.
	/// </summary>
	public required string Id { get; init; }

	/// <summary>
	/// Notification type (e.g., ExamResult, TournamentInvite).
	/// </summary>
	public required string Type { get; init; }

	/// <summary>
	/// Display title.
	/// </summary>
	public required string Title { get; init; }

	/// <summary>
	/// Detailed message.
	/// </summary>
	public required string Message { get; init; }

	/// <summary>
	/// ID of related entity for navigation/action.
	/// </summary>
	public string? RelatedEntityId { get; init; }

	/// <summary>
	/// Type of related entity (Tournament, Team, etc.).
	/// </summary>
	public string? RelatedEntityType { get; init; }

	/// <summary>
	/// Secondary entity ID if applicable.
	/// </summary>
	public string? SecondaryEntityId { get; init; }

	/// <summary>
	/// Type of secondary entity.
	/// </summary>
	public string? SecondaryEntityType { get; init; }

	/// <summary>
	/// Whether the notification has been read.
	/// </summary>
	public required bool IsRead { get; init; }

	/// <summary>
	/// When the notification was created.
	/// </summary>
	public required DateTime CreatedAt { get; init; }

	/// <summary>
	/// When the notification was read (if applicable).
	/// </summary>
	public DateTime? ReadAt { get; init; }
}

/// <summary>
/// Response containing a list of notifications and metadata.
/// </summary>
public class NotificationListResponse
{
	/// <summary>
	/// List of notifications.
	/// </summary>
	public required IEnumerable<NotificationViewModel> Notifications { get; init; }

	/// <summary>
	/// Total count of unread notifications.
	/// </summary>
	public required int UnreadCount { get; init; }
}
