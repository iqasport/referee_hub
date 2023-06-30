namespace ManagementHub.Service.Areas.Tests;

public class RefereeTestStartModel
{
	public required IEnumerable<Question> Questions { get; set; }

	public class Question
	{
		public required long QuestionId { get; set; }
		public required string HtmlText { get; set; }
		public required IEnumerable<Answer> Answers { get; set; }
	}

	public class Answer
	{
		public required long AnswerId { get; set; }
		public required string HtmlText { get; set; }
	}
}
