using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
public class NumberOfAttemptsEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public NumberOfAttemptsEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<RefereeEligibilityResult> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId, cancellationToken);

		// checks if referee has attempted the test less than maximum times
		if (referee.TestAttempts.Count(at => at.TestId == test.TestId) < test.MaximumAttempts)
		{
			return RefereeEligibilityResult.Eligible;
		}

		return RefereeEligibilityResult.TestAttemptedMaximumNumberOfTimes;
	}
}
