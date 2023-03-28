using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Referee;

/// <summary>
/// Actions related to the current user who is a referee.
/// </summary>
[Authorize(AuthotizationPolicies.RefereePolicy)]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class RefereeController
{
	private readonly ICurrentContextAccessor contextAccessor;

	public RefereeController(ICurrentContextAccessor contextAccessor)
	{
		this.contextAccessor = contextAccessor;
	}

	[HttpGet]
	public async Task<RefereeRole> Test()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		return userContext.Roles.OfType<RefereeRole>().Single();
	}
}
