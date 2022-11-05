using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class TestResult
    {
        public long Id { get; set; }
        public long RefereeId { get; set; }
        public TimeOnly? TimeStarted { get; set; }
        public TimeOnly? TimeFinished { get; set; }
        public string? Duration { get; set; }
        public int? Percentage { get; set; }
        public int? PointsScored { get; set; }
        public int? PointsAvailable { get; set; }
        public bool? Passed { get; set; }
        public string? CertificateUrl { get; set; }
        public int? MinimumPassPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? TestLevel { get; set; }
        public long? TestId { get; set; }

        public virtual User Referee { get; set; } = null!;
        public virtual Test? Test { get; set; }
    }
}
