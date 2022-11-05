using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class Answer
    {
        public Answer()
        {
            RefereeAnswers = new HashSet<RefereeAnswer>();
        }

        public long Id { get; set; }
        public long QuestionId { get; set; }
        public string Description { get; set; } = null!;
        public bool Correct { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Question Question { get; set; } = null!;
        public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
    }
}
