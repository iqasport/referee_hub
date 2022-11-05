using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class ExportedCsv
    {
        public long Id { get; set; }
        public string? Type { get; set; }
        public int UserId { get; set; }
        public string? Url { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string ExportOptions { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
