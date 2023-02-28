using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.User;

[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
	private readonly ICurrentContextAccessor contextAccessor;

	public UserController(ICurrentContextAccessor contextAccessor)
	{
		this.contextAccessor = contextAccessor;
	}

	[HttpGet]
	public async Task<string> GetUser()
	{
		var user = await this.contextAccessor.GetCurrentUserContextAsync();
		return this.HttpContext.User.Identity?.Name ?? "empty";
	}
}
