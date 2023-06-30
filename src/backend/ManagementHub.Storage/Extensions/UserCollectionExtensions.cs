using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Storage.Extensions;
public static class UserCollectionExtensions
{
	public static IQueryable<User> WithIdentifier(this IQueryable<User> users, UserIdentifier userId) =>
		users.Where(user => user.UniqueId == userId.ToString() || (user.UniqueId == null && user.Id == userId.ToLegacyUserId()));

	public static IQueryable<User> WithEmail(this IQueryable<User> users, string email) =>
		users.Where(user => user.Email == email);
	public static IQueryable<User> WithEmail(this IQueryable<User> users, Email email) => users.WithEmail(email.Value);
}
