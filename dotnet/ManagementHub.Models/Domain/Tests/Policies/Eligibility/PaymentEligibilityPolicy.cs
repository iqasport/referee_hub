using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class PaymentEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public PaymentEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId);

		var hrCertifications = test.AwardedCertifications.Where(c => c.Level == Enums.CertificationLevel.Head);

		if (hrCertifications.Any())
		{
			foreach (var certification in hrCertifications)
			{
				referee.HeadCertificationsPaid.Contains(certification.Version);
			}
		}

		return true;
	}
}
