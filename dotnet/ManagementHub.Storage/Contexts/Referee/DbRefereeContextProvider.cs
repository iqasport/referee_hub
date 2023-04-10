using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Referee;
public class DbRefereeContextProvider : IRefereeContextProvider
{
	private readonly DbRefereeViewContextFactory dbRefereeViewContextFactory;

	public DbRefereeContextProvider(
		ManagementHubDbContext dbContext,
		ILoggerFactory loggerFactory)
	{
		this.dbRefereeViewContextFactory = new DbRefereeViewContextFactory(
			dbContext.Users,
			dbContext.Roles,
			dbContext.RefereeCertifications,
			dbContext.RefereeLocations,
			dbContext.RefereeTeams,
			dbContext.NationalGoverningBodies,
			loggerFactory.CreateLogger<DbRefereeViewContextFactory>());
	}

	public async Task<IRefereeViewContext> GetRefereeViewContextAsync(UserIdentifier userId, NgbConstraint ngbConstraint, CancellationToken cancellationToken)
	{
		return await this.dbRefereeViewContextFactory.LoadAsync(userId, ngbConstraint, cancellationToken);
	}

	public IQueryable<IRefereeViewContext> GetRefereeViewContextQueryable(NgbConstraint ngbConstraint)
	{
		return this.dbRefereeViewContextFactory.QueryReferees(ngbConstraint);
	}
}
