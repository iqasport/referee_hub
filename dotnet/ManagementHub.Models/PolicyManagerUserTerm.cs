using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class PolicyManagerUserTerm
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
