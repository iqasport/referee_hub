using System;
using System.Security.Cryptography;
using ManagementHub.Models.Abstraction.Services;
using Microsoft.AspNetCore.DataProtection;

namespace ManagementHub.Storage.Services;

internal static class UserSensitiveInfoProtection
{
	public const string ProtectedPrefix = "dp:";
}

public class UserSensitiveInfoProtector : IUserSensitiveInfoProtector
{
	private readonly IDataProtector dataProtector;

	public UserSensitiveInfoProtector(IDataProtectionProvider dataProtectionProvider)
	{
		this.dataProtector = dataProtectionProvider.CreateProtector("ManagementHub.UserSensitiveInfo");
	}

	public string? Protect(string? value)
	{
		if (value == null)
		{
			return null;
		}

		return UserSensitiveInfoProtection.ProtectedPrefix + this.dataProtector.Protect(value);
	}

	public string? Unprotect(string? value)
	{
		if (value == null)
		{
			return null;
		}

		if (!value.StartsWith(UserSensitiveInfoProtection.ProtectedPrefix, StringComparison.Ordinal))
		{
			return value;
		}

		try
		{
			return this.dataProtector.Unprotect(value[UserSensitiveInfoProtection.ProtectedPrefix.Length..]);
		}
		catch (CryptographicException)
		{
			return value;
		}
		catch (FormatException)
		{
			return value;
		}
	}
}
