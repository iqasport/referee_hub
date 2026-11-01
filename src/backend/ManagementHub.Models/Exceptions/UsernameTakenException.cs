using System;

namespace ManagementHub.Models.Exceptions;

/// <summary>
/// An exception to be thrown when a requested username is already taken by another user.
/// </summary>
public class UsernameTakenException : Exception
{
	public UsernameTakenException(string username) : base($"The username '{username}' is already taken.")
	{
		this.Username = username;
	}

	public string Username { get; }
}
