using System;
using ManagementHub.Models.Domain.General;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Extended user data that include bio and pronouns.
/// </summary>
public class ExtendedUserData : UserData
{
	public ExtendedUserData(Email email, string firstName, string lastName) : base(email, firstName, lastName)
	{
	}

	/// <summary>
	/// Short bio of the user.
	/// </summary>
	public string Bio { get; set; } = string.Empty;

	/// <summary>
	/// Pronouns of the user.
	/// </summary>
	public string Pronouns { get; set; } = string.Empty;

	/// <summary>
	/// Whether to show pronouns on the user profile page.
	/// </summary>
	public bool ShowPronouns { get; set; } = false;

	/// <summary>
	/// Whether to show user name in exported data.
	/// </summary>
	public bool ExportName { get; set; } = true;

	/// <summary>
	/// Date when the user signed up.
	/// </summary>
	public DateOnly CreatedAt { get; set; }
}
