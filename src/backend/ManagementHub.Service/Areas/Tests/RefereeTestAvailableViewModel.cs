using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;
using ManagementHub.Processing.Domain.Tests.Extensions;
using ManagementHub.Processing.Domain.Tests.Policies.Eligibility;

namespace ManagementHub.Service.Areas.Tests;

public class RefereeTestAvailableViewModel
{
	public required TestIdentifier TestId { get; set; }
	public required string Title { get; set; }
	public required IEnumerable<Certification> AwardedCertifications { get; set; }
	public required LanguageIdentifier Language { get; set; }
	public required bool IsRefereeEligible { get; set; }
	public RefereeEligibilityResult RefereeEligibilityResult { get; set; }
	public CertificationLevel? Level => this.AwardedCertifications.Max()?.Level;
}
