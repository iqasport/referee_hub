using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.User;

/// <summary>
/// Actions related to users.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IUpdateUserDataCommand updateUserDataCommand;
	private readonly IUpdateUserAvatarCommand updateUserAvatarCommand;

	public UsersController(IUserContextAccessor contextAccessor, IUpdateUserDataCommand updateUserDataCommand, IUpdateUserAvatarCommand updateUserAvatarCommand)
	{
		this.contextAccessor = contextAccessor;
		this.updateUserDataCommand = updateUserDataCommand;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
	}

	/// <summary>
	/// Retrieves identity information about the currently signed-in user.
	/// </summary>
	[HttpGet("me")]
	[Tags("User")]
	public async Task<CurrentUserViewModel> GetCurrentUser()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		// TODO: add attribute filtering as per access (me should return root attributes only)
		return new CurrentUserViewModel(userContext);
	}

	/// <summary>
	/// Retrieves personal information about the currently signed-in user.
	/// </summary>
	[HttpGet("me/info")]
	[Tags("UserInfo")]
	public async Task<UserDataViewModel> GetCurrentUserData()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var userDataContext = await this.contextAccessor.GetUserDataContextAsync(userContext.UserId);
		return new UserDataViewModel(userDataContext, isCurrentUser: true);
	}

	/// <summary>
	/// Retrieves personal information about another user.
	/// </summary>
	[HttpGet("{userId}/info")]
	[Tags("UserInfo")]
	public async Task<UserDataViewModel> GetUserData([FromRoute] UserIdentifier userId)
	{
		var userDataContext = await this.contextAccessor.GetUserDataContextAsync(userId);
		return new UserDataViewModel(userDataContext, isCurrentUser: false);
	}

	/// <summary>
	/// Updates the personal information of the currently signed-in user.
	/// </summary>
	/// <param name="userData">A partial model of user data.</param>
	[HttpPatch("me/info")]
	[Tags("UserInfo")]
	public async Task UpdateCurrentUserData(UserDataViewModel userData)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		static string? DefaultIfEmpty(string? value, string? defaultValue) => string.IsNullOrWhiteSpace(value) ? defaultValue : value;

		// TODO: move it to a processor
		await this.updateUserDataCommand.UpdateUserDataAsync(userContext.UserId, (data) =>
		{
			var firstName = DefaultIfEmpty(userData.FirstName, data.FirstName);
			var lastName = DefaultIfEmpty(userData.LastName, data.LastName);
			var bio = userData.Bio ?? data.Bio;
			var pronouns = userData.Pronouns ?? data.Pronouns;
			var showPronouns = userData.ShowPronouns ?? data.ShowPronouns;
			var exportName = userData.ExportName ?? data.ExportName;
			var lang = userData.Language ?? data.UserLang;
			return new ExtendedUserData(data.Email, firstName!, lastName!)
			{
				Bio = bio,
				ExportName = exportName,
				Pronouns = pronouns,
				ShowPronouns = showPronouns,
				UserLang = lang,
			};
		}, this.HttpContext.RequestAborted);
	}

	/// <summary>
	/// Retrieves the url of the avatar of the currently signed-in user.
	/// </summary>
	/// <returns><c>null</c> if user has no avatar, a uri to the image otherwise</returns>
	[HttpGet("me/avatar")]
	[Tags("UserAvatar")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<Uri?> GetCurrentUserAvatar()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var avatarContext = await this.contextAccessor.GetUserAvatarContextAsync(userContext.UserId);
		return avatarContext.AvatarUri;
	}

	/// <summary>
	/// Retrieves the url of the avatar of another user.
	/// </summary>
	/// <returns><c>null</c> if user has no avatar, a uri to the image otherwise</returns>
	[HttpGet("{userId}/avatar")]
	[Tags("UserAvatar")]
	[ProducesResponseType(typeof(Uri), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<Uri?> GetUserAvatar([FromRoute] UserIdentifier userId)
	{
		var avatarContext = await this.contextAccessor.GetUserAvatarContextAsync(userId);
		return avatarContext.AvatarUri;
	}

	/// <summary>
	/// Updates the avatar of the currently signed-in user.
	/// </summary>
	/// <param name="avatarBlob">Avatar image file contents (streamable).</param>
	/// <returns>A url to download the uploaded avatar from.</returns>
	[HttpPut("me/avatar")]
	[Tags("UserAvatar")]
	public async Task<Uri> UpdateCurrentUserAvatar(IFormFile avatarBlob)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		// TODO: move it to a processor
		var avatarUri = await this.updateUserAvatarCommand.UpdateUserAvatarAsync(
			userContext.UserId,
			avatarBlob.ContentType,
			avatarBlob.OpenReadStream(),
			this.HttpContext.RequestAborted);
		return avatarUri;
	}
}
