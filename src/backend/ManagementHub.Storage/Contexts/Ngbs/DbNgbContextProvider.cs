using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Ngbs;
public class DbNgbContextProvider : INgbContextProvider
{
	private readonly DbNgbContextFactory ngbContextFactory;
	private readonly DbNgbStatsContextFactory ngbStatsContextFactory;

	public DbNgbContextProvider(ManagementHubDbContext dbContext, ILoggerFactory loggerFactory)
	{
		this.ngbContextFactory = new(dbContext, loggerFactory.CreateLogger<DbNgbContextFactory>());
		this.ngbStatsContextFactory = new(dbContext);
	}

	public async Task<INgbContext> GetNgbContextAsync(NgbIdentifier ngb)
	{
		return await this.ngbContextFactory.GetSingleNgb(ngb);
	}

	public async Task<INgbStatsContext> GetNgbStatsAsync(NgbIdentifier ngbIdentifier)
	{
		return await this.ngbStatsContextFactory.GetNgbStatsContextAsync(ngbIdentifier);
	}

	public IQueryable<INgbContext> QueryNgbs()
	{
		return this.ngbContextFactory.QueryNgbs();
	}
}
