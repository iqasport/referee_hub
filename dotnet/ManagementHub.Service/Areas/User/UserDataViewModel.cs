using ManagementHub.Models.Abstraction.Contexts;

namespace ManagementHub.Service.Areas.User;

public class UserDataViewModel
{
	/// <summary>
	/// Constructor for serialization.
	/// </summary>
	public UserDataViewModel() { }

	/// <summary>
	/// Creates a new instance of view model based on the <paramref name="userDataContext"/>.
	/// </summary>
	/// <param name="userDataContext">Context of the user data.</param>
	/// <param name="isCurrentUser">If true full data is returned, else pronouns are returned only if permitted.</param>
	public UserDataViewModel(IUserDataContext userDataContext, bool isCurrentUser)
	{
		this.FirstName = userDataContext.ExtendedUserData.FirstName;
		this.LastName = userDataContext.ExtendedUserData.LastName;
		this.Bio = userDataContext.ExtendedUserData.Bio;
		this.ShowPronouns = userDataContext.ExtendedUserData.ShowPronouns;
		if (isCurrentUser || this.ShowPronouns == true)
		{
			this.Pronouns = userDataContext.ExtendedUserData.Pronouns;
		}
		this.ExportName = userDataContext.ExtendedUserData.ExportName;
	}

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? Pronouns { get; set; }
	public bool? ShowPronouns { get; set; }
	public bool? ExportName { get; set; }
}
