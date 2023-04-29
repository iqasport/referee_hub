using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;

public interface IRefereeEligibilityPolicy
{
	Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId, CancellationToken cancellationToken);
}
