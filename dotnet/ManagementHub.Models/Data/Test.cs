using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data
{
	public partial class Test : IIdentifiable
	{
		public Test()
		{
			Questions = new HashSet<Question>();
			RefereeAnswers = new HashSet<RefereeAnswer>();
			TestAttempts = new HashSet<TestAttempt>();
			TestResults = new HashSet<TestResult>();
		}

		public long Id { get; set; }
		public TestLevel? Level { get; set; }
		public string? Name { get; set; }
		public long? CertificationId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string Description { get; set; } = null!;
		public int TimeLimit { get; set; }
		public int MinimumPassPercentage { get; set; }
		public string? PositiveFeedback { get; set; }
		public string? NegativeFeedback { get; set; }
		public string? Language { get; set; }
		public bool Active { get; set; }
		public int TestableQuestionCount { get; set; }
		public bool? Recertification { get; set; }
		public long? NewLanguageId { get; set; }

		public virtual Certification? Certification { get; set; }
		public virtual Language? NewLanguage { get; set; }
		public virtual ICollection<Question> Questions { get; set; }
		public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
		public virtual ICollection<TestAttempt> TestAttempts { get; set; }
		public virtual ICollection<TestResult> TestResults { get; set; }
	}
}
