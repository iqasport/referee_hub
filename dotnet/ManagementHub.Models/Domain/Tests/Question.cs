using System.Collections.Generic;

namespace ManagementHub.Models.Domain.Tests;

/// <summary>
/// A question that is part of a test.
/// </summary>
public class Question
{
	/// <summary>
	/// Identifier of the question (unique within a test).
	/// </summary>
	public required long QuestionId { get; set; }

	/// <summary>
	/// The question text - Html formatted.
	/// </summary>
	public required string HtmlText { get; set; }

	/// <summary>
	/// Amount of points this question contributes overall.
	/// </summary>
	public int Points { get; set; } = 1;

	/// <summary>
	/// Answers for this question (should be 4 of them).
	/// </summary>
	public required ISet<Answer> Answers { get; set; }

	/// <summary>
	/// Set of correct answers - if any of these is selected the question will be awared the points.
	/// </summary>
	public required ISet<Answer> CorrectAnswers { get; set; }
}
