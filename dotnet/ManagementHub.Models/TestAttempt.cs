using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class TestAttempt
    {
        public TestAttempt()
        {
            RefereeAnswers = new HashSet<RefereeAnswer>();
        }

        public long Id { get; set; }
        public long? TestId { get; set; }
        public long? RefereeId { get; set; }
        public int? TestLevel { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User? Referee { get; set; }
        public virtual Test? Test { get; set; }
        public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
    }
}
