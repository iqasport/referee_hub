using ManagementHub.Models.Domain.General;

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

    public string Bio { get; set; } = string.Empty;

    public string Pronouns { get; set; } = string.Empty;

    public bool ShowPronouns { get; set; } = false;
}