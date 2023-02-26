using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts;

/// <summary>
/// Implementation of <see cref="IContextProvider"/> that loads data from the database using factory classes.
/// </summary>
public class DbContextProvider : IContextProvider
{
	private readonly DbUserAuthenticationContextFactory userAuthenticationContextFactory;
	private readonly DbUserContextFactory userContextFactory;

	public DbContextProvider(
		ManagementHubDbContext dbContext,
		ILoggerFactory loggerFactory)
	{
		this.userAuthenticationContextFactory = new DbUserAuthenticationContextFactory(
			dbContext.Users,
			loggerFactory.CreateLogger<DbUserAuthenticationContextFactory>());

		this.userContextFactory = new DbUserContextFactory(
			dbContext.Users,
			dbContext.Roles,
			dbContext.NationalGoverningBodyAdmins,
			dbContext.RefereeTeams,
			dbContext.RefereeLocations,
			loggerFactory.CreateLogger<DbUserContextFactory>());
	}
	public async Task<IUserContext> GetUserContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userContextFactory.LoadAsync(userId, cancellationToken);
	}

	public async Task<IUserAuthenticationContext?> TryGetUserAuthenticationContextAsync(Email userEmail, CancellationToken cancellationToken)
	{
		return await this.userAuthenticationContextFactory.TryLoadAsync(userEmail, cancellationToken);
	}
}
