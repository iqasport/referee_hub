using System;

namespace ManagementHub.Models.Domain.User;

public class SignInStatistics
{
	/// <summary>
	/// How many times the user has signed in.
	/// </summary>
	public int SignInCount { get; set; }

	/// <summary>
	/// Timestamp when the user last signed in.
	/// </summary>
	public DateTime LastSignInAt { get; set; }
}
