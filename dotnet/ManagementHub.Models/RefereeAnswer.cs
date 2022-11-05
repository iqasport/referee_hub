using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class RefereeAnswer
    {
        public long Id { get; set; }
        public long RefereeId { get; set; }
        public long TestId { get; set; }
        public long QuestionId { get; set; }
        public long AnswerId { get; set; }
        public long TestAttemptId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Answer Answer { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        public virtual User Referee { get; set; } = null!;
        public virtual Test Test { get; set; } = null!;
        public virtual TestAttempt TestAttempt { get; set; } = null!;
    }
}
