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
	/// Retrieves a list of referee view contexts that satisfy the <paramref name="ngbConstraint"/>.
	/// </summary>
	Task<IQueryable<IRefereeViewContext>> GetRefereeViewContextListAsync(NgbConstraint ngbConstraint, CancellationToken cancellationToken);
}
