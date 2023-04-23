using System.Collections.Generic;

namespace ManagementHub.Models.Domain.Tests;

/// <summary>
/// A test with additionally loaded questions.
/// </summary>
public class TestWithQuestions : Test
{
	/// <summary>
	/// List of questions available to be selected as part of this test.
	/// </summary>
	public required IEnumerable<Question> AvailableQuestions { get; set; }
}
