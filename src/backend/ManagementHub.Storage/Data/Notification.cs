using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Notification;

namespace ManagementHub.Models.Data;

/// <summary>
/// EF Core entity for notifications.
/// </summary>
public partial class Notification : IIdentifiable
{
	public long Id { get; set; }

	public string? UniqueId { get; set; }

	public long UserId { get; set; }

	public NotificationType Type { get; set; }

	public string Title { get; set; } = null!;

	public string Message { get; set; } = null!;

	public string? RelatedEntityId { get; set; }

	public string? RelatedEntityType { get; set; }

	public string? SecondaryEntityId { get; set; }

	public string? SecondaryEntityType { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime? ReadAt { get; set; }

	public DateTime? ArchivedAt { get; set; }

	public virtual User? User { get; set; }
}
