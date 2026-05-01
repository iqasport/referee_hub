using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Notification;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Hubs;
using ManagementHub.Storage;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using NotificationEntity = ManagementHub.Models.Data.Notification;

namespace ManagementHub.Service.Services;

/// <summary>
/// Implementation of INotificationService.
/// </summary>
public class NotificationService : INotificationService
{
	private readonly ManagementHubDbContext dbContext;
	private readonly IHubContext<NotificationsHub> hubContext;
	private readonly ILogger<NotificationService> logger;

	public NotificationService(
		ManagementHubDbContext dbContext,
		IHubContext<NotificationsHub> hubContext,
		ILogger<NotificationService> logger)
	{
		this.dbContext = dbContext;
		this.hubContext = hubContext;
		this.logger = logger;
	}

	public Task<NotificationEntity> CreateNgbAdminAssignmentNotificationAsync(
		UserIdentifier userId,
		NgbIdentifier ngb,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.ManagerAssignment,
			"You were added as NGB admin",
			$"You can now manage NGB {ngb}.",
			ngb.ToString(),
			"Ngb",
			cancellationToken: cancellationToken);

	public Task<NotificationEntity> CreateTeamManagerAssignmentNotificationAsync(
		UserIdentifier userId,
		TeamIdentifier teamId,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.ManagerAssignment,
			"You were added as team manager",
			$"You can now manage team {teamId}.",
			teamId.ToString(),
			"Team",
			cancellationToken: cancellationToken);

	public Task<NotificationEntity> CreateTournamentManagerAssignmentNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		string tournamentName,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.ManagerAssignment,
			"You were added as tournament manager",
			$"You can now manage tournament {tournamentName}.",
			tournamentId.ToString(),
			"Tournament",
			cancellationToken: cancellationToken);

	public Task<NotificationEntity> CreateTournamentInviteNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.TournamentInvite,
			"Your team was invited",
			$"{tournamentName} invited your team to join.",
			tournamentId.ToString(),
			"Tournament",
			teamId.ToString(),
			"Team",
			cancellationToken);

	public Task<NotificationEntity> CreateTeamTournamentJoinRequestNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.TeamTournamentJoinRequest,
			"New team join request",
			$"A team requested to join {tournamentName}.",
			tournamentId.ToString(),
			"Tournament",
			teamId.ToString(),
			"Team",
			cancellationToken);

	public Task<NotificationEntity> CreateRequestResponseNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		bool approved,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			approved ? NotificationType.RequestAccepted : NotificationType.RequestRejected,
			approved ? "Join request approved" : "Join request rejected",
			$"Your team's request for {tournamentName} was {(approved ? "approved" : "rejected")}.",
			tournamentId.ToString(),
			"Tournament",
			teamId.ToString(),
			"Team",
			cancellationToken);

	public Task<NotificationEntity> CreateInviteResponseNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		bool approved,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			approved ? NotificationType.InviteAccepted : NotificationType.InviteRejected,
			approved ? "Tournament invite accepted" : "Tournament invite rejected",
			$"Team {teamId} {(approved ? "accepted" : "rejected")} the invite to {tournamentName}.",
			tournamentId.ToString(),
			"Tournament",
			teamId.ToString(),
			"Team",
			cancellationToken);

	public Task<NotificationEntity> CreateRosterRegistrationNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		RosterRole role,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.RosterRegistration,
			"Tournament roster registration",
			$"You have been signed up to {tournamentName} as {GetRosterRoleDisplayName(role)}.",
			tournamentId.ToString(),
			"Tournament",
			teamId.ToString(),
			"Team",
			cancellationToken);

	public Task<NotificationEntity> CreateExamResultNotificationAsync(
		UserIdentifier userId,
		TestIdentifier testId,
		string testTitle,
		Percentage score,
		bool passed,
		CancellationToken cancellationToken = default) =>
		this.CreateNotificationCoreAsync(
			userId,
			NotificationType.ExamResult,
			"Exam result available",
			$"Your result for {testTitle} is {score.Value}% ({(passed ? "Passed" : "Failed")}).",
			testId.ToString(),
			"Test",
			cancellationToken: cancellationToken);

	private async Task<NotificationEntity> CreateNotificationCoreAsync(
		UserIdentifier userId,
		NotificationType type,
		string title,
		string message,
		string? relatedEntityId = null,
		string? relatedEntityType = null,
		string? secondaryEntityId = null,
		string? secondaryEntityType = null,
		CancellationToken cancellationToken = default)
	{
		var userDbId = await this.ResolveUserDbIdAsync(userId, cancellationToken);
		if (!userDbId.HasValue)
		{
			throw new InvalidOperationException($"Could not resolve database user for {nameof(UserIdentifier)} {userId}");
		}

		var notification = new NotificationEntity
		{
			UserId = userDbId.Value,
			UniqueId = NotificationIdentifier.NewNotificationId().ToString(),
			Type = type,
			Title = title,
			Message = message,
			RelatedEntityId = relatedEntityId,
			RelatedEntityType = relatedEntityType,
			SecondaryEntityId = secondaryEntityId,
			SecondaryEntityType = secondaryEntityType,
			CreatedAt = DateTime.UtcNow,
		};

		this.dbContext.Notifications.Add(notification);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		await this.hubContext.Clients.Group($"user-{userId}")
			.SendAsync("NotificationCreated", GetNotificationIdentifier(notification).ToString(), cancellationToken);
		await this.PublishUnreadCountAsync(userId, cancellationToken);

		this.logger.LogInformation(
			"Created notification {NotificationId} for user {UserId} with type {Type}",
			notification.UniqueId,
			userId,
			type);

		return notification;
	}

	public async Task<IEnumerable<NotificationEntity>> GetActiveNotificationsAsync(
		UserIdentifier userId,
		CancellationToken cancellationToken = default)
	{
		var userDbId = await this.ResolveUserDbIdAsync(userId, cancellationToken);
		if (!userDbId.HasValue)
			return Enumerable.Empty<NotificationEntity>();

		var notifications = await this.dbContext.Notifications
			.AsNoTracking()
			.Where(n => n.UserId == userDbId.Value && n.ArchivedAt == null)
			.OrderByDescending(n => n.CreatedAt)
			.ToListAsync(cancellationToken);

		return notifications;
	}

	public async Task<int> GetUnreadCountAsync(
		UserIdentifier userId,
		CancellationToken cancellationToken = default)
	{
		var userDbId = await this.ResolveUserDbIdAsync(userId, cancellationToken);
		if (!userDbId.HasValue)
			return 0;

		return await this.dbContext.Notifications
			.Where(n => n.UserId == userDbId.Value && n.ArchivedAt == null && n.ReadAt == null)
			.CountAsync(cancellationToken);
	}

	public async Task<NotificationEntity?> MarkAsReadAsync(
		UserIdentifier userId,
		NotificationIdentifier notificationId,
		CancellationToken cancellationToken = default)
	{
		var userDbId = await this.ResolveUserDbIdAsync(userId, cancellationToken);
		if (!userDbId.HasValue)
			return null;

		var notification = await this.dbContext.Notifications
			.FirstOrDefaultAsync(
				n => n.UniqueId == notificationId.ToString() && n.UserId == userDbId.Value,
				cancellationToken);

		if (notification is null)
			return null;

		notification.ReadAt ??= DateTime.UtcNow;

		await this.dbContext.SaveChangesAsync(cancellationToken);

		await this.hubContext.Clients.Group($"user-{userId}")
			.SendAsync("NotificationRead", GetNotificationIdentifier(notification).ToString(), cancellationToken);
		await this.PublishUnreadCountAsync(userId, cancellationToken);

		this.logger.LogInformation(
			"Marked notification {NotificationId} as read for user {UserId}",
			notification.UniqueId,
			userId);

		return notification;
	}

	public async Task<int> MarkAllAsReadAsync(
		UserIdentifier userId,
		CancellationToken cancellationToken = default)
	{
		var userDbId = await this.ResolveUserDbIdAsync(userId, cancellationToken);
		if (!userDbId.HasValue)
			return 0;

		var unreadNotifications = await this.dbContext.Notifications
			.Where(n => n.UserId == userDbId.Value && n.ArchivedAt == null && n.ReadAt == null)
			.ToListAsync(cancellationToken);

		foreach (var notification in unreadNotifications)
		{
			notification.ReadAt = DateTime.UtcNow;
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		await this.hubContext.Clients.Group($"user-{userId}")
			.SendAsync("AllNotificationsRead", cancellationToken);
		await this.PublishUnreadCountAsync(userId, cancellationToken);

		this.logger.LogInformation(
			"Marked {Count} notifications as read for user {UserId}",
			unreadNotifications.Count,
			userId);

		return unreadNotifications.Count;
	}

	public async Task<bool> DeleteNotificationAsync(
		UserIdentifier userId,
		NotificationIdentifier notificationId,
		CancellationToken cancellationToken = default)
	{
		var userDbId = await this.ResolveUserDbIdAsync(userId, cancellationToken);
		if (!userDbId.HasValue)
			return false;

		var notification = await this.dbContext.Notifications
			.FirstOrDefaultAsync(
				n => n.UniqueId == notificationId.ToString() && n.UserId == userDbId.Value,
				cancellationToken);

		if (notification is null)
			return false;

		notification.ArchivedAt ??= DateTime.UtcNow;

		await this.dbContext.SaveChangesAsync(cancellationToken);

		await this.hubContext.Clients.Group($"user-{userId}")
			.SendAsync("NotificationDeleted", GetNotificationIdentifier(notification).ToString(), cancellationToken);
		await this.PublishUnreadCountAsync(userId, cancellationToken);

		this.logger.LogInformation(
			"Archived notification {NotificationId} for user {UserId}",
			notification.UniqueId,
			userId);

		return true;
	}

	public async Task ArchiveOldNotificationsAsync(
		CancellationToken cancellationToken = default)
	{
		var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

		var oldNotifications = await this.dbContext.Notifications
			.Where(n => n.ArchivedAt == null && n.CreatedAt < thirtyDaysAgo)
			.ToListAsync(cancellationToken);

		foreach (var notification in oldNotifications)
		{
			notification.ArchivedAt = DateTime.UtcNow;
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation(
			"Archived {Count} notifications older than 30 days",
			oldNotifications.Count);
	}

	private async Task PublishUnreadCountAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		var unreadCount = await this.GetUnreadCountAsync(userId, cancellationToken);
		await this.hubContext.Clients.Group($"user-{userId}")
			.SendAsync("UnreadCountChanged", unreadCount, cancellationToken);
	}

	private async Task<long?> ResolveUserDbIdAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(u => (long?)u.Id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	private static NotificationIdentifier GetNotificationIdentifier(NotificationEntity notification)
	{
		if (!string.IsNullOrWhiteSpace(notification.UniqueId)
			&& NotificationIdentifier.TryParse(notification.UniqueId, out var notificationId))
		{
			return notificationId;
		}

		throw new InvalidOperationException($"Notification {notification.Id} is missing a valid unique identifier.");
	}

	private static string GetRosterRoleDisplayName(RosterRole role) => role switch
	{
		RosterRole.Player => "player",
		RosterRole.Coach => "coach",
		RosterRole.Staff => "staff",
		_ => "participant",
	};
}
