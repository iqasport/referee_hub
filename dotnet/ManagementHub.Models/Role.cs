using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class Role
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public int AccessType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
