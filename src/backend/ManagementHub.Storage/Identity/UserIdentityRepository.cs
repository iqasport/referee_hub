using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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
		var dbUser = new User
		{
			UniqueId = user.UserId.ToString(),
			Email = user.UserEmail.Value,
			CreatedAt = DateTime.UtcNow,
			EncryptedPassword = user.UserPassword?.PasswordHash ?? throw new ArgumentException(nameof(user.UserPassword)),
		};
		this.dbContext.Users.Add(dbUser);
		this.dbContext.Roles.Add(new Role
		{
			AccessType = Models.Enums.UserAccessType.Referee,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			User = dbUser,
		});

		await this.dbContext.SaveChangesAsync(cancellationToken);
	}

	public IQueryable<User> QueryUsers() => this.dbContext.Users.AsNoTracking();

	public async Task<string?> GetEmailConfirmationToken(UserIdentity user)
	{
		var dbUser = await this.QueryUsers()
			.Where(u => u.Email == user.UserEmail.Value)
			.Select(u => new { u.Email, u.ConfirmationToken })
			.SingleAsync();
		return dbUser.ConfirmationToken;
	}

	public Task SetEmailConfirmationToken(UserIdentity user, string token, CancellationToken cancellationToken) =>
		this.UpdateUser(u => u.SetProperty(dbUser => dbUser.ConfirmationToken, token), user, cancellationToken);

	public Task UpdateEmailAsync(UserIdentity user, Email newEmail, CancellationToken cancellationToken) =>
		this.UpdateUser(u => u.SetProperty(dbUser => dbUser.Email, newEmail.Value), user, cancellationToken);

	public Task UpdatePasswordAsync(UserIdentity user, UserPassword password, CancellationToken cancellationToken) =>
		this.UpdateUser(u => u.SetProperty(dbUser => dbUser.EncryptedPassword, password.PasswordHash), user, cancellationToken);

	public Task SetEmailConfirmedAsync(UserIdentity user, CancellationToken cancellationToken) =>
		this.UpdateUser(u => u.SetProperty(dbUser => dbUser.ConfirmedAt, DateTime.UtcNow), user, cancellationToken);

	public Task SetLastLoginTime(UserIdentity user, CancellationToken cancellationToken) =>
		this.UpdateUser(u => u.SetProperty(dbUser => dbUser.LastSignInAt, DateTime.UtcNow), user, cancellationToken);

	private async Task UpdateUser(Expression<Func<SetPropertyCalls<User>, SetPropertyCalls<User>>> updater, UserIdentity user, CancellationToken cancellationToken)
	{
		await this.dbContext.Users.Where(u => u.Email == user.UserEmail.Value)
			.ExecuteUpdateAsync([updater, u => u.SetProperty(x => x.UpdatedAt, DateTime.UtcNow)], cancellationToken);
	}
}
