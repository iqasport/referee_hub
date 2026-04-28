using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Service.Hubs;
using ManagementHub.Service.Services;
using ManagementHub.Service.ViewModels;
using ManagementHub.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
	private readonly ManagementHubDbContext dbContext;
	private readonly INotificationService notificationService;
	private readonly IHubContext<NotificationsHub> hubContext;
	private readonly ILogger<NotificationsController> logger;

	public NotificationsController(
		ManagementHubDbContext dbContext,
		INotificationService notificationService,
		IHubContext<NotificationsHub> hubContext,
		ILogger<NotificationsController> logger)
	{
		this.dbContext = dbContext;
		this.notificationService = notificationService;
		this.hubContext = hubContext;
		this.logger = logger;
	}

	/// <summary>
	/// Gets all active (non-archived) notifications for the current user.
	/// </summary>
	[HttpGet]
	[Tags("Notifications")]
	public async Task<ActionResult<NotificationListResponse>> GetNotifications(CancellationToken cancellationToken)
	{
		var userIdClaim = this.User.FindFirst("sub")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
			return this.Unauthorized();

		var notifications = await this.notificationService.GetActiveNotificationsAsync(userId, cancellationToken);
		var unreadCount = await this.notificationService.GetUnreadCountAsync(userId, cancellationToken);

		var viewModels = notifications.Select(MapToViewModel).ToList();

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
		var userIdClaim = this.User.FindFirst("sub")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
			return this.Unauthorized();

		var unreadCount = await this.notificationService.GetUnreadCountAsync(userId, cancellationToken);

		return this.Ok(new { unreadCount });
	}

	/// <summary>
	/// Marks a specific notification as read.
	/// </summary>
	[HttpPatch("{id}/read")]
	[Tags("Notifications")]
	public async Task<ActionResult<NotificationViewModel>> MarkAsRead(
		long id,
		CancellationToken cancellationToken)
	{
		var userIdClaim = this.User.FindFirst("sub")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
			return this.Unauthorized();

		var notification = await this.notificationService.MarkAsReadAsync(userId, id, cancellationToken);
		if (notification is null)
			return this.NotFound();

		var viewModel = MapToViewModel(notification);

		// Notify via SignalR
		await this.hubContext.Clients.User(userId.ToString())
			.SendAsync("NotificationRead", viewModel, cancellationToken);

		return this.Ok(viewModel);
	}

	/// <summary>
	/// Marks all notifications as read for the current user.
	/// </summary>
	[HttpPatch("read-all")]
	[Tags("Notifications")]
	public async Task<ActionResult<object>> MarkAllAsRead(CancellationToken cancellationToken)
	{
		var userIdClaim = this.User.FindFirst("sub")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
			return this.Unauthorized();

		var count = await this.notificationService.MarkAllAsReadAsync(userId, cancellationToken);

		// Notify via SignalR
		await this.hubContext.Clients.User(userId.ToString())
			.SendAsync("AllNotificationsRead", cancellationToken);

		return this.Ok(new { markedAsReadCount = count });
	}

	/// <summary>
	/// Deletes (archives) a notification.
	/// </summary>
	[HttpDelete("{id}")]
	[Tags("Notifications")]
	public async Task<IActionResult> DeleteNotification(
		long id,
		CancellationToken cancellationToken)
	{
		var userIdClaim = this.User.FindFirst("sub")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
			return this.Unauthorized();

		var success = await this.notificationService.DeleteNotificationAsync(userId, id, cancellationToken);
		if (!success)
			return this.NotFound();

		// Notify via SignalR
		await this.hubContext.Clients.User(userId.ToString())
			.SendAsync("NotificationDeleted", id, cancellationToken);

		return this.Ok();
	}

	/// <summary>
	/// Maps Notification entity to ViewModel.
	/// </summary>
	private static NotificationViewModel MapToViewModel(ManagementHub.Models.Data.Notification notification)
	{
		return new NotificationViewModel
		{
			Id = notification.UniqueId ?? notification.Id.ToString(),
			Type = ((ManagementHub.Models.Domain.Notification.NotificationType)notification.Type).ToString(),
			Title = notification.Title,
			Message = notification.Message,
			RelatedEntityId = notification.RelatedEntityId,
			RelatedEntityType = notification.RelatedEntityType,
			SecondaryEntityId = notification.SecondaryEntityId,
			SecondaryEntityType = notification.SecondaryEntityType,
			IsRead = notification.IsRead,
			CreatedAt = notification.CreatedAt,
			ReadAt = notification.ReadAt,
		};
	}
}
