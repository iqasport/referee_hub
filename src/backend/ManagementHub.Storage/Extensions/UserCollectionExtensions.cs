using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;

namespace ManagementHub.Storage.Extensions;

public static class UserCollectionExtensions
{
	public static IQueryable<User> WithIdentifier(this IQueryable<User> users, UserIdentifier userId) =>
		users.Where(user => user.UniqueId == userId.ToString() || (user.UniqueId == null && user.Id == userId.ToLegacyUserId()));

	public static IQueryable<User> WithEmail(this IQueryable<User> users, string email) =>
		users.Where(user => user.Email == email);
	public static IQueryable<User> WithEmail(this IQueryable<User> users, Email email) => users.WithEmail(email.Value);

	// A user is considered active if:
	// a) they have made an attempt at the most recent certification
	// b) they have signed in within the last year (e.g. to provide their profile for a tournament)
	public static IQueryable<User> ActiveUsers(this ManagementHubDbContext dbContext, ISystemClock clock, CertificationVersion currentVersion)
	{
		var now = clock.UtcNow.UtcDateTime;
		var lastYear = now.AddYears(-1);
		var users = dbContext.Users.Include(u => u.TestAttempts).ThenInclude(ta => ta.Test).ThenInclude(t => t!.Certification);
		return users.Where(user => user.LastSignInAt >= lastYear).Union(users
			.Where(u => u.TestAttempts.Any(ta => ta.Test!.Certification!.Version == currentVersion)));
	}
}
