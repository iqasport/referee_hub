using System;
using ManagementHub.Models.Abstraction.Services;
using Microsoft.AspNetCore.DataProtection;

namespace ManagementHub.Storage.Services;

public class UserSensitiveInfoProtector : IUserSensitiveInfoProtector
{
	private const string ProtectedPrefix = "dp:";
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

		return ProtectedPrefix + this.dataProtector.Protect(value);
	}

	public string? Unprotect(string? value)
	{
		if (value == null)
		{
			return null;
		}

		if (!value.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
		{
			return value;
		}

		return this.dataProtector.Unprotect(value[ProtectedPrefix.Length..]);
	}
}