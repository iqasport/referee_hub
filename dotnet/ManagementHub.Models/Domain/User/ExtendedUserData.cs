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

	public string Bio { get; set; } = string.Empty;

	public string Pronouns { get; set; } = string.Empty;

	public bool ShowPronouns { get; set; } = false;
}
