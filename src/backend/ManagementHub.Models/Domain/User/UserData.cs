using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// Personal data of a user.
/// </summary>
public class UserData
{
	public UserData(Email email, string firstName, string lastName)
	{
		this.Email = email;
		this.FirstName = firstName;
		this.LastName = lastName;
	}

	public Email Email { get; }

	public string FirstName { get; }

	public string LastName { get; }

	public LanguageIdentifier UserLang { get; set; } = LanguageIdentifier.Default;
}
