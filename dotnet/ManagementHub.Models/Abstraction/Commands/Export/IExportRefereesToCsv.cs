using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;

public interface IExportRefereesToCsv
{
	Stream ExportRefereesAsync(NgbConstraint ngbs, CancellationToken cancellationToken);
}
