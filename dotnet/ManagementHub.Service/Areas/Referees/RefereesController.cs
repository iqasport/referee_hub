using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
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

	[HttpGet("{userId}")]
	[Authorize(AuthotizationPolicies.RefereeViewerPolicy)]
	public async Task<RefereeViewModel> GetReferee([FromRoute]UserIdentifier userId)
	{
		if (userId == default)
		{
			throw new ArgumentException("User identifier has not been provided.", nameof(userId));
		}

		var currentUserContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var viewerRole = currentUserContext.Roles.OfType<RefereeViewerRole>().First();

		var userContext = await this.contextAccessor.GetUserContextAsync(userId);
		var refereeRole = userContext.Roles.OfType<RefereeRole>().FirstOrDefault();

		if (refereeRole == null)
		{
			throw new NotFoundException(nameof(RefereeRole));
		}

		// TODO: make it a filter for the query for the user
		// TODO: consolidate accessing data and referee role into a new context and allow batch queries
		var userRelatedNgbs = new List<NgbIdentifier>(2);
		if (refereeRole.PrimaryNgb != null) userRelatedNgbs.Add(refereeRole.PrimaryNgb.Value);
		if (refereeRole.SecondaryNgb != null) userRelatedNgbs.Add(refereeRole.SecondaryNgb.Value);

		if (!userRelatedNgbs.Any(viewerRole.Ngb.AppliesTo))
		{
			throw new AccessDeniedException(userId.ToString());
		}

		var userData = (await this.contextAccessor.GetUserDataContextAsync(userId)).ExtendedUserData;

		return new RefereeViewModel
		{
			UserId = userId,
			Name = userData.ExportName ? $"{userData.FirstName} {userData.LastName}" : "Anonymous referee",
			CoachingTeam = refereeRole.CoachingTeam,
			PlayingTeam = refereeRole.PlayingTeam,
			PrimaryNgb = refereeRole.PrimaryNgb,
			SecondaryNgb = refereeRole.SecondaryNgb,
			//AcquiredCertifications = TODO
		};
	}
}
