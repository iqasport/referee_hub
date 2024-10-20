using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Service.Areas.Tests;

public class TestViewModel
{
	/// <summary>
	/// Title of the test (how it's displayed to users).
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// Block of text displayed to the refere before taking the test.
	/// </summary>
	public required string Description { get; set; }

	/// <summary>
	/// Language of the test.
	/// </summary>
	public required LanguageIdentifier Language { get; set; }

	/// <summary>
	/// The certification awarded for passing the test.
	/// </summary>
	public required Certification AwardedCertification { get; set; }

	/// <summary>
	///	Time limit in minutes.
	/// </summary>
	public required int TimeLimit { get; set; }

	/// <summary>
	/// Pass percentage.
	/// </summary>
	public required int PassPercentage { get; set; } = 80;

	/// <summary>
	/// How many questions to given to the referee during the test.
	/// </summary>
	public required int QuestionsCount { get; set; }

	/// <summary>
	/// If it's a recertification test for the previous rulebook.
	/// </summary>
	public bool Recertification { get; set; }

	/// <summary>
	/// Feedback to be displayed to the referee after the test if the pass.
	/// </summary>
	public string? PositiveFeedback { get; set; }

	/// <summary>
	/// Feedback to be displayed to the referee after the test if they fail.
	/// </summary>
	public string? NegativeFeedback { get; set; }

	/// <summary>
	/// Whether the test is active for all users.
	/// </summary>
	public bool Active { get; set; } = false;

	/// <summary>
	/// Identifier of the test.
	/// </summary>
	public TestIdentifier TestId { get; set; }
}
