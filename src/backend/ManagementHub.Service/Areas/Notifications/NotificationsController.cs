using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Service.Services;
using ManagementHub.Service.ViewModels;
using ManagementHub.Service.Contexts;
using ManagementHub.Storage;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
	private readonly IUserContextAccessor contextAccessor;
	private readonly ManagementHubDbContext dbContext;
	private readonly INotificationService notificationService;

	public NotificationsController(
		IUserContextAccessor contextAccessor,
		ManagementHubDbContext dbContext,
		INotificationService notificationService)
	{
		this.contextAccessor = contextAccessor;
		this.dbContext = dbContext;
		this.notificationService = notificationService;
	}

	/// <summary>
	/// Gets all active (non-archived) notifications for the current user.
	/// </summary>
	[HttpGet]
	[Tags("Notifications")]
	public async Task<ActionResult<NotificationListResponse>> GetNotifications(CancellationToken cancellationToken)
	{
		var userDbId = await this.GetCurrentUserDbIdAsync(cancellationToken);
		if (!userDbId.HasValue)
			return this.Unauthorized();

		var notifications = await this.notificationService.GetActiveNotificationsAsync(userDbId.Value, cancellationToken);
		var unreadCount = await this.notificationService.GetUnreadCountAsync(userDbId.Value, cancellationToken);

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
		var userDbId = await this.GetCurrentUserDbIdAsync(cancellationToken);
		if (!userDbId.HasValue)
			return this.Unauthorized();

		var unreadCount = await this.notificationService.GetUnreadCountAsync(userDbId.Value, cancellationToken);

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
		var userDbId = await this.GetCurrentUserDbIdAsync(cancellationToken);
		if (!userDbId.HasValue)
			return this.Unauthorized();

		var notification = await this.notificationService.MarkAsReadAsync(userDbId.Value, id, cancellationToken);
		if (notification is null)
			return this.NotFound();

		var viewModel = MapToViewModel(notification);

		return this.Ok(viewModel);
	}

	/// <summary>
	/// Marks all notifications as read for the current user.
	/// </summary>
	[HttpPatch("read-all")]
	[Tags("Notifications")]
	public async Task<ActionResult<object>> MarkAllAsRead(CancellationToken cancellationToken)
	{
		var userDbId = await this.GetCurrentUserDbIdAsync(cancellationToken);
		if (!userDbId.HasValue)
			return this.Unauthorized();

		var count = await this.notificationService.MarkAllAsReadAsync(userDbId.Value, cancellationToken);

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
		var userDbId = await this.GetCurrentUserDbIdAsync(cancellationToken);
		if (!userDbId.HasValue)
			return this.Unauthorized();

		var success = await this.notificationService.DeleteNotificationAsync(userDbId.Value, id, cancellationToken);
		if (!success)
			return this.NotFound();

		return this.Ok();
	}

	private async Task<long?> GetCurrentUserDbIdAsync(CancellationToken cancellationToken)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		return await this.dbContext.Users
			.WithIdentifier(userContext.UserId)
			.Select(u => (long?)u.Id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	/// <summary>
	/// Maps Notification entity to ViewModel.
	/// </summary>
	private static NotificationViewModel MapToViewModel(ManagementHub.Models.Data.Notification notification)
	{
		return new NotificationViewModel
		{
			Id = notification.Id.ToString(),
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
