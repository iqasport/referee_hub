using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models
{
	public partial class PolicyManagerTerm : IIdentifiable
	{
		public PolicyManagerTerm()
		{
			PolicyManagerUserTerms = new HashSet<PolicyManagerUserTerm>();
		}

		public long Id { get; set; }
		public string? Description { get; set; }
		public string? Rule { get; set; }
		public string? State { get; set; }
		public DateTime? AcceptedAt { get; set; }
		public DateTime? RejectedAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public virtual ICollection<PolicyManagerUserTerm> PolicyManagerUserTerms { get; set; }
	}
}
