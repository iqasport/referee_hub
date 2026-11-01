using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.User;

/// <summary>
/// Result of resolving a username to a user.
/// </summary>
public class UserLookupViewModel
{
	public required UserIdentifier UserId { get; set; }
	public required string Username { get; set; }
}

/// <summary>
/// Result of checking whether a username can be claimed.
/// </summary>
public class UsernameAvailabilityViewModel
{
	/// <summary>
	/// The normalized (lowercase) username that was checked.
	/// </summary>
	public required string Username { get; set; }

	/// <summary>
	/// Whether the username matches the format rules.
	/// </summary>
	public required bool IsValid { get; set; }

	/// <summary>
	/// Whether the username is free (or already belongs to the current user).
	/// </summary>
	public required bool IsAvailable { get; set; }
}
