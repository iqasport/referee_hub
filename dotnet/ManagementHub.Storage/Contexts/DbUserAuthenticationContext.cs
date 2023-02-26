using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts;

public record DbUserAuthenticationContext(UserIdentifier UserId, UserPassword UserPassword) : IUserAuthenticationContext
{
}

public class DbUserAuthenticationContextFactory
{
	private readonly IQueryable<User> users;
	private readonly ILogger<DbUserContextFactory> logger;

	public DbUserAuthenticationContextFactory(
		IQueryable<User> users,
		ILogger<DbUserAuthenticationContextFactory> logger)
	{
		this.users = users;
		this.logger = logger;
	}

	public async Task<DbUserAuthenticationContext?> TryLoadAsync(Email userEmail, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Loading user authentication context.");
		var user = await this.users.Where(user => user.Email == userEmail.Value)
			.Select(user => new User
			{
				Id = user.Id,
				EncryptedPassword = user.EncryptedPassword,
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (user is null)
		{
			this.logger.LogInformation(0, "User for the specified email address was not found.");
			return null;
		}

		var userId = new UserIdentifier(user.Id);
		var userPassword = new UserPassword(user.EncryptedPassword);

		this.logger.LogInformation(0, "Loaded authentication context for user ({userId}).", userId);

		return new DbUserAuthenticationContext(userId, userPassword);
	}
}

