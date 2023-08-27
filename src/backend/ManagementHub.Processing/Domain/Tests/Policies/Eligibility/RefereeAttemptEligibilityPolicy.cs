using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using Microsoft.Extensions.Internal;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
public class RefereeAttemptEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;
	private readonly ISystemClock systemClock;

	public RefereeAttemptEligibilityPolicy(IRefereeContextProvider refereeContextProvider, ISystemClock systemClock)
	{
		this.refereeContextProvider = refereeContextProvider;
		this.systemClock = systemClock;
	}

	public async Task<RefereeEligibilityResult> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId, cancellationToken);

		foreach (var attempt in referee.TestAttempts.Where(at => at.TestId == test.TestId))
		{
			if (this.IsWithinCooldownPeriod(attempt, test))
			{
				return RefereeEligibilityResult.InCooldownPeriod;
			}
		}

		return RefereeEligibilityResult.Eligible;
	}

	private bool IsWithinCooldownPeriod(TestAttempt attempt, Test test)
	{
		// TODO: put this into configuration?
		var cooldown = test.Cooldown;
		var now = this.systemClock.UtcNow.UtcDateTime;

		if (attempt is FinishedTestAttempt finished)
		{
			var nextAttemptAt = finished.FinishedAt + cooldown;
			return nextAttemptAt > now;
		}
		else
		{
			var nextAttemptAt = attempt.StartedAt + test.TimeLimit + cooldown;
			return nextAttemptAt > now;
		}
	}
}
