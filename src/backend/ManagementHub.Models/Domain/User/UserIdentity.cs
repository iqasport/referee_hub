using ManagementHub.Models.Domain.General;

namespace ManagementHub.Models.Domain.User;

/// <summary>
/// This class encapsulates the user identity used for authentication purposes.
/// </summary>
public class UserIdentity
{
	public UserIdentity(UserIdentifier userId, Email email)
	{
		this.UserId = userId;
		this.UserEmail = email;
	}

	public UserIdentifier UserId { get; }
	public Email UserEmail { get; }
	public bool IsEmailConfirmed { get; set; }
	public UserPassword? UserPassword { get; set; }
}
