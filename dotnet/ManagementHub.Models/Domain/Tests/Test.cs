using System;
using System.Collections.Generic;
using System.Diagnostics;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests.Policies;

namespace ManagementHub.Models.Domain.Tests;

[DebuggerDisplay("{Name} ({TestId})")]
public class Test
{
	public required TestIdentifier TestId { get; set; }

	/// <summary>
	/// Name of the test (how it's displayed to users).
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Block of text displayed to the refere before taking the test.
	/// </summary>
	public required string Description { get; set; }

	/// <summary>
	/// Language of the test.
	/// </summary>
	public required LanguageIdentifier Language { get; set; }

	/// <summary>
	/// Whether the test if active and can be attempted by the referees.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// Set of certifications awarded to the referee upon passing the test.
	/// </summary>
	public required IEnumerable<Certification> AwardedCertifications { get; set; }

	/// <summary>
	/// Time limit for the test (can be overriden for user with accessibility needs).
	/// </summary>
	public required TimeSpan TimeLimit { get; set; }

	/// <summary>
	/// Percentage of questions that have to be answered correctly for the referee to pass this test.
	/// </summary>
	public required Percentage PassPercentage { get; set; }

	/// <summary>
	/// Policy determining which questions are chosen to be presented to the referee.
	/// </summary>
	public required IQuestionChoicePolicy QuestionChoicePolicy { get; set; }

	/// <summary>
	/// (optional) If set, indicates this test is a recertification test for referees who held the returned certification previosly.
	/// </summary>
	public Certification? RecertificationFor { get; set; }

	/// <summary>
	/// Maximum attempts at this test. TODO: make it configurable in database.
	/// </summary>
	public int MaximumAttempts { get; set; } = 6;
}
