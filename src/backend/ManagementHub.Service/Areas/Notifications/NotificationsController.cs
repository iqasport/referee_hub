using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Notification;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Services;
using ManagementHub.Service.ViewModels;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Notifications;

/// <summary>
/// Manages user notifications.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly INotificationService notificationService;

	public NotificationsController(
		IUserContextAccessor contextAccessor,
		INotificationService notificationService)
	{
		this.contextAccessor = contextAccessor;
		this.notificationService = notificationService;
	}

	/// <summary>
	/// Gets all active (non-archived) notifications for the current user.
	/// </summary>
	[HttpGet]
	[Tags("Notifications")]
	public async Task<ActionResult<NotificationListResponse>> GetNotifications(CancellationToken cancellationToken)
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		var notifications = await this.notificationService.GetActiveNotificationsAsync(currentUser.UserId, cancellationToken);
		var unreadCount = await this.notificationService.GetUnreadCountAsync(currentUser.UserId, cancellationToken);
		var viewModels = notifications
			.Select(this.MapToViewModel)
			.ToList();

		return this.Ok(new NotificationListResponse
		{
			Notifications = viewModels,
			UnreadCount = unreadCount,
		});
	}

	/// <summary>
	/// Gets the count of unread notifications for the current user.
	/// </summary>
	[HttpGet("unread-count")]
	[Tags("Notifications")]
	public async Task<ActionResult<object>> GetUnreadCount(CancellationToken cancellationToken)
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		var unreadCount = await this.notificationService.GetUnreadCountAsync(currentUser.UserId, cancellationToken);

		return this.Ok(new { unreadCount });
	}

	/// <summary>
	/// Marks a specific notification as read.
	/// </summary>
	[HttpPatch("{id}/read")]
	[Tags("Notifications")]
	public async Task<ActionResult<NotificationViewModel>> MarkAsRead(
		string id,
		CancellationToken cancellationToken)
	{
		if (!NotificationIdentifier.TryParse(id, out var notificationId))
			return this.BadRequest();

		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		var notification = await this.notificationService.MarkAsReadAsync(currentUser.UserId, notificationId, cancellationToken);
		if (notification is null)
			return this.NotFound();

		var viewModel = this.MapToViewModel(notification);

		return this.Ok(viewModel);
	}

	/// <summary>
	/// Marks all notifications as read for the current user.
	/// </summary>
	[HttpPatch("read-all")]
	[Tags("Notifications")]
	public async Task<ActionResult<object>> MarkAllAsRead(CancellationToken cancellationToken)
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		var count = await this.notificationService.MarkAllAsReadAsync(currentUser.UserId, cancellationToken);

		return this.Ok(new { markedAsReadCount = count });
	}

	/// <summary>
	/// Deletes (archives) a notification.
	/// </summary>
	[HttpDelete("{id}")]
	[Tags("Notifications")]
	public async Task<IActionResult> DeleteNotification(
		string id,
		CancellationToken cancellationToken)
	{
		if (!NotificationIdentifier.TryParse(id, out var notificationId))
			return this.BadRequest();

		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		var success = await this.notificationService.DeleteNotificationAsync(currentUser.UserId, notificationId, cancellationToken);
		if (!success)
			return this.NotFound();

		return this.Ok();
	}

	private NotificationViewModel MapToViewModel(ManagementHub.Models.Data.Notification notification)
	{
		return new NotificationViewModel
		{
			Id = NotificationIdentifier.Parse(notification.UniqueId!),
			Type = notification.Type,
			Title = notification.Title,
			Message = notification.Message,
			RelatedEntityId = notification.RelatedEntityId,
			RelatedEntityType = notification.RelatedEntityType,
			SecondaryEntityId = notification.SecondaryEntityId,
			SecondaryEntityType = notification.SecondaryEntityType,
			IsRead = notification.ReadAt.HasValue,
			CreatedAt = notification.CreatedAt,
			ReadAt = notification.ReadAt,
		};
	}
}
