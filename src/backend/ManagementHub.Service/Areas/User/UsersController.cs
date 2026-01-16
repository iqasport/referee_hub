using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Services;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Storage;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options.Contextual;

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
	private readonly ISetUserAttributeCommand setUserAttributeCommand;
	private readonly IUserDelicateInfoService userDelicateInfoService;
	private readonly ManagementHubDbContext dbContext;
	private readonly IContextualOptions<FeatureGates> featureGatesOptions;

	public UsersController(
		IUserContextAccessor contextAccessor,
		IUpdateUserDataCommand updateUserDataCommand,
		IUpdateUserAvatarCommand updateUserAvatarCommand,
		ISetUserAttributeCommand setUserAttributeCommand,
		IUserDelicateInfoService userDelicateInfoService,
		ManagementHubDbContext dbContext,
		IContextualOptions<FeatureGates> featureGatesOptions)
	{
		this.contextAccessor = contextAccessor;
		this.updateUserDataCommand = updateUserDataCommand;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
		this.setUserAttributeCommand = setUserAttributeCommand;
		this.userDelicateInfoService = userDelicateInfoService;
		this.dbContext = dbContext;
		this.featureGatesOptions = featureGatesOptions;
	}

	/// <summary>
	/// Retrieves identity information about the currently signed-in user.
	/// </summary>
	[HttpGet("me")]
	[Tags("User")]
	public async Task<CurrentUserViewModel> GetCurrentUser()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var userAttributes = await this.contextAccessor.GetUserAttributesAsync(userContext.UserId);

		return new CurrentUserViewModel(userContext, userAttributes);
	}

	/// <summary>
	/// Retrieves feature gates for the currently signed-in user.
	/// </summary>
	[HttpGet("me/featuregates")]
	[Tags("User")]
	public async Task<FeatureGates> GetCurrentUserFeatureGates()
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var featureGates = await this.featureGatesOptions.GetAsync(
			new FeatureGatesContext { UserId = userContext.UserId.ToString() },
			this.HttpContext.RequestAborted);

		return featureGates;
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

	/// <summary>
	/// Sets a root attribute on the users account.
	/// Those can be used by the Management Hub to make decisions about showing the user certain experiences or not.
	/// </summary>
	[HttpPut("{userId}/attributes/root/{key}")]
	[Tags("User")]
	[Authorize(AuthorizationPolicies.TechAdminPolicy)] // TODO: IQA admin?
	public async Task PutRootUserAttribute(
		[FromRoute] UserIdentifier userId,
		[FromRoute] string key,
		[FromBody] JsonDocument attribute)
	{
		await this.setUserAttributeCommand.SetRootUserAttributeAsync(userId, key, attribute, this.HttpContext.RequestAborted);
	}

	/// <summary>
	/// Sets an NGB owned attribute on the users account.
	/// Those attributes can be used by NGB to correlate data from their own systems, or to aid managing referees.
	/// Attribute value has 4kB limit (any valid JSON object).
	/// </summary>
	[HttpPut("{userId}/attributes/{ngb}/{key}")]
	[Tags("User")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)] // TODO: require higher permissions here
	public async Task PutUserAttribute(
		[FromRoute] UserIdentifier userId,
		[FromRoute] NgbIdentifier ngb,
		[FromRoute][MaxLength(128)] string key,
		[FromBody] JsonDocument attribute)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		var refereeViewerRole = userContext.Roles.OfType<RefereeViewerRole>().First();
		if (!refereeViewerRole.Ngb.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		await this.setUserAttributeCommand.SetUserAttributeAsync(userId, ngb, key, attribute, this.HttpContext.RequestAborted);
	}

	// Phase 4: User gender management

	/// <summary>
	/// Get current user's gender data and tournaments where it is referenced.
	/// </summary>
	[HttpGet("me/gender")]
	[Tags("User")]
	public async Task<UserGenderViewModel> GetMyGender()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		// Single query to get gender and tournaments where user is rostered as a player
		var result = await this.dbContext.Users
			.WithIdentifier(currentUser.UserId)
			.GroupJoin(
				this.dbContext.UserDelicateInfos,
				u => u.Id,
				udi => udi.UserId,
				(u, genders) => new
				{
					UserId = u.Id,
					Gender = genders.Select(g => g.Gender).FirstOrDefault()
				})
			.FirstOrDefaultAsync(this.HttpContext.RequestAborted);

		var tournaments = new List<TournamentReferenceViewModel>();

		if (result != null)
		{
			// Get tournaments where this user is on a roster as a player
			tournaments = await this.dbContext.TournamentTeamRosterEntries
				.Where(entry => entry.UserId == result.UserId && entry.Role == RosterRole.Player)
				.Join(
					this.dbContext.TournamentTeamParticipants,
					entry => entry.TournamentTeamParticipantId,
					participant => participant.Id,
					(entry, participant) => participant.Tournament)
				.Select(t => new TournamentReferenceViewModel
				{
					Id = t.UniqueId,
					Name = t.Name,
					StartDate = t.StartDate,
					EndDate = t.EndDate
				})
				.ToListAsync(this.HttpContext.RequestAborted);
		}

		return new UserGenderViewModel
		{
			Gender = result?.Gender,
			ReferencedInTournaments = tournaments
		};
	}

	/// <summary>
	/// Delete current user's gender data.
	/// </summary>
	[HttpDelete("me/gender")]
	[Tags("User")]
	public async Task<IActionResult> DeleteMyGender()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		// Single query to delete gender data using join with Users.WithIdentifier
		await this.dbContext.UserDelicateInfos
			.Join(
				this.dbContext.Users.WithIdentifier(currentUser.UserId),
				udi => udi.UserId,
				u => u.Id,
				(udi, u) => udi)
			.ExecuteDeleteAsync(this.HttpContext.RequestAborted);

		return this.Ok();
	}

	/// <summary>
	/// Get teams managed by the current user.
	/// Returns team IDs, team names, and NGB country codes for all teams the user is a manager of.
	/// </summary>
	[HttpGet("me/managedTeams")]
	[Tags("User")]
	public async Task<List<ManagedTeamViewModel>> GetManagedTeams()
	{
		var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

		var managedTeams = await this.dbContext.TeamManagers
			.Join(
				this.dbContext.Users.WithIdentifier(currentUser.UserId),
				tm => tm.UserId,
				u => u.Id,
				(tm, u) => tm)
			.Join(
				this.dbContext.Teams,
				tm => tm.TeamId,
				t => t.Id,
				(tm, t) => new { tm, t })
			.Join(
				this.dbContext.NationalGoverningBodies,
				combined => combined.t.NationalGoverningBodyId,
				ngb => ngb.Id,
				(combined, ngb) => new ManagedTeamViewModel
				{
					TeamId = new TeamIdentifier(combined.t.Id),
					TeamName = combined.t.Name,
					Ngb = new NgbIdentifier(ngb.CountryCode)
				})
			.ToListAsync(this.HttpContext.RequestAborted);

		return managedTeams;
	}
}
