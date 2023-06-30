using System.Linq;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Ngbs;
public class DbNgbContextProvider : INgbContextProvider
{
	private readonly DbNgbContextFactory ngbContextFactory;

	public DbNgbContextProvider(ManagementHubDbContext dbContext, ILoggerFactory loggerFactory)
	{
		this.ngbContextFactory = new(dbContext, loggerFactory.CreateLogger<DbNgbContextFactory>());
	}

	public IQueryable<INgbContext> QueryNgbs()
	{
		return this.ngbContextFactory.QueryNgbs();
	}
}
