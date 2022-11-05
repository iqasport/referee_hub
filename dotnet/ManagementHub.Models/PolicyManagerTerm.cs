using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class PolicyManagerTerm
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
