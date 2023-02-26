using System;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Data.Extensions
{
	public static class UserExtensions
	{
		public static UserIdentifier GetIdentifier(this User user)
		{
			if (user.UniqueId is not null)
			{
				if (UserIdentifier.TryParse(user.UniqueId, out var userId))
				{
					return userId;
				}

				throw new Exception("User Id is in an incorrect format!");
			}

			return UserIdentifier.FromLegacyUserId(user.Id);
		}
	}
}
