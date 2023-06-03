using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Storage.Extensions;
public static class UserCollectionExtensions
{
	public static IQueryable<User> WithIdentifier(this IQueryable<User> users, UserIdentifier userId) =>
		users.Where(user => user.UniqueId == userId.ToString() || (user.UniqueId == null && user.Id == userId.ToLegacyUserId()));
}
