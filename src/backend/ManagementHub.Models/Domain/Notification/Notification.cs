using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.Notification;

/// <summary>
/// Represents an in-app notification for a user.
/// </summary>
public class Notification : IIdentifiable
{
	/// <summary>
	/// Database identifier (legacy/internal use).
	/// </summary>
	public long Id { get; set; }

	/// <summary>
	/// Strongly-typed identifier for the notification.
	/// </summary>
	public string? UniqueId { get; set; }

	/// <summary>
	/// User who receives this notification.
	/// </summary>
	public required long UserId { get; set; }

	/// <summary>
	/// Type of notification (exam result, invite, etc.)
	/// </summary>
	public required NotificationType Type { get; set; }

	/// <summary>
	/// How this notification should be grouped with similar ones.
	/// </summary>
	public required NotificationGroupType GroupType { get; set; }

	/// <summary>
	/// Display title for the notification.
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// Detailed message/description.
	/// </summary>
	public required string Message { get; set; }

	/// <summary>
	/// ID of the related entity (tournament ID, team ID, test ID, etc.)
	/// Format depends on entity type.
	/// </summary>
	public string? RelatedEntityId { get; set; }

	/// <summary>
	/// Type of related entity (Tournament, Team, Test, Manager, etc.)
	/// Used to determine navigation/link target.
	/// </summary>
	public string? RelatedEntityType { get; set; }

	/// <summary>
	/// Optional secondary entity ID for complex notifications (e.g., team within tournament).
	/// </summary>
	public string? SecondaryEntityId { get; set; }

	/// <summary>
	/// Type of secondary entity.
	/// </summary>
	public string? SecondaryEntityType { get; set; }

	/// <summary>
	/// Whether the user has read this notification.
	/// </summary>
	public required bool IsRead { get; set; }

	/// <summary>
	/// Whether this notification has been archived (soft-delete for >30 day old).
	/// </summary>
	public required bool IsArchived { get; set; }

	/// <summary>
	/// When the notification was created.
	/// </summary>
	public required DateTime CreatedAt { get; set; }

	/// <summary>
	/// When the notification was marked as read (if applicable).
	/// </summary>
	public DateTime? ReadAt { get; set; }

	/// <summary>
	/// When the notification was archived (if applicable).
	/// </summary>
	public DateTime? ArchivedAt { get; set; }
}
