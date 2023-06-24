using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Tests;

public class TestAttemptViewModel
{
	/// <summary>
	/// Id of this attempt.
	/// </summary>
	public required TestAttemptIdentifier AttemptId { get; set; }

	/// <summary>
	/// Identifier of the attempted test.
	/// </summary>
	public required TestIdentifier TestId { get; set; }

	/// <summary>
	/// Certification level (highest level awardable by the test).
	/// </summary>
	public required CertificationLevel Level { get; set; }

	/// <summary>
	/// When the attempt was started.
	/// </summary>
	public required DateTime StartedAt { get; set; }

	/// <summary>
	/// Whether the test attempt is still in progress.
	/// </summary>
	public bool IsInProgress => this.FinishedAt is null;

	/// <summary>
	/// When the attempt was finished (either through submission or timeout).
	/// </summary>
	public DateTime? FinishedAt { get; set; }

	/// <summary>
	/// How the attempt was finished.
	/// </summary>
	public TestAttemptFinishMethod? FinishMethod { get; set; }

	/// <summary>
	/// Score of the finished attempt.
	/// </summary>
	public Percentage? Score { get; set; }

	/// <summary>
	/// Minimum score required to pass.
	/// </summary>
	public Percentage? PassPercentage { get; set; }

	/// <summary>
	/// Whether the score was enough to pass the test. (saved in case the test was later modified)
	/// </summary>
	public bool? Passed { get; set; }

	/// <summary>
	/// New certifications the referee was awarded with this attempt if passed.
	/// </summary>
	public ISet<Certification>? AwardedCertifications { get; set; }

	/// <summary>
	/// Duration of the test.
	/// </summary>
	public TimeSpan? Duration => this.FinishedAt - this.StartedAt;

	public static TestAttemptViewModel FromTestAttempt(TestAttempt attempt)
	{
		var model = new TestAttemptViewModel
		{
			AttemptId = attempt.Id,
			TestId = attempt.TestId,
			Level = attempt.Level,
			StartedAt = attempt.StartedAt,
		};

		if (attempt is FinishedTestAttempt finished)
		{
			model.FinishedAt = finished.FinishedAt;
			model.FinishMethod = finished.FinishMethod;
			model.Score = finished.Score;
			model.PassPercentage = finished.PassPercentage;
			model.Passed = finished.Passed;
			model.AwardedCertifications = finished.AwardedCertifications;
		}

		return model;
	}
}
