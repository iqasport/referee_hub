using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.User;

[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
	private readonly ICurrentContextAccessor contextAccessor;

	public UsersController(ICurrentContextAccessor contextAccessor)
	{
		this.contextAccessor = contextAccessor;
	}

	[HttpGet("me")]
	public async Task<CurrentUserViewModel> GetCurrentUser()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		return new CurrentUserViewModel(userContext);
	}
}
