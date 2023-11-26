using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Models.Data;

[PrimaryKey(nameof(UserId), nameof(Key))]
public partial class UserAttribute
{
	public long UserId { get; set; }

	[MaxLength(128)]
	public required string Key { get; set; }

	[MaxLength(4096)]
	public required string Attribute { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime UpdatedAt { get; set; }
	
	public virtual User User { get; set; } = null!;
}
