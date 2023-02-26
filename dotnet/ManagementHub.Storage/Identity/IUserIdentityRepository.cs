using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Storage.Identity;

/// <summary>
/// Repository of action related to user identity.
/// </summary>
public interface IUserIdentityRepository
{
	/// <summary>
	/// Persists <see cref="user"/> in the storage.
	/// </summary>
	Task CreateUserAsync(UserIdentity user);

	/// <summary>
	/// Returns a queryable of the users from underlying storage.
	/// </summary>
	/// <returns></returns>
	IQueryable<User> QueryUsers();

	/// <summary>
	/// Updates the database with the email confirmation token.
	/// </summary>
	Task SetEmailConfirmationToken(UserIdentity user, string token);

	/// <summary>
	/// Retrieves the email confirmation token from the database.
	/// </summary>
	Task<string?> GetEmailConfirmationToken(UserIdentity user);
}

