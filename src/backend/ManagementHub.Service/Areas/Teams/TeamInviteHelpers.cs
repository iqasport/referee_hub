namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Shared helpers for team invite-related controllers.
/// </summary>
internal static class TeamInviteHelpers
{
	/// <summary>
	/// Builds a display name from first and last name, returning null if both are empty.
	/// </summary>
	public static string? BuildDisplayName(string? firstName, string? lastName)
	{
		var displayName = string.Join(" ", new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
		return string.IsNullOrWhiteSpace(displayName) ? null : displayName;
	}

	/// <summary>
	/// Normalizes an email address for case-insensitive in-memory comparison.
	/// Note: Use <c>string.ToLower()</c> directly in EF Core query expressions,
	/// as <c>ToLowerInvariant()</c> is not translated to SQL by EF Core.
	/// </summary>
	public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
