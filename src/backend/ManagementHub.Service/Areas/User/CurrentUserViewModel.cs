using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.User;

public class CurrentUserViewModel
{
	public CurrentUserViewModel(IUserContext userContext)
	{
		this.UserId = userContext.UserId;
		this.FirstName = userContext.UserData.FirstName;
		this.LastName = userContext.UserData.LastName;
		this.LanguageId = userContext.UserData.UserLang;
		this.Roles = userContext.Roles.ToList();
	}

	public UserIdentifier UserId { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public Uri? AvatarUrl { get; set; }
	public LanguageIdentifier? LanguageId { get; set; }
	public IReadOnlyCollection<IUserRole> Roles { get; set; }
}
