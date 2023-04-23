namespace ManagementHub.Models.Domain.Tests;

public class Answer
{
	/// <summary>
	/// Id of an answer (unique within a test).
	/// </summary>
	public required long AnswerId { get; set; }

	/// <summary>
	/// The answer text - Html formatted.
	/// </summary>
	public required string HtmlText { get; set; }
}
