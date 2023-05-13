using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Processing.Domain.Tests.Extensions;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
public class HasRequiredCertificationEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public HasRequiredCertificationEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<RefereeEligibilityResult> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId, cancellationToken);

		if (test.RecertificationFor != null)
		{
			var previousVersion = test.RecertificationFor.Version;
			var awardedVersion = test.AwardedCertifications.First().Version;
			var previousCertification = referee.AcquiredCertifications
				.Where(c => c.Version == previousVersion)
				.Select(c => c.Level)
				.ToArray();
			var highestPreviousLevel = previousCertification.Length > 0
				? (CertificationLevel?)previousCertification.Max()
				: null;

			if (referee.AcquiredCertifications.Where(c => c.Version == awardedVersion).Any())
			{
				// referee has another certification for the version of rulebook this provides
				return RefereeEligibilityResult.RecertificationNotAllowedDueToInitialCertificationStarted;
			}
			else if (test.RecertificationFor.Level.Compare(highestPreviousLevel) < 0)
			{
				// the recertification level is lower than the highest previously acquired level
				return RefereeEligibilityResult.RecertificationForLowerThanPreviouslyHeld;

			}
			else if (!referee.AcquiredCertifications.Contains(test.RecertificationFor))
			{
				// referee is missing a certification this test recertifies
				return RefereeEligibilityResult.MissingRequiredCertification;
			}
			else
			{
				return RefereeEligibilityResult.Eligible;
			}
		}

		Certification lowestAwardedCert = test.AwardedCertifications.Min()!;

		Certification? requiredCert = GetRequiredCertificationToAttempt(lowestAwardedCert);

		if (requiredCert != null)
		{
			if (referee.AcquiredCertifications.Contains(requiredCert))
				return RefereeEligibilityResult.Eligible;
			else
				return RefereeEligibilityResult.MissingRequiredCertification;
		}

		return RefereeEligibilityResult.Eligible;
	}

	private static Certification? GetRequiredCertificationToAttempt(Certification lowestAwardedCert)
	{
		switch (lowestAwardedCert.Level)
		{
			case CertificationLevel.Scorekeeper: return null;
			case CertificationLevel.Assistant: return null;
			case CertificationLevel.Flag: return new Certification(CertificationLevel.Assistant, lowestAwardedCert.Version);
			case CertificationLevel.Head: return new Certification(CertificationLevel.Flag, lowestAwardedCert.Version);
			default: throw new InvalidOperationException("Could not determine required certification.");
		}
	}
}
