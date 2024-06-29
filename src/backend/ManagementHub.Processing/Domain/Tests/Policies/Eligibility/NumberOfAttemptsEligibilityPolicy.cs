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
	private readonly IContextualOptions<TestPolicyOverride> overrides;

	public NumberOfAttemptsEligibilityPolicy(IRefereeContextProvider refereeContextProvider, IContextualOptions<TestPolicyOverride> overrides)
	{
		this.refereeContextProvider = refereeContextProvider;
		this.overrides = overrides;
	}

	public async Task<RefereeEligibilityResult> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId, cancellationToken);
		var overrides = await this.overrides.GetAsync<TestPolicyContext>(new TestPolicyContext { UserId = userId.ToString(), TestId = test.TestId.ToString() }, cancellationToken);

		var maxAttempts = overrides.MaxAttempts ?? test.MaximumAttempts;

		// checks if referee has attempted the test less than maximum times
		if (referee.TestAttempts.Count(at => at.TestId == test.TestId) < maxAttempts)
		{
			return RefereeEligibilityResult.Eligible;
		}

		return RefereeEligibilityResult.TestAttemptedMaximumNumberOfTimes;
	}
}
