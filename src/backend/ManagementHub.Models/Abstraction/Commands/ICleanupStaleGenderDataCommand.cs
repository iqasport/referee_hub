using System.Threading;
using System.Threading.Tasks;

namespace ManagementHub.Models.Abstraction.Commands;

public interface ICleanupStaleGenderDataCommand
{
	Task CleanupStaleGenderDataAsync(CancellationToken cancellationToken);
}
