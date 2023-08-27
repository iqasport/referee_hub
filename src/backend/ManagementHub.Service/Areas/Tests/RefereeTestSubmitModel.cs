using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Service.Areas.Tests;

public class RefereeTestSubmitModel
{
	public required DateTime StartedAt { get; set; }

	[Required]
	public required IEnumerable<SubmittedTestAnswer> Answers { get; set; }

	public class SubmittedTestAnswer
	{
		public required long QuestionId { get; set; }
		public required long AnswerId { get; set; }

		public override string ToString() => $"{{ Q: {this.QuestionId}, A: {this.AnswerId} }}";
	}
}
