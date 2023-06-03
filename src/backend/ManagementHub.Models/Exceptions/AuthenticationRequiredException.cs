using System;

namespace ManagementHub.Models.Exceptions;
public class AuthenticationRequiredException : Exception
{
	public AuthenticationRequiredException(string? message) : base(message)
	{
	}
}
