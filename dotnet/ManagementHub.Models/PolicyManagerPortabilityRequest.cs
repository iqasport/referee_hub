using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models
{
	public partial class PolicyManagerPortabilityRequest : IIdentifiable
	{
		public long Id { get; set; }
		public long? UserId { get; set; }
		public string? State { get; set; }
		public DateTime? ExpireAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public virtual User? User { get; set; }
	}
}
