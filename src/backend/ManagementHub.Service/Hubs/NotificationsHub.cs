using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManagementHub.Service.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery.
/// 
/// Clients connect to receive real-time notification updates.
/// Messages:
/// - NotificationCreated: Sent when a new notification is created
/// - NotificationRead: Sent when a notification is marked as read
/// - AllNotificationsRead: Sent when all notifications are marked as read
/// - NotificationDeleted: Sent when a notification is archived
/// - UnreadCountChanged: Sent when unread count changes
/// </summary>
[Authorize]
public class NotificationsHub : Hub
{
	private readonly ILogger<NotificationsHub> logger;

	public NotificationsHub(ILogger<NotificationsHub> logger)
	{
		this.logger = logger;
	}

	/// <summary>
	/// Called when a client connects.
	/// </summary>
	public override async Task OnConnectedAsync()
	{
		var userId = this.Context.User?.FindFirst("sub")?.Value;
		if (userId is null)
		{
			this.logger.LogWarning("Notification hub connection attempted without valid userId");
			throw new HubException("Unauthorized");
		}

		// Add user to a group named after their user ID for targeted messaging
		await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"user-{userId}");

		this.logger.LogInformation(
			"User {UserId} connected to notifications hub. ConnectionId: {ConnectionId}",
			userId,
			this.Context.ConnectionId);

		await base.OnConnectedAsync();
	}

	/// <summary>
	/// Called when a client disconnects.
	/// </summary>
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var userId = this.Context.User?.FindFirst("sub")?.Value;
		this.logger.LogInformation(
			"User {UserId} disconnected from notifications hub. ConnectionId: {ConnectionId}",
			userId,
			this.Context.ConnectionId);

		await base.OnDisconnectedAsync(exception);
	}

	/// <summary>
	/// Notifies a specific user of a new notification.
	/// Called by the API when creating a notification.
	/// </summary>
	public async Task NotifyNewNotification(long userId, object notification)
	{
		await this.Clients.Group($"user-{userId}")
			.SendAsync("NotificationCreated", notification);
	}
}
