using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data
{
	public partial class TestAttempt : IIdentifiable
	{
		public TestAttempt()
		{
			RefereeAnswers = new HashSet<RefereeAnswer>();
		}

		public long Id { get; set; }
		public string? UniqueId { get; set; }
		public long? TestId { get; set; }
		public long? RefereeId { get; set; }
		public TestLevel? TestLevel { get; set; }
		public DateTime? NextAttemptAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public virtual User? Referee { get; set; }
		public virtual Test? Test { get; set; }
		public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
	}
}
