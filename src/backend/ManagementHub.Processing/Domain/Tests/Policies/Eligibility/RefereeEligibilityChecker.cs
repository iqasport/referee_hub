using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;

public class RefereeEligibilityChecker
{
	private readonly IEnumerable<IRefereeEligibilityPolicy> refereeEligibilityPolicies;
	private readonly ILogger<RefereeEligibilityChecker> logger;

	public RefereeEligibilityChecker(IEnumerable<IRefereeEligibilityPolicy> refereeEligibilityPolicies, ILogger<RefereeEligibilityChecker> logger)
	{
		this.refereeEligibilityPolicies = refereeEligibilityPolicies;
		this.logger = logger;
	}

	public async Task<RefereeEligibilityResult> CheckRefereeEligibilityAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken)
	{
		foreach (var policy in this.refereeEligibilityPolicies)
		{
			try
			{
				var result = await policy.IsUserEligibleForTestAsync(test, userId, cancellationToken);

				if (result == default)
				{
					this.logger.LogError(0, "Policy {policy} dud not return a valid result. Continuing checking further policies.", result);
				}
				else if (result != RefereeEligibilityResult.Eligible)
				{
					this.logger.LogWarning(0, "Referee ({userId}) not eligible for test ({testId}). Failed policy: {policy} with result: {result}", userId, test.TestId, policy.GetType().Name, result);
					return result;
				}
			}
			catch (Exception ex)
			{
				this.logger.LogError(0, ex, "Exception occured while checking eligibility for referee ({userId}) while checking policy {policy}", userId, policy.GetType().Name);
				throw;
			}
		}

		return RefereeEligibilityResult.Eligible;
	}
}
