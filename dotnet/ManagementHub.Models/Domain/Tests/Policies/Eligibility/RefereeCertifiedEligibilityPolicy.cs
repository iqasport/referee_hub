using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class RefereeCertifiedEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public RefereeCertifiedEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId, cancellationToken);

		// return true if the test can award any certifications the user doesn't have yet.
		if (test.AwardedCertifications.Except(referee.AcquiredCertifications).Any())
		{
			return true;
		}

		return false;
	}
}
