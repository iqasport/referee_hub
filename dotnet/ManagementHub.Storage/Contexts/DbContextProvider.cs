using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts;

/// <summary>
/// Implementation of <see cref="IContextProvider"/> that loads data from the database using factory classes.
/// </summary>
public class DbContextProvider : IContextProvider
{
	private readonly DbUserContextFactory userContextFactory;
	private readonly DbUserDataContextFactory userDataContextFactory;

	public DbContextProvider(
		ManagementHubDbContext dbContext,
		ILoggerFactory loggerFactory)
	{
		this.userContextFactory = new DbUserContextFactory(
			dbContext.Users,
			dbContext.Roles,
			dbContext.NationalGoverningBodyAdmins,
			dbContext.RefereeTeams,
			dbContext.RefereeLocations,
			dbContext.Languages,
			loggerFactory.CreateLogger<DbUserContextFactory>());
		this.userDataContextFactory = new DbUserDataContextFactory(
			dbContext.Users,
			dbContext.Languages,
			loggerFactory.CreateLogger<DbUserDataContextFactory>());
	}
	public async Task<IUserContext> GetUserContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userContextFactory.LoadAsync(userId, cancellationToken);
	}

	public async Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userDataContextFactory.LoadAsync(userId, cancellationToken);
	}
}
