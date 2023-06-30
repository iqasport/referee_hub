using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
public class PaymentEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public PaymentEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<RefereeEligibilityResult> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId, cancellationToken);

		var hrCertifications = test.AwardedCertifications.Where(c => c.Level == CertificationLevel.Head);
		foreach (var certification in hrCertifications)
		{
			if (!referee.HeadCertificationsPaid.Contains(certification.Version))
			{
				return RefereeEligibilityResult.MissingCertificationPayment;
			}
		}

		return RefereeEligibilityResult.Eligible;
	}
}
