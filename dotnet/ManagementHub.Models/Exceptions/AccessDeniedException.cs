using System;

namespace ManagementHub.Models.Exceptions;

/// <summary>
/// An exception to be thrown when user is deemed to not have access to a resource.
/// </summary>
public class AccessDeniedException : Exception
{
	public AccessDeniedException() : base("User doesn't have access to the specified resource.")
	{ }

	public AccessDeniedException(string identifier) : base($"User doesn't have access to the specified resource ('{identifier}').")
	{ }

	public AccessDeniedException(string message, bool customMessage) : base(message)
	{ }
}
