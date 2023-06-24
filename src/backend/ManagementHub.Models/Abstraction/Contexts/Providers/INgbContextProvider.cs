using System.Linq;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface INgbContextProvider
{
	IQueryable<INgbContext> QueryNgbs();
}
