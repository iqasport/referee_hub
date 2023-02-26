using ManagementHub.Models.Data;
using ManagementHub.Models.Data.Extensions;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Identity;

/// <summary>
/// An implementation of the ASP.NET Core Identity data storage layer.
/// </summary>
/// <remarks><seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers"/>></remarks>
public class UserStore : IUserStore<UserIdentity>, IUserPasswordStore<UserIdentity>, IUserEmailStore<UserIdentity>
{
	private readonly IUserIdentityRepository userRepository;
	private readonly ILogger<UserStore> logger;

	public UserStore(
		IUserIdentityRepository userRepository,
		ILogger<UserStore> logger)
	{
		this.userRepository = userRepository;
		this.logger = logger;
	}
    #region Persisting methods - only CreateAsync, DeleteAsync, UpdateAsync write to the database
    public async Task<IdentityResult> CreateAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		if (user.UserId == default)
		{
			throw new ArgumentException("Provided user object must have the user id already set.", nameof(user));
		}

		this.logger.LogInformation(0, "Creating new user ({userId}).", user.UserId);

		var existingUserById = await this.FindByIdAsync(user.UserId.ToString(), cancellationToken);

		if (existingUserById is not null)
		{
			this.logger.LogError(0, "User with specified id ({userId}) already exists!", user.UserId);
			return IdentityResult.Failed();
		}

		var existingUserByEmail = await this.FindByEmailAsync(user.UserEmail.Value, cancellationToken);

		if (existingUserByEmail is not null)
		{
			this.logger.LogError(0, "User with specified email already exists!");
			return IdentityResult.Failed(new IdentityError { Description = "User with specified email already exists!" });
		}

		await this.userRepository.CreateUserAsync(user);

		return IdentityResult.Success;
	}

	public Task<IdentityResult> DeleteAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
    }

    public Task<IdentityResult> UpdateAsync(UserIdentity user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    #endregion
    public void Dispose()
	{
	}

	public async Task<UserIdentity?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Looking up user by email address.");
		var userEmail = new Email(normalizedEmail);
		var dbUser = await this.userRepository.QueryUsers().Where(user => user.Email == userEmail.Value)
			.Select(user => new User
			{
				Id = user.Id,
				UniqueId = user.UniqueId,
				EncryptedPassword = user.EncryptedPassword,
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (dbUser is null)
		{
			this.logger.LogInformation(0, "User for the specified email address was not found.");
			return null;
		}

		var userId = dbUser.GetIdentifier();

		this.logger.LogInformation(0, "Loaded user ({userId}).", userId);

		return new UserIdentity(userId, userEmail) { UserPassword = new UserPassword(dbUser.EncryptedPassword) };
	}

	public async Task<UserIdentity?> FindByIdAsync(string userIdentifier, CancellationToken cancellationToken)
	{
		if (!UserIdentifier.TryParse(userIdentifier, out var userId))
		{
			throw new ArgumentException("User Id is in an incorrect format!", nameof(userIdentifier));
		}

		this.logger.LogInformation(0, "Looking up user by id ({userId}).", userId);
		var dbUser = await this.userRepository.QueryUsers().Where(user => user.UniqueId == userId.ToString() || (user.UniqueId == null && user.Id == userId.ToLegacyUserId()))
			.Select(user => new User
			{
				Email = user.Email,
                EncryptedPassword = user.EncryptedPassword,
            })
			.SingleOrDefaultAsync(cancellationToken);

		if (dbUser is null)
		{
			this.logger.LogInformation(0, "User for the specified user id ({userId}) was not found.", userId);
			return null;
		}

		var userEmail = new Email(dbUser.Email);

		this.logger.LogInformation(0, "Loaded user ({userId}).", userId);

		return new UserIdentity(userId, userEmail) { UserPassword = new UserPassword(dbUser.EncryptedPassword) };
	}

	public Task<UserIdentity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
	{
		return this.FindByEmailAsync(normalizedUserName, cancellationToken);
	}

	public Task<string?> GetEmailAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return this.GetNormalizedEmailAsync(user, cancellationToken);
	}

	public Task<bool> GetEmailConfirmedAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return Task.FromResult(false);
	}

	public Task<string?> GetNormalizedEmailAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return Task.FromResult<string?>(user.UserEmail.Value);
	}

	public Task<string?> GetNormalizedUserNameAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return this.GetNormalizedEmailAsync(user, cancellationToken);
	}

	public Task<string?> GetPasswordHashAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return Task.FromResult(user.UserPassword?.PasswordHash);
	}

	public Task<string> GetUserIdAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return Task.FromResult(user.UserId.ToString());
	}

	public Task<string?> GetUserNameAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return this.GetNormalizedEmailAsync(user, cancellationToken);
	}

	/// <summary>
	/// Returns true is the user has a password - i.e. if they are registered directly with the service and not through an external provider.
	/// </summary>
	public Task<bool> HasPasswordAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		return Task.FromResult(user.UserPassword != null);
	}

	public Task SetEmailAsync(UserIdentity user, string? email, CancellationToken cancellationToken)
	{
		return this.SetNormalizedEmailAsync(user, email, cancellationToken);
	}

	public Task SetEmailConfirmedAsync(UserIdentity user, bool confirmed, CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task SetNormalizedEmailAsync(UserIdentity user, string? normalizedEmail, CancellationToken cancellationToken)
	{
		user.UserEmail = new Email(normalizedEmail ?? string.Empty);
		return Task.CompletedTask;
	}

	public Task SetNormalizedUserNameAsync(UserIdentity user, string? normalizedName, CancellationToken cancellationToken)
	{
		return this.SetEmailAsync(user, normalizedName, cancellationToken);
	}

	public Task SetPasswordHashAsync(UserIdentity user, string? passwordHash, CancellationToken cancellationToken)
	{
		if (passwordHash is null)
		{
			throw new ArgumentNullException(nameof(passwordHash));
		}

		user.UserPassword = new UserPassword(passwordHash);
		return Task.CompletedTask;
	}

	public Task SetUserNameAsync(UserIdentity user, string? userName, CancellationToken cancellationToken)
	{
		return this.SetEmailAsync(user, userName, cancellationToken);
	}
}
