using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Notification;
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
		var tournamentNamesById = await this.GetTournamentNamesByIdAsync(notifications, cancellationToken);

		var viewModels = notifications
			.Select(notification => this.MapToViewModel(notification, tournamentNamesById))
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

		var tournamentNamesById = await this.GetTournamentNamesByIdAsync(new[] { notification }, cancellationToken);
		var viewModel = this.MapToViewModel(notification, tournamentNamesById);

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
	private async Task<Dictionary<string, string>> GetTournamentNamesByIdAsync(
		IEnumerable<ManagementHub.Models.Data.Notification> notifications,
		CancellationToken cancellationToken)
	{
		var tournamentIds = notifications
			.Where(n => n.Type == (int)NotificationType.ManagerAssignment)
			.Where(n => string.Equals(n.RelatedEntityType, "Tournament", StringComparison.Ordinal))
			.Select(n => n.RelatedEntityId)
			.Where(id => !string.IsNullOrWhiteSpace(id))
			.Select(id => id!)
			.Distinct()
			.ToList();

		if (tournamentIds.Count == 0)
		{
			return new Dictionary<string, string>();
		}

		return await this.dbContext.Tournaments
			.Where(t => t.UniqueId != null && tournamentIds.Contains(t.UniqueId))
			.Select(t => new { t.UniqueId, t.Name })
			.ToDictionaryAsync(t => t.UniqueId!, t => t.Name, cancellationToken);
	}

	private NotificationViewModel MapToViewModel(
		ManagementHub.Models.Data.Notification notification,
		IReadOnlyDictionary<string, string> tournamentNamesById)
	{
		var message = notification.Message;
		if (notification.Type == (int)NotificationType.ManagerAssignment
			&& string.Equals(notification.RelatedEntityType, "Tournament", StringComparison.Ordinal)
			&& !string.IsNullOrWhiteSpace(notification.RelatedEntityId)
			&& tournamentNamesById.TryGetValue(notification.RelatedEntityId, out var tournamentName))
		{
			message = $"You can now manage tournament {tournamentName}.";
		}

		return new NotificationViewModel
		{
			Id = notification.Id.ToString(),
			Type = ((ManagementHub.Models.Domain.Notification.NotificationType)notification.Type).ToString(),
			Title = notification.Title,
			Message = message,
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
