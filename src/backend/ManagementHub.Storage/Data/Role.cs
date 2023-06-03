using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class Role : IIdentifiable
{
	public long Id { get; set; }
	public long? UserId { get; set; }
	public UserAccessType AccessType { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public virtual User? User { get; set; }
}
