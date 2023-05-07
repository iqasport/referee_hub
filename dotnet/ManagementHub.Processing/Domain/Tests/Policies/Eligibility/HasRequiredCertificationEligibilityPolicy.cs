using System;
using System.Collections.Generic;
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

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
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
			return
				// referee has a certification this test recertifies
				referee.AcquiredCertifications.Contains(test.RecertificationFor) &&
				// the recertification level is greater or equal to the highest previously acquired level
				test.RecertificationFor.Level.Compare(highestPreviousLevel) >= 0 &&
				// referee does not have any other certifications for the version of rulebook this provides
				!referee.AcquiredCertifications.Where(c => c.Version == awardedVersion).Any();
		}

		Certification lowestAwardedCert = test.AwardedCertifications.Min()!;

		Certification? requiredCert = GetRequiredCertificationToAttempt(lowestAwardedCert);

		if (requiredCert != null)
		{
			return referee.AcquiredCertifications.Contains(requiredCert);
		}

		return true;
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
