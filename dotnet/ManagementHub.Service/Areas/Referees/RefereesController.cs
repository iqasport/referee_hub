using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Referees;

/// <summary>
/// Actions related to users with the referee role.
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class RefereesController : ControllerBase
{
	private readonly ICurrentContextAccessor contextAccessor;
	private readonly IUpdateRefereeRoleCommand updateRefereeRoleCommand;

	public RefereesController(ICurrentContextAccessor contextAccessor, IUpdateRefereeRoleCommand updateRefereeRoleCommand)
	{
		this.contextAccessor = contextAccessor;
		this.updateRefereeRoleCommand = updateRefereeRoleCommand;
	}

	[HttpPatch("me")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public async Task UpdateReferee([FromBody]RefereeUpdateViewModel refereeUpdate)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		await this.updateRefereeRoleCommand.UpdateRefereeRoleAsync(userContext.UserId, refereeRole => new RefereeRole
		{
			IsActive = refereeRole.IsActive,
			CoachingTeam = refereeUpdate.CoachingTeam ?? refereeRole.CoachingTeam,
			PlayingTeam = refereeUpdate.PlayingTeam ?? refereeRole.PlayingTeam,
			PrimaryNgb = refereeUpdate.PrimaryNgb ?? refereeRole.PrimaryNgb,
			SecondaryNgb = refereeUpdate.SecondaryNgb ?? refereeRole.SecondaryNgb,
		}, this.HttpContext.RequestAborted);
	}
}
