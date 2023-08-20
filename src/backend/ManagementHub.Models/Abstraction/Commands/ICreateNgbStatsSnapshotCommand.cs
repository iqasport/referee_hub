using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Commands;
public interface ICreateNgbStatsSnapshotCommand
{
	Task CreateNgbStatsSnapshot(NgbConstraint ngbs, CancellationToken cancellationToken);
}
