using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;

public interface IRefereeEligibilityPolicy
{
	Task<RefereeEligibilityResult> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken);
}
