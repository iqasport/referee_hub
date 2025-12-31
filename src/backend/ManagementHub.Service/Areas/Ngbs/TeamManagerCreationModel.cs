using System.Text.Json.Serialization;

namespace ManagementHub.Service.Areas.Ngbs;

public class TeamManagerCreationModel
{
	public required string Email { get; set; }
	public bool CreateAccountIfNotExists { get; set; }
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TeamManagerCreationStatus
{
	/// <summary>
	/// The provided email is not a valid value.
	/// </summary>
	InvalidEmail = 0,

	/// <summary>
	/// The user doesn't exist and the request did not ask to create the account.
	/// </summary>
	UserDoesNotExist = 1,

	/// <summary>
	/// Manager role added to an existing user.
	/// </summary>
	ManagerRoleAdded = 2,

	/// <summary>
	/// Manager user has been created.
	/// </summary>
	ManagerUserCreated = 3,
}
