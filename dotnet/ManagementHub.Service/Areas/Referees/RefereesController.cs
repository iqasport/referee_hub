using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Paging;
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
	private readonly IUserContextAccessor contextAccessor;
	private readonly IUpdateRefereeRoleCommand updateRefereeRoleCommand;
	private readonly IRefereeContextAccessor refereeContextAccessor;

	public RefereesController(IUserContextAccessor contextAccessor, IUpdateRefereeRoleCommand updateRefereeRoleCommand, IRefereeContextAccessor refereeContextAccessor)
	{
		this.contextAccessor = contextAccessor;
		this.updateRefereeRoleCommand = updateRefereeRoleCommand;
		this.refereeContextAccessor = refereeContextAccessor;
	}

	[HttpPatch("me")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public async Task UpdateReferee([FromBody]RefereeUpdateViewModel refereeUpdate)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		await this.updateRefereeRoleCommand.UpdateRefereeRoleAsync(userContext.UserId, refereeRole => new RefereeRole
		{
			IsActive = refereeRole.IsActive,
			// TODO: handle how to detect partial patch vs setting null actually
			CoachingTeam = refereeUpdate.CoachingTeam ?? refereeRole.CoachingTeam,
			PlayingTeam = refereeUpdate.PlayingTeam ?? refereeRole.PlayingTeam,
			PrimaryNgb = refereeUpdate.PrimaryNgb ?? refereeRole.PrimaryNgb,
			SecondaryNgb = refereeUpdate.SecondaryNgb ?? refereeRole.SecondaryNgb,
		}, this.HttpContext.RequestAborted);
	}

	[HttpGet("me")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public async Task<RefereeViewModel> GetCurrentReferee()
	{
		var context = await this.refereeContextAccessor.GetRefereeViewContextForCurrentUserAsync();
		return new RefereeViewModel
		{
			AcquiredCertifications = context.AcquiredCertifications,
			CoachingTeam = context.CoachingTeam,
			Name = context.DisplayName,
			PlayingTeam = context.PlayingTeam,
			PrimaryNgb = context.PrimaryNgb,
			SecondaryNgb = context.SecondaryNgb,
			UserId = context.UserId,
		};
	}

	[HttpGet("{userId}")]
	[Authorize(AuthotizationPolicies.RefereeViewerPolicy)]
	public async Task<RefereeViewModel> GetReferee([FromRoute]UserIdentifier userId)
	{
		if (userId == default)
		{
			throw new ArgumentException("User identifier has not been provided.", nameof(userId));
		}

		var context = await this.refereeContextAccessor.GetRefereeViewContextAsync(userId);
		return new RefereeViewModel
		{
			AcquiredCertifications = context.AcquiredCertifications,
			CoachingTeam = context.CoachingTeam,
			Name = context.DisplayName,
			PlayingTeam = context.PlayingTeam,
			PrimaryNgb = context.PrimaryNgb,
			SecondaryNgb = context.SecondaryNgb,
			UserId = context.UserId,
		};
	}

	[HttpGet]
	[Authorize(AuthotizationPolicies.RefereeViewerPolicy)]
	public async Task<IQueryable<RefereeViewModel>> GetReferees(PagingParameters paging)
	{
		var collection = await this.refereeContextAccessor.GetRefereeViewContextListAsync();
		return collection.Select(context => new RefereeViewModel
		{
			AcquiredCertifications = context.AcquiredCertifications,
			CoachingTeam = context.CoachingTeam,
			Name = context.DisplayName,
			PlayingTeam = context.PlayingTeam,
			PrimaryNgb = context.PrimaryNgb,
			SecondaryNgb = context.SecondaryNgb,
			UserId = context.UserId,
		}).Page(paging);
	}
}
