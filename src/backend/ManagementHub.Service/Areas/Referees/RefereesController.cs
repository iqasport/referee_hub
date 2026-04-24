using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
	private readonly IUpdateUserDataCommand updateUserDataCommand;
	private readonly ManagementHubDbContext dbContext;

	public RefereesController(
		IUserContextAccessor contextAccessor,
		IUpdateRefereeRoleCommand updateRefereeRoleCommand,
		IRefereeContextAccessor refereeContextAccessor,
		IUpdateUserDataCommand updateUserDataCommand,
		ManagementHubDbContext dbContext)
	{
		this.contextAccessor = contextAccessor;
		this.updateRefereeRoleCommand = updateRefereeRoleCommand;
		this.refereeContextAccessor = refereeContextAccessor;
		this.updateUserDataCommand = updateUserDataCommand;
		this.dbContext = dbContext;
	}

	/// <summary>
	/// Updates the referees metadata (Ngb, Team).
	/// </summary>
	[HttpPut("me")]
	[Tags("Referee", "User")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task UpdateCurrentReferee([FromBody] RefereeUpdateViewModel refereeUpdate)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		await this.updateRefereeRoleCommand.UpdateRefereeRoleAsync(userContext.UserId, refereeRole => new RefereeRole
		{
			IsActive = refereeRole.IsActive,
			CoachingTeam = refereeUpdate.CoachingTeam?.Id,
			PlayingTeam = refereeUpdate.PlayingTeam?.Id,
			NationalTeam = refereeUpdate.NationalTeam?.Id,
			PrimaryNgb = refereeUpdate.PrimaryNgb,
			SecondaryNgb = refereeUpdate.SecondaryNgb,
		}, this.HttpContext.RequestAborted);
	}

	/// <summary>
	/// Get the referee profile for the current user.
	/// </summary>
	[HttpGet("me")]
	[Tags("Referee", "UserInfo")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<RefereeViewModel> GetCurrentReferee()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var context = await this.refereeContextAccessor.GetRefereeViewContextForCurrentUserAsync();
		return MapRefereeViewContextToViewModel(context, GetViewerPerimissionConstraint(userContext));
	}

	/// <summary>
	/// Get the referee profile for another user.
	/// </summary>
	[HttpGet("{userId}")]
	[Tags("Referee", "UserInfo")]
	[Authorize]
	public async Task<RefereeViewModel> GetReferee([FromRoute] UserIdentifier userId)
	{
		if (userId == default)
		{
			throw new ArgumentException("User identifier has not been provided.", nameof(userId));
		}

		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var context = await this.refereeContextAccessor.GetRefereeViewContextAsync(userId);
		return MapRefereeViewContextToViewModel(context, GetViewerPerimissionConstraint(userContext));
	}

	/// <summary>
	/// Gets the referee profiles for all users (limited by viewer permissions).
	/// </summary>
	[HttpGet]
	[Tags("Referee")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)]
	public async Task<Filtered<RefereeViewModel>> GetReferees([FromQuery] FilteringParameters filtering)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var collection = await this.refereeContextAccessor.GetRefereeViewContextListAsync();
		var viewerPermissionConstraint = GetViewerPerimissionConstraint(userContext);
		return collection.Select(x => MapRefereeViewContextToViewModel(x, viewerPermissionConstraint)).AsFiltered();
	}

	/// <summary>
	/// Gets the referee profiles for all users from a given NGB (limited by viewer permissions).
	/// </summary>
	[HttpGet("/api/v2/Ngbs/{ngb}/referees")]
	[Tags("Referee")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)]
	public async Task<Filtered<RefereeViewModel>> GetNgbReferees([FromRoute] NgbIdentifier ngb, [FromQuery] FilteringParameters filtering)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var collection = await this.refereeContextAccessor.GetRefereeViewContextListAsync(ngb);
		var viewerPermissionConstraint = GetViewerPerimissionConstraint(userContext);
		return collection.Select(x => MapRefereeViewContextToViewModel(x, viewerPermissionConstraint)).AsFiltered();
	}

	/// <summary>
	/// Updates a referee's name (admin operation - no NGB scope restrictions).
	/// </summary>
	[HttpPatch("{userId}/name")]
	[Tags("Referee", "UserInfo")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateRefereeNameAdmin(
		[FromRoute] UserIdentifier userId,
		[FromBody] UpdateRefereeNameRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName))
		{
			return this.BadRequest("At least one of FirstName or LastName must be provided.");
		}

		// Verify the user exists
		var userExists = await this.dbContext.Users.WithIdentifier(userId)
			.AnyAsync(u => true, this.HttpContext.RequestAborted);

		if (!userExists)
		{
			return this.NotFound();
		}

		await this.updateUserDataCommand.UpdateUserDataAsync(userId, data =>
		{
			var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? data.FirstName : request.FirstName;
			var lastName = string.IsNullOrWhiteSpace(request.LastName) ? data.LastName : request.LastName;
			return new ManagementHub.Models.Domain.User.ExtendedUserData(data.Email, firstName, lastName)
			{
				Bio = data.Bio,
				ExportName = data.ExportName,
				Pronouns = data.Pronouns,
				ShowPronouns = data.ShowPronouns,
				UserLang = data.UserLang,
			};
		}, this.HttpContext.RequestAborted);

		return this.NoContent();
	}

	private static NgbConstraint GetViewerPerimissionConstraint(IUserContext userContext) =>
		userContext.Roles.OfType<RefereeViewerRole>().FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();

	private static RefereeViewModel MapRefereeViewContextToViewModel(IRefereeViewContext context, NgbConstraint viewerPermissionSet)
	{
		return new RefereeViewModel
		{
			AcquiredCertifications = context.AcquiredCertifications,
			CoachingTeam = context.CoachingTeam == null ? null : new TeamIndicator
			{
				Id = context.CoachingTeam.Value,
				Name = context.TeamContext[context.CoachingTeam.Value].TeamData.Name,
			},
			Name = context.DisplayName,
			PlayingTeam = context.PlayingTeam == null ? null : new TeamIndicator
			{
				Id = context.PlayingTeam.Value,
				Name = context.TeamContext[context.PlayingTeam.Value].TeamData.Name,
			},
			NationalTeam = context.NationalTeam == null ? null : new TeamIndicator
			{
				Id = context.NationalTeam.Value,
				Name = context.TeamContext[context.NationalTeam.Value].TeamData.Name,
			},
			PrimaryNgb = context.PrimaryNgb,
			SecondaryNgb = context.SecondaryNgb,
			UserId = context.UserId,
			Attributes = context.Attributes.GetPrefixedByConstraint(viewerPermissionSet),
		};
	}
}
