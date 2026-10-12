namespace ManagementHub.Models.Abstraction.Services;

public interface IUserSensitiveInfoProtector
{
	string? Protect(string? value);

	string? Unprotect(string? value);
}