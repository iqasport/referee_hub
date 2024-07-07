using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
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
	Task CreateUserAsync(UserIdentity user, CancellationToken cancellationToken);

	/// <summary>
	/// Returns a queryable of the users from underlying storage.
	/// </summary>
	/// <returns></returns>
	IQueryable<User> QueryUsers();

	/// <summary>
	/// Updates the database with the email confirmation token.
	/// </summary>
	Task SetEmailConfirmationToken(UserIdentity user, string token, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieves the email confirmation token from the database.
	/// </summary>
	Task<string?> GetEmailConfirmationToken(UserIdentity user);

	/// <summary>
	/// Updates email of the user in the db.
	/// </summary>
	Task UpdateEmailAsync(UserIdentity user, Email email, CancellationToken cancellationToken);

	/// <summary>
	/// Updates password of the user in the db.
	/// </summary>
	Task UpdatePasswordAsync(UserIdentity user, UserPassword password, CancellationToken cancellationToken);

	/// <summary>
	/// Confirms user had a valid email address at one point.
	/// </summary>
	Task SetEmailConfirmedAsync(UserIdentity user, CancellationToken cancellationToken);

	/// <summary>
	/// Sets the last login time of the user to the current time.
	/// </summary>
	Task SetLastLoginTime(UserIdentity user, CancellationToken cancellationToken);
}

