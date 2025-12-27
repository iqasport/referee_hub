using System;
using Microsoft.AspNetCore.Identity;

namespace ManagementHub.Storage.Identity;

public class BCryptPasswordHasher<TUser> : IPasswordHasher<TUser>
	where TUser : class
{
	private const int HashingWorkFactor = 12; // 4,096 iterations

	public string HashPassword(TUser user, string password)
	{
		return BCrypt.Net.BCrypt.HashPassword(password, HashingWorkFactor);
	}

	public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
	{
		if (!BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
		{
			return PasswordVerificationResult.Failed;
		}

		if (int.TryParse(hashedPassword.AsSpan()[4..6], out var hashedWorkFactor))
		{
			if (hashedWorkFactor < HashingWorkFactor)
			{
				return PasswordVerificationResult.SuccessRehashNeeded;
			}
		}

		return PasswordVerificationResult.Success;
	}
}
