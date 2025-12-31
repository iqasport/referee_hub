using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class UserDelicateInfo : IIdentifiable
{
	public long Id { get; set; }
	public required long UserId { get; set; }
	public string? Gender { get; set; }
	public DateTime UpdatedAt { get; set; }
	public DateTime CreatedAt { get; set; }

	public virtual User User { get; set; } = null!;
}
