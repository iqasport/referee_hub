using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class Question
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
            RefereeAnswers = new HashSet<RefereeAnswer>();
        }

        public long Id { get; set; }
        public long TestId { get; set; }
        public string Description { get; set; } = null!;
        public int PointsAvailable { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Test Test { get; set; } = null!;
        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
    }
}
