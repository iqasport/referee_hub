using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class TeamManager : IIdentifiable
{
	public long Id { get; set; }
	public long TeamId { get; set; }
	public long UserId { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public virtual Team Team { get; set; } = null!;
	public virtual User User { get; set; } = null!;
}
