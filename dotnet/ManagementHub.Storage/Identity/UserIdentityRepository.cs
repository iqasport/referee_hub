using ManagementHub.Models.Data;
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

	public async Task CreateUserAsync(UserIdentity user)
	{
		this.dbContext.Users.Add(new User
		{
			UniqueId = user.UserId.ToString(),
			Email = user.UserEmail.Value,
			CreatedAt = DateTime.UtcNow,
			EncryptedPassword = user.UserPassword?.PasswordHash ?? throw new ArgumentException(nameof(user.UserPassword)),
		});

		await this.dbContext.SaveChangesAsync();
	}

	public IQueryable<User> QueryUsers() => this.dbContext.Users.AsNoTracking();

	public async Task SetEmailConfirmationToken(UserIdentity user, string token)
	{
		var dbUser = await this.dbContext.Users.SingleAsync(u => u.Email == user.UserEmail.Value);
		dbUser.ConfirmationToken = token;
		await this.dbContext.SaveChangesAsync();
	}

	public async Task<string?> GetEmailConfirmationToken(UserIdentity user)
	{
        var dbUser = await this.QueryUsers().SingleAsync(u => u.Email == user.UserEmail.Value);
		return dbUser.ConfirmationToken;
    }
}
