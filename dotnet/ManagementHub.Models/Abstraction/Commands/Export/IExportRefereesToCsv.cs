using System.IO;
using System.Threading;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Commands.Export;
public interface IExportRefereesToCsv
{
	Stream ExportRefereesAsync(NgbConstraint ngbs, CancellationToken cancellationToken);
}
