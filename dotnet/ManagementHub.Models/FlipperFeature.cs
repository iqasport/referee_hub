using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class FlipperFeature
    {
        public long Id { get; set; }
        public string Key { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
