using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Extensions;

using User = ManagementHub.Models.Data.User;

public static class UsernameQueryExtensions
{
	public static IQueryable<User> WithUsername(this IQueryable<User> users, string username)
	{
		var normalized = UsernameRules.Normalize(username);
		return users.Where(u => u.Username == normalized);
	}

	/// <summary>
	/// Resolves user input that may be either an email or a username into an email address.
	/// Input containing '@' is treated as an email and returned unchanged.
	/// </summary>
	/// <returns>
	/// The email (or null if the input was a username that doesn't exist) and whether the input was treated as a username.
	/// </returns>
	public static async Task<(string? Email, bool IsUsername)> ResolveEmailOrUsernameAsync(
		this IQueryable<User> users,
		string? emailOrUsername,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(emailOrUsername)
			|| UsernameRules.LooksLikeEmail(emailOrUsername)
			|| !UsernameRules.IsValid(UsernameRules.Normalize(emailOrUsername)))
		{
			// not a username - let the caller validate it as an email
			return (emailOrUsername, false);
		}

		var email = await users.AsNoTracking()
			.WithUsername(emailOrUsername)
			.Select(u => u.Email)
			.FirstOrDefaultAsync(cancellationToken);
		return (email, true);
	}

	/// <summary>
	/// Finds the first available username of the form <c>base</c>, <c>base2</c>, <c>base3</c>, ...
	/// </summary>
	/// <param name="users">Users collection.</param>
	/// <param name="baseUsername">Base username (already normalized and valid).</param>
	/// <param name="excludeUserDbId">Database id of a user whose current username should not count as taken.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public static async Task<string> FindAvailableUsernameAsync(
		this IQueryable<User> users,
		string baseUsername,
		long? excludeUserDbId,
		CancellationToken cancellationToken)
	{
		var taken = (await users.AsNoTracking()
			.Where(u => u.Username != null && u.Username.StartsWith(baseUsername))
			.Where(u => excludeUserDbId == null || u.Id != excludeUserDbId)
			.Select(u => u.Username!)
			.ToListAsync(cancellationToken))
			.ToHashSet();

		for (var suffix = 1; ; suffix++)
		{
			var candidate = UsernameRules.WithSuffix(baseUsername, suffix);
			if (!taken.Contains(candidate))
			{
				return candidate;
			}
		}
	}

	/// <summary>
	/// Generates a default username for a user from their names, falling back to <c>user{id}</c>.
	/// </summary>
	public static Task<string> GenerateDefaultUsernameAsync(
		this IQueryable<User> users,
		long userDbId,
		string? firstName,
		string? lastName,
		CancellationToken cancellationToken)
	{
		var baseUsername = UsernameRules.CreateBase(firstName, lastName) ?? UsernameRules.FallbackFor(userDbId);
		return users.FindAvailableUsernameAsync(baseUsername, userDbId, cancellationToken);
	}
}
