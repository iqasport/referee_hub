using ManagementHub.Models.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.User;

[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
	[HttpGet("/")]
	public string GetUser()
	{
		return this.HttpContext.User.Identity?.Name ?? "empty";
	}
}
