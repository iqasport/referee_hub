using System.Collections.Generic;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests.Policies;

namespace ManagementHub.Models.Domain.Tests;

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
	public required LanguageIdentifier LanguageId { get; set; }

	/// <summary>
	/// Whether the test if active and can be attempted by the referees.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// List of policies describing how the test can be taken, what is it resulting in, etc.
	/// </summary>
	public required IEnumerable<ITestPolicy> TestPolicies { get; set; }
}
