using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Language;

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
	/// <param name="isCurrentUser">
	/// When <c>true</c> the full data set is returned, including sensitive personal fields
	/// (DateOfBirth, FoodRestrictions, MedicalInformation, EmergencyContact) that are private to the user.
	/// When <c>false</c> only publicly visible fields are returned (pronouns are included only if
	/// the user has opted in via <c>ShowPronouns</c>).
	/// </param>
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
		this.Language = userDataContext.ExtendedUserData.UserLang;
		this.CreatedAt = userDataContext.ExtendedUserData.CreatedAt;
		if (isCurrentUser)
		{
			this.DateOfBirth = userDataContext.ExtendedUserData.DateOfBirth;
			this.FoodRestrictions = userDataContext.ExtendedUserData.FoodRestrictions;
			this.MedicalInformation = userDataContext.ExtendedUserData.MedicalInformation;
			this.EmergencyContact = userDataContext.ExtendedUserData.EmergencyContact;
		}
	}

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? Pronouns { get; set; }
	public bool? ShowPronouns { get; set; }
	public bool? ExportName { get; set; }
	public LanguageIdentifier? Language { get; set; }
	public DateOnly CreatedAt { get; set; }
	public DateOnly? DateOfBirth { get; set; }
	public string? FoodRestrictions { get; set; }
	public string? MedicalInformation { get; set; }
	public string? EmergencyContact { get; set; }
}
