using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class HasRequiredCertificationEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public HasRequiredCertificationEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId);

		if (test.RecertificationFor != null)
		{
			return referee.AcquiredCertifications.Contains(test.RecertificationFor);
		}

		Certification lowestAwardedCert = GetLowestAwardedCertification(test.AwardedCertifications);

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
			case CertificationLevel.Snitch: return new Certification(CertificationLevel.Assistant, lowestAwardedCert.Version);
			case CertificationLevel.Head: return new Certification(CertificationLevel.Snitch, lowestAwardedCert.Version);
			default: throw new InvalidOperationException("Could not determine required certification.");
		}
	}

	private static Certification GetLowestAwardedCertification(IEnumerable<Certification> awardedCertifications)
	{
		var scorekeeper = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Scorekeeper);
		if (scorekeeper != default)
		{
			return scorekeeper;
		}

		var assistant = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Assistant);
		if (assistant != default)
		{
			return assistant;
		}

		var flag = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Snitch);
		if (flag != default)
		{
			return flag;
		}

		var head = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Head);
		if (head != default)
		{
			return head;
		}

		throw new InvalidOperationException("Could not find the lowest awarded certification.");
	}
}
