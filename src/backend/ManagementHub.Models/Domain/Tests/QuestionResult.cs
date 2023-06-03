namespace ManagementHub.Models.Domain.Tests;

/// <summary>
/// Like <see cref="Question"/> but only used to indicate result.
/// </summary>
public class QuestionResult
{
	/// <summary>
	/// The question text - Html formatted.
	/// </summary>
	public required string HtmlText { get; set; }

	/// <summary>
	/// Amount of points this question contributes overall.
	/// </summary>
	public int Points { get; set; } = 1;

	/// <summary>
	/// Whether the question was answered correctly during the test.
	/// </summary>
	public required bool AnsweredCorrectly { get; set; }

	/// <summary>
	/// (optional) Feedback - where to look in the rulebook for the answer or smth.
	/// </summary>
	public string? Feedback { get; set; }
}
