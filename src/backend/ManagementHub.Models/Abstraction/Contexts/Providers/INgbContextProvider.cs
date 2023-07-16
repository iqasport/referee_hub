using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface INgbContextProvider
{
	IQueryable<INgbContext> QueryNgbs();

	Task<INgbStatsContext> GetNgbStatsAsync(NgbIdentifier ngbIdentifier);
	Task<INgbContext> GetNgbContextAsync(NgbIdentifier ngb);
}
