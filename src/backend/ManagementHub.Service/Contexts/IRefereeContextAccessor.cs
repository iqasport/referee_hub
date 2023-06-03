using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;

public interface IRefereeContextAccessor
{
	Task<IRefereeTestContext> GetRefereeTestContextForCurrentUserAsync();

	Task<IRefereeViewContext> GetRefereeViewContextForCurrentUserAsync();

	Task<IRefereeViewContext> GetRefereeViewContextAsync(UserIdentifier userId);

	Task<IQueryable<IRefereeViewContext>> GetRefereeViewContextListAsync();
}
