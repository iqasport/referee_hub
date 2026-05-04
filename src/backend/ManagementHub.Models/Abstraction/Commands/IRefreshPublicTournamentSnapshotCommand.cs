using System.Threading;
using System.Threading.Tasks;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IRefreshPublicTournamentSnapshotCommand
{
	Task RefreshPublicTournamentSnapshot(CancellationToken cancellationToken);
}