using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data
{
	public partial class PolicyManagerUserTerm : IIdentifiable
	{
		public long Id { get; set; }
		public long? UserId { get; set; }
		public long? TermId { get; set; }
		public string? State { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public virtual PolicyManagerTerm? Term { get; set; }
		public virtual User? User { get; set; }
	}
}
