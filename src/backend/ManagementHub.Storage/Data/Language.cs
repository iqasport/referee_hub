using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class Language : IIdentifiable
{
	public Language()
	{
		this.Tests = new HashSet<Test>();
		this.Users = new HashSet<User>();
	}

	public long Id { get; set; }
	public string LongName { get; set; } = null!;
	public string ShortName { get; set; } = null!;
	public string? LongRegion { get; set; }
	public string? ShortRegion { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public virtual ICollection<Test> Tests { get; set; }
	public virtual ICollection<User> Users { get; set; }
}
