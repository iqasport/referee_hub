using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Service.Areas.Tests;

public class RefereeTestSubmitResponse
{
	public required bool Passed { get; set; }

	public required Percentage PassPercentage { get; set; }

	public required Percentage ScoredPercentage { get; set; }

	public required ISet<Certification>? AwardedCertifications { get; set; }
}
