using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class NumberOfAttemptsEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public NumberOfAttemptsEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId);

		// checks if referee has attempted the test less than maximum times
		if (referee.TestAttempts.Count(at => at.TestId == test.TestId) < test.MaximumAttempts)
		{
			return true;
		}

		return false;
	}
}
