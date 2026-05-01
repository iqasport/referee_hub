using System;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Notification;

/// <summary>
/// Represents an in-app notification for a user.
/// </summary>
public abstract class Notification
{
	/// <summary>
	/// Strongly-typed identifier for this notification.
	/// </summary>
	public NotificationIdentifier Id { get; set; } = NotificationIdentifier.NewNotificationId();

	/// <summary>
	/// User who receives this notification.
	/// </summary>
	public required UserIdentifier UserId { get; set; }

	/// <summary>
	/// Type of notification (exam result, invite, etc.)
	/// </summary>
	public abstract NotificationType Type { get; }

	/// <summary>
	/// Display title for the notification.
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// Detailed message/description.
	/// </summary>
	public required string Message { get; set; }

	/// <summary>
	/// Whether the user has read this notification.
	/// </summary>
	public bool IsRead => this.ReadAt.HasValue;

	/// <summary>
	/// Whether this notification has been archived (soft-delete for >30 day old).
	/// </summary>
	public bool IsArchived => this.ArchivedAt.HasValue;

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

public sealed class ExamResultNotification : Notification
{
	public override NotificationType Type => NotificationType.ExamResult;
	public required TestIdentifier TestId { get; set; }
}

public sealed class TournamentInviteNotification : Notification
{
	public override NotificationType Type => NotificationType.TournamentInvite;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class TeamTournamentJoinRequestNotification : Notification
{
	public override NotificationType Type => NotificationType.TeamTournamentJoinRequest;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class ManagerAssignmentNotification : Notification
{
	public override NotificationType Type => NotificationType.ManagerAssignment;
	public required TournamentIdentifier TournamentId { get; set; }
	public required UserIdentifier ManagerId { get; set; }
}

public sealed class InviteAcceptedNotification : Notification
{
	public override NotificationType Type => NotificationType.InviteAccepted;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class InviteRejectedNotification : Notification
{
	public override NotificationType Type => NotificationType.InviteRejected;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class RequestAcceptedNotification : Notification
{
	public override NotificationType Type => NotificationType.RequestAccepted;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class RequestRejectedNotification : Notification
{
	public override NotificationType Type => NotificationType.RequestRejected;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class TeamApprovalNeededNotification : Notification
{
	public override NotificationType Type => NotificationType.TeamApprovalNeeded;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
}

public sealed class NgbApprovalNeededNotification : Notification
{
	public override NotificationType Type => NotificationType.NgbApprovalNeeded;
	public required TournamentIdentifier TournamentId { get; set; }
	public required NgbIdentifier NgbId { get; set; }
}

public sealed class RosterRegistrationNotification : Notification
{
	public override NotificationType Type => NotificationType.RosterRegistration;
	public required TournamentIdentifier TournamentId { get; set; }
	public required TeamIdentifier TeamId { get; set; }
	public required UserIdentifier RegisteredUserId { get; set; }
}
