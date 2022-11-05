using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class FlipperGate
    {
        public long Id { get; set; }
        public string FeatureKey { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
