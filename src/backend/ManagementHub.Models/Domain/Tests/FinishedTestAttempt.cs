using System;
using System.Collections.Generic;

namespace ManagementHub.Models.Domain.Tests;

public class FinishedTestAttempt : TestAttempt
{
	/// <summary>
	/// When the attempt was finished (either through submission or timeout).
	/// </summary>
	public required DateTime FinishedAt { get; set; }

	/// <summary>
	/// How the attempt was finished.
	/// </summary>
	public required TestAttemptFinishMethod FinishMethod { get; set; }

	/// <summary>
	/// Score of the finished attempt.
	/// </summary>
	public required Percentage Score { get; set; }

	/// <summary>
	/// Minimum score required to pass.
	/// </summary>
	public required Percentage PassPercentage { get; set; }

	/// <summary>
	/// Whether the score was enough to pass the test. (saved in case the test was later modified)
	/// </summary>
	public required bool Passed { get; set; }

	/// <summary>
	/// New certifications the referee was awarded with this attempt if passed.
	/// </summary>
	public ISet<Certification>? AwardedCertifications { get; set; }

	/// <summary>
	/// Duration of the test.
	/// </summary>
	public TimeSpan Duration => this.FinishedAt - this.StartedAt;
}
