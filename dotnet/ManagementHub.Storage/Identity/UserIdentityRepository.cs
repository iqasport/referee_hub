using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Identity;

/// <summary>
/// Thin wrapper over the dbcontext for making queries.
/// </summary>
public class UserIdentityRepository : IUserIdentityRepository
{
	private readonly ManagementHubDbContext dbContext;

	public UserIdentityRepository(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task CreateUserAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		this.dbContext.Users.Add(new User
		{
			UniqueId = user.UserId.ToString(),
			Email = user.UserEmail.Value,
			CreatedAt = DateTime.UtcNow,
			EncryptedPassword = user.UserPassword?.PasswordHash ?? throw new ArgumentException(nameof(user.UserPassword)),
		});

		await this.dbContext.SaveChangesAsync(cancellationToken);
	}

	public IQueryable<User> QueryUsers() => this.dbContext.Users.AsNoTracking();

	public async Task<string?> GetEmailConfirmationToken(UserIdentity user)
	{
        var dbUser = await this.QueryUsers().SingleAsync(u => u.Email == user.UserEmail.Value);
		return dbUser.ConfirmationToken;
    }

	public Task SetEmailConfirmationToken(UserIdentity user, string token, CancellationToken cancellationToken) =>
		this.UpdateUser(dbUser => dbUser.ConfirmationToken = token, user, cancellationToken);

	public Task UpdateEmailAsync(UserIdentity user, Email newEmail, CancellationToken cancellationToken) =>
		this.UpdateUser(dbUser => dbUser.Email = newEmail.Value, user, cancellationToken);

	public Task UpdatePasswordAsync(UserIdentity user, UserPassword password, CancellationToken cancellationToken) =>
		this.UpdateUser(dbUser => dbUser.EncryptedPassword = password.PasswordHash, user, cancellationToken);

	public Task SetEmailConfirmedAsync(UserIdentity user, CancellationToken cancellationToken) =>
		this.UpdateUser(dbUser => dbUser.ConfirmedAt = DateTime.UtcNow, user, cancellationToken);

	private async Task UpdateUser(Action<User> updater, UserIdentity user, CancellationToken cancellationToken)
	{
		var dbUser = await this.GetUser(user, cancellationToken);
		updater(dbUser);
		await this.dbContext.SaveChangesAsync(cancellationToken);
	}

	private Task<User> GetUser(UserIdentity user, CancellationToken cancellationToken)
	{
		return this.dbContext.Users.SingleAsync(u => u.Email == user.UserEmail.Value, cancellationToken);
	}
}
