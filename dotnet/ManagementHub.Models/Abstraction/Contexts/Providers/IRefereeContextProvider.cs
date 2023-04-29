using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;
public interface IRefereeContextProvider
{
	/// <summary>
	/// Retrieves a referee view context for a specified <paramref name="userId"/> provided the <paramref name="ngbConstraint"/> is satisfied by the refere.
	/// </summary>
	Task<IRefereeViewContext> GetRefereeViewContextAsync(UserIdentifier userId, NgbConstraint ngbConstraint, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieves a queryable collection of referee view contexts that satisfy the <paramref name="ngbConstraint"/>.
	/// </summary>
	IQueryable<IRefereeViewContext> GetRefereeViewContextQueryable(NgbConstraint ngbConstraint);

	Task<IRefereeTestContext> GetRefereeTestContextAsync(UserIdentifier userId, CancellationToken cancellationToken);
}
