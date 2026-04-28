namespace ManagementHub.Models.Domain.Notification;

/// <summary>
/// Defines how notifications should be grouped. Similar notifications can be combined
/// (e.g., "5 new exam results" instead of 5 separate entries).
/// </summary>
public enum NotificationGroupType
{
	/// <summary>
	/// Do not group (show each notification separately)
	/// </summary>
	None,
	
	/// <summary>
	/// Group by type only (e.g., all exam results together)
	/// </summary>
	ByType,
	
	/// <summary>
	/// Group by type and related entity (e.g., all invites for tournament X)
	/// </summary>
	ByTypeAndEntity,
}
