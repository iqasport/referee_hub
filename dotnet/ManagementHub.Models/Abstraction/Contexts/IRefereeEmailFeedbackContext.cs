using System.Collections.Generic;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Models.Abstraction.Contexts;

/// <summary>
/// Context for the feedback email for the referee after finishing a test.
/// </summary>
public interface IRefereeEmailFeedbackContext
{
	/// <summary>
	/// Test details.
	/// </summary>
	Test Test { get; }

	/// <summary>
	/// Details of the test attempt.
	/// </summary>
	FinishedTestAttempt TestAttempt { get; }

	/// <summary>
	/// Results per question.
	/// </summary>
	IEnumerable<QuestionResult> QuestionResults { get; }
}
