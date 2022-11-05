using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class TeamStatusChangeset
    {
        public long Id { get; set; }
        public long? TeamId { get; set; }
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Team? Team { get; set; }
    }
}
