using System;

namespace ManagementHub.Models.Exceptions;

/// <summary>
/// An exception to be thrown when an object hasn't been found.
/// </summary>
public class NotFoundException : Exception
{
	public NotFoundException() : base("The specified object has not been found.")
	{ }

	public NotFoundException(string identifier) : base($"The specified object ('{identifier}') has not been found.")
	{ }
}
