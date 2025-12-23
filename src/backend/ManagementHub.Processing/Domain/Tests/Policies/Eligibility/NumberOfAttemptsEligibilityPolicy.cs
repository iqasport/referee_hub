using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Processing.Domain.Tests.Extensions;

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
		var cert = test.AwardedCertifications.Max();
		if (referee.TestAttempts.Count(at => at.TestId == test.TestId || (at.Level == cert.Level && at.Version == cert.Version && (test.RecertificationFor != null) == at.IsRecertification)) < test.MaximumAttempts)
		{
			return RefereeEligibilityResult.Eligible;
		}

		return RefereeEligibilityResult.TestAttemptedMaximumNumberOfTimes;
	}
}
