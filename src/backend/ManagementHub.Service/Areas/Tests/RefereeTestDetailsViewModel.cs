using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Service.Areas.Tests;

public class RefereeTestDetailsViewModel
{
	public required TestIdentifier TestId { get; set; }
	public required string Title { get; set; }
	public required IEnumerable<Certification> AwardedCertifications { get; set; }
	public required LanguageIdentifier Language { get; set; }
	public required bool IsRefereeEligible { get; set; }
	public required TimeSpan TimeLimit { get; set; }
	public required string Description { get; set; }
	public required int MaximumAttempts { get; set; }
	public required Percentage PassPercentage { get; set; }
	public required int QuestionsCount { get; set; }
}
