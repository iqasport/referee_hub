using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Notification;
using ManagementHub.Storage;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = ManagementHub.Models.Data.Notification;

namespace ManagementHub.Service.Services;

/// <summary>
/// Service for managing notifications.
/// </summary>
public interface INotificationService
{
	/// <summary>
	/// Creates a new notification for a user.
	/// </summary>
	Task<NotificationEntity> CreateNotificationAsync(
		long userId,
		NotificationType type,
		NotificationGroupType groupType,
		string title,
		string message,
		string? relatedEntityId = null,
		string? relatedEntityType = null,
		string? secondaryEntityId = null,
		string? secondaryEntityType = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets active (non-archived) notifications for a user, grouped if applicable.
	/// </summary>
	Task<IEnumerable<NotificationEntity>> GetActiveNotificationsAsync(
		long userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets unread notification count for a user.
	/// </summary>
	Task<int> GetUnreadCountAsync(
		long userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Marks a notification as read.
	/// </summary>
	Task<NotificationEntity?> MarkAsReadAsync(
		long userId,
		long notificationId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Marks all notifications as read for a user.
	/// </summary>
	Task<int> MarkAllAsReadAsync(
		long userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes (soft-deletes) a notification.
	/// </summary>
	Task<bool> DeleteNotificationAsync(
		long userId,
		long notificationId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Archives notifications older than 30 days. Called by scheduled job.
	/// </summary>
	Task ArchiveOldNotificationsAsync(
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of INotificationService.
/// </summary>
public class NotificationService : INotificationService
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<NotificationService> logger;

	public NotificationService(ManagementHubDbContext dbContext, ILogger<NotificationService> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public async Task<NotificationEntity> CreateNotificationAsync(
		long userId,
		NotificationType type,
		NotificationGroupType groupType,
		string title,
		string message,
		string? relatedEntityId = null,
		string? relatedEntityType = null,
		string? secondaryEntityId = null,
		string? secondaryEntityType = null,
		CancellationToken cancellationToken = default)
	{
		var notification = new NotificationEntity
		{
			UserId = userId,
			UniqueId = NotificationIdentifier.NewNotificationId().ToString(),
			Type = (int)type,
			GroupType = (int)groupType,
			Title = title,
			Message = message,
			RelatedEntityId = relatedEntityId,
			RelatedEntityType = relatedEntityType,
			SecondaryEntityId = secondaryEntityId,
			SecondaryEntityType = secondaryEntityType,
			IsRead = false,
			IsArchived = false,
			CreatedAt = DateTime.UtcNow,
		};

		this.dbContext.Notifications.Add(notification);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation(
			"Created notification {NotificationId} for user {UserId} with type {Type}",
			notification.UniqueId,
			userId,
			type);

		return notification;
	}

	public async Task<IEnumerable<NotificationEntity>> GetActiveNotificationsAsync(
		long userId,
		CancellationToken cancellationToken = default)
	{
		var notifications = await this.dbContext.Notifications
			.Where(n => n.UserId == userId && !n.IsArchived)
			.OrderByDescending(n => n.CreatedAt)
			.ToListAsync(cancellationToken);

		return notifications;
	}

	public async Task<int> GetUnreadCountAsync(
		long userId,
		CancellationToken cancellationToken = default)
	{
		return await this.dbContext.Notifications
			.Where(n => n.UserId == userId && !n.IsArchived && !n.IsRead)
			.CountAsync(cancellationToken);
	}

	public async Task<NotificationEntity?> MarkAsReadAsync(
		long userId,
		long notificationId,
		CancellationToken cancellationToken = default)
	{
		var notification = await this.dbContext.Notifications
			.FirstOrDefaultAsync(
				n => n.Id == notificationId && n.UserId == userId,
				cancellationToken);

		if (notification is null)
			return null;

		notification.IsRead = true;
		notification.ReadAt = DateTime.UtcNow;

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation(
			"Marked notification {NotificationId} as read for user {UserId}",
			notification.UniqueId,
			userId);

		return notification;
	}

	public async Task<int> MarkAllAsReadAsync(
		long userId,
		CancellationToken cancellationToken = default)
	{
		var unreadNotifications = await this.dbContext.Notifications
			.Where(n => n.UserId == userId && !n.IsArchived && !n.IsRead)
			.ToListAsync(cancellationToken);

		foreach (var notification in unreadNotifications)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.UtcNow;
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation(
			"Marked {Count} notifications as read for user {UserId}",
			unreadNotifications.Count,
			userId);

		return unreadNotifications.Count;
	}

	public async Task<bool> DeleteNotificationAsync(
		long userId,
		long notificationId,
		CancellationToken cancellationToken = default)
	{
		var notification = await this.dbContext.Notifications
			.FirstOrDefaultAsync(
				n => n.Id == notificationId && n.UserId == userId,
				cancellationToken);

		if (notification is null)
			return false;

		notification.IsArchived = true;
		notification.ArchivedAt = DateTime.UtcNow;

		await this.dbContext.SaveChangesAsync(cancellationToken);

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
			.Where(n => !n.IsArchived && n.CreatedAt < thirtyDaysAgo)
			.ToListAsync(cancellationToken);

		foreach (var notification in oldNotifications)
		{
			notification.IsArchived = true;
			notification.ArchivedAt = DateTime.UtcNow;
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation(
			"Archived {Count} notifications older than 30 days",
			oldNotifications.Count);
	}
}
