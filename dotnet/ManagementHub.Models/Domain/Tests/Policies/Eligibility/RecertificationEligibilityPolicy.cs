using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class RecertificationEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public RecertificationEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId);

		if (test.RecertificationFor.HasValue)
		{
			return referee.AcquiredCertifications.Contains(test.RecertificationFor.Value);
		}

		return true;
	}
}
