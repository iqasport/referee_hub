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
	private readonly ICurrentContextAccessor contextAccessor;

	public UsersController(ICurrentContextAccessor contextAccessor)
	{
		this.contextAccessor = contextAccessor;
	}

	/// <summary>
	/// Retrieves identity information about the currently signed-in user.
	/// </summary>
	[HttpGet("me")]
	public async Task<CurrentUserViewModel> GetCurrentUser()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		return new CurrentUserViewModel(userContext);
	}

	/// <summary>
	/// Retrieves personal information about the currently signed-in user.
	/// </summary>
	[HttpGet("me/info")]
	public async Task<UserDataViewModel> GetCurrentUserData()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var userDataContext = await this.contextAccessor.GetUserDataContextAsync(userContext.UserId);
		return new UserDataViewModel(userDataContext, isCurrentUser: true);
	}

	/// <summary>
	/// Updates the personal information of the currently signed-in user.
	/// </summary>
	/// <param name="userData">A partial model of user data.</param>
	[HttpPatch("me/info")]
	public async Task UpdateUserData(UserDataViewModel userData)
	{
		// TODO: invoke a command that will persist user data in db
		throw new NotImplementedException();
	}

	/// <summary>
	/// Retrieves the url of the avatar of the currently signed-in user.
	/// </summary>
	/// <returns><c>null</c> if user has no avatar, a uri to the image otherwise</returns>
	[HttpGet("me/avatar")]
	public async Task<Uri?> GetAvatar()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Updates the avatar of the currently signed-in user.
	/// </summary>
	/// <param name="avatarBlob">Avatar image file contents (streamable).</param>
	/// <returns>A url to download the uploaded avatar from.</returns>
	[HttpPut("me/avatar")]
	public async Task<Uri> UpdateAvatar(IFormFile avatarBlob)
	{
		throw new NotImplementedException();
	}
}
