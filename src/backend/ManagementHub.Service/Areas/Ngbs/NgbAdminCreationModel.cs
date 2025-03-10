using System.Text.Json.Serialization;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbAdminCreationModel
{
	public required string Email { get; set; }
	public bool CreateAccountIfNotExists { get; set; }
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum NgbAdminCreationStatus
{
	/// <summary>
	/// The provided email is not a valid value.
	/// </summary>
	InvalidEmail = 1,

	/// <summary>
	/// The user doesn't exist and the request did not ask to create the account.
	/// </summary>
	UserDoesNotExist,

	/// <summary>
	/// Admin role added to an existing user.
	/// </summary>
	AdminRoleAdded,

	/// <summary>
	/// Admin user has been created.
	/// </summary>
	AdminUserCreated,
}
