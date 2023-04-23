using System;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests;
public class TestAttempt
{
	/// <summary>
	/// Id of this attempt. In the future should be used to remember which questions have been shown to the user so they can resume a test.
	/// </summary>
	public TestAttemptIdentifier Id { get; set; } = TestAttemptIdentifier.NewTestAttemptId();

	/// <summary>
	/// Identifier of the referee who made the attempt.
	/// </summary>
	public required UserIdentifier UserId { get; set; }

	/// <summary>
	/// Identifier of the attempted test.
	/// </summary>
	public required TestIdentifier TestId { get; set; }

	/// <summary>
	/// When the attempt was started.
	/// </summary>
	public required DateTime StartedAt { get; set; }
}
