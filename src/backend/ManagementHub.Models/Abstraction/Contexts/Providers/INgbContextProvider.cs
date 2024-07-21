using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface INgbContextProvider
{
	IQueryable<INgbContext> QueryNgbs();

	Task<INgbStatsContext> GetCurrentNgbStatsAsync(NgbIdentifier ngbIdentifier);
	Task<INgbContext> GetNgbContextAsync(NgbIdentifier ngb);
	IAsyncEnumerable<INgbContext> GetNgbContextsAsync(NgbConstraint ngb);

	Task<Uri?> GetNgbAvatarUriAsync(NgbIdentifier ngb);
	Task<IOrderedEnumerable<INgbStatsContext>> GetHistoricalNgbStatsAsync(NgbIdentifier ngb);
	Task UpdateNgbInfoAsync(NgbIdentifier ngb, NgbData ngbData);
	Task CreateNgbAsync(NgbIdentifier ngb, NgbData ngbData);
}
