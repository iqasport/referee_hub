using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Identity;

/// <summary>
/// An implementation of the ASP.NET Core Identity data storage layer.
/// 
/// This is a bridge between the the identity concepts and the database.
/// The management hub database schema followed ruby's Devise and doesn't match the ASP.NET Core Identity assumptions 1-1, but it's ok.
/// This file is split into a few parts to simplify reading it.
/// It's breaking away from the some of the assumptions of the core library in order to minimize the need to load some of the data when not used.
/// </summary>
/// <remarks><seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers"/>></remarks>
public partial class UserStore : IUserStore<UserIdentity>, IUserPasswordStore<UserIdentity>, IUserEmailStore<UserIdentity>
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

		await this.userRepository.CreateUserAsync(user, cancellationToken);

		return IdentityResult.Success;
	}

	public Task<IdentityResult> DeleteAsync(UserIdentity user, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
    }

    public Task<IdentityResult> UpdateAsync(UserIdentity user, CancellationToken cancellationToken)
    {
		// This method throws an exception because it's a generic Update
		// Potentially anything could be changed and I'd have to write some logic to detect this.
		// Instead it's simpler to just enforce setting values in the special set methods.
        throw new NotSupportedException($"{nameof(UpdateAsync)} is not supported on this {nameof(UserStore)}. Use a specific Set* method below instead.");
    }

    public void Dispose()
	{
		// not used
	}

	public async Task<UserIdentity?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Looking up user by email address.");
		var userEmail = new Email(normalizedEmail);
		var dbUser = await SelectUserIdentityDataFromQueryable(this.userRepository.QueryUsers().Where(user => user.Email == userEmail.Value))
			.SingleOrDefaultAsync(cancellationToken);

		if (dbUser is null)
		{
			this.logger.LogInformation(0, "User for the specified email address was not found.");
			return null;
		}

		var user = ConvertDbUserViewToUserIdentity(dbUser);

		this.logger.LogInformation(0, "Loaded user ({userId}).", user.UserId);

		return user;
	}

	public async Task<UserIdentity?> FindByIdAsync(string userIdentifier, CancellationToken cancellationToken)
	{
		if (!UserIdentifier.TryParse(userIdentifier, out var userId))
		{
			throw new ArgumentException("User Id is in an incorrect format! Please clear your cookies.", nameof(userIdentifier));
		}

		this.logger.LogInformation(0, "Looking up user by id ({userId}).", userId);
		var dbUser = await SelectUserIdentityDataFromQueryable(this.userRepository.QueryUsers().WithIdentifier(userId))
			.SingleOrDefaultAsync(cancellationToken);

		if (dbUser is null)
		{
			this.logger.LogInformation(0, "User for the specified user id ({userId}) was not found.", userId);
			return null;
		}

		var user = ConvertDbUserViewToUserIdentity(dbUser);

		this.logger.LogInformation(0, "Loaded user ({userId}).", user.UserId);

		return user;
	}

	public Task<bool> GetEmailConfirmedAsync(UserIdentity user, CancellationToken cancellationToken) =>
		Task.FromResult(user.IsEmailConfirmed);

	public Task<string?> GetNormalizedEmailAsync(UserIdentity user, CancellationToken cancellationToken) =>
		Task.FromResult<string?>(user.UserEmail.Value);

	public Task<string?> GetPasswordHashAsync(UserIdentity user, CancellationToken cancellationToken) =>
		Task.FromResult(user.UserPassword?.PasswordHash);

	public Task<string> GetUserIdAsync(UserIdentity user, CancellationToken cancellationToken) =>
		Task.FromResult(user.UserId.ToString());

	/// <summary>
	/// Returns true is the user has a password - i.e. if they are registered directly with the service and not through an external provider.
	/// </summary>
	public Task<bool> HasPasswordAsync(UserIdentity user, CancellationToken cancellationToken) =>
		Task.FromResult(user.UserPassword != null);

	public async Task SetEmailConfirmedAsync(UserIdentity user, bool confirmed, CancellationToken cancellationToken)
	{
		if (!confirmed)
		{
			throw new NotSupportedException($"{nameof(SetEmailConfirmedAsync)} should not be called with a false value.");
		}

		this.logger.LogInformation(0, "Setting email as confirmed.");
		await this.userRepository.SetEmailConfirmedAsync(user, cancellationToken);
	}

	public async Task SetNormalizedEmailAsync(UserIdentity user, string? normalizedEmail, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Updating user email.");
		var newEmail = new Email(normalizedEmail ?? string.Empty);
		await this.userRepository.UpdateEmailAsync(user, newEmail, cancellationToken);
	}

	public Task SetPasswordHashAsync(UserIdentity user, string? passwordHash, CancellationToken cancellationToken)
	{
		if (passwordHash is null)
		{
			throw new ArgumentNullException(nameof(passwordHash));
		}

		if (user.UserPassword is null)
		{
			// this only happens when we call this method the first time before creating the new user
			user.UserPassword = new UserPassword(passwordHash);
			return Task.CompletedTask;
		}

		this.logger.LogInformation(0, "Updating user password.");

		return this.userRepository.UpdatePasswordAsync(user, new UserPassword(passwordHash), cancellationToken);
	}

	private static UserIdentity ConvertDbUserViewToUserIdentity(DbUserView dbUser) =>
		new(GetIdentifier(dbUser.UniqueId, dbUser.Id), new Email(dbUser.Email))
		{
			IsEmailConfirmed = dbUser.IsEmailConfirmed,
			UserPassword = new UserPassword(dbUser.PasswordHash)
		};

	private static IQueryable<DbUserView> SelectUserIdentityDataFromQueryable(IQueryable<User> users) =>
		users.Select(u => new DbUserView
		{
			Id = u.Id,
			UniqueId = u.UniqueId,
			Email = u.Email,
			PasswordHash = u.EncryptedPassword,
			IsEmailConfirmed = u.ConfirmedAt != null,
		});

	private static UserIdentifier GetIdentifier(string? uniqueId, long id)
	{
		if (uniqueId is not null)
		{
			if (UserIdentifier.TryParse(uniqueId, out var userId))
			{
				return userId;
			}

			throw new Exception("User Id is in an incorrect format!");
		}

        return UserIdentifier.FromLegacyUserId(id);
    }

	private class DbUserView
	{
		public long Id { get; set; }
		public string? UniqueId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public bool IsEmailConfirmed { get; set; }
	}
}
