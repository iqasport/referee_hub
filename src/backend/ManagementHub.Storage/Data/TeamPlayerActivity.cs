using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class TeamPlayerActivity : IIdentifiable
{
	public long Id { get; set; }
	public long TeamId { get; set; }
	public long? UserId { get; set; }
	public string Email { get; set; } = null!;
	public long InitiatorUserId { get; set; }
	public TeamPlayerActivityType ActivityType { get; set; }
	public DateTime CreatedAt { get; set; }

	public virtual Team Team { get; set; } = null!;
	public virtual User? User { get; set; }
	public virtual User Initiator { get; set; } = null!;
}