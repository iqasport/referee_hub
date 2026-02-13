using Amazon.S3.Model;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Areas.Tournaments;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Ngbs;

/// <summary>
/// Actions related to NGBs.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class NgbsController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly INgbContextProvider ngbContextProvider;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ITournamentContextProvider tournamentContextProvider;
	private readonly ISocialAccountsProvider socialAccountsProvider;
	private readonly IUpdateUserAvatarCommand updateUserAvatarCommand;
	private readonly IUpdateNgbAdminRoleCommand updateNgbAdminRoleCommand;
	private readonly IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand;

	public NgbsController(
		IUserContextAccessor contextAccessor,
		INgbContextProvider ngbContextProvider,
		ITeamContextProvider teamContextProvider,
		ITournamentContextProvider tournamentContextProvider,
		ISocialAccountsProvider socialAccountsProvider,
		IUpdateUserAvatarCommand updateUserAvatarCommand,
		IUpdateNgbAdminRoleCommand updateNgbAdminRoleCommand,
		IUpdateTeamManagerRoleCommand updateTeamManagerRoleCommand)
	{
		this.contextAccessor = contextAccessor;
		this.ngbContextProvider = ngbContextProvider;
		this.teamContextProvider = teamContextProvider;
		this.tournamentContextProvider = tournamentContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
		this.updateUserAvatarCommand = updateUserAvatarCommand;
		this.updateNgbAdminRoleCommand = updateNgbAdminRoleCommand;
		this.updateTeamManagerRoleCommand = updateTeamManagerRoleCommand;
	}

	/// <summary>
	/// List NGBs registered in the Hub.
	/// </summary>
	[HttpGet]
	[Tags("Ngb")]
	public Filtered<NgbViewModel> GetNgbs([FromQuery] FilteringParameters filtering)
	{
		return this.ngbContextProvider.QueryNgbs().Select(ngb => new NgbViewModel
		{
			CountryCode = ngb.NgbId.NgbCode,
			Name = ngb.NgbData.Name,
			Acronym = ngb.NgbData.Acronym,
			Country = ngb.NgbData.Country,
			MembershipStatus = ngb.NgbData.MembershipStatus,
			PlayerCount = ngb.NgbData.PlayerCount,
			Region = ngb.NgbData.Region,
			Website = ngb.NgbData.Website,
		}).AsFiltered();
	}

	/// <summary>
	/// Get NGB profile information.
	/// </summary>
	[HttpGet("{ngb}")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task<NgbInfoViewModel> GetNgbInfo([FromRoute] NgbIdentifier ngb)
	{
		var context = await this.ngbContextProvider.GetNgbContextAsync(ngb);

		var socialAccounts = await this.socialAccountsProvider.QueryNgbSocialAccounts(NgbConstraint.Single(ngb));
		var stats = await this.ngbContextProvider.GetCurrentNgbStatsAsync(ngb);
		var historicalStats = await this.ngbContextProvider.GetHistoricalNgbStatsAsync(ngb);
		var avatarUri = await this.ngbContextProvider.GetNgbAvatarUriAsync(ngb);
		var adminEmails = await this.ngbContextProvider.GetNgbAdminEmails(ngb);

		return new NgbInfoViewModel
		{
			Acronym = context.NgbData.Acronym,
			Country = context.NgbData.Country,
			CountryCode = context.NgbId.NgbCode,
			MembershipStatus = context.NgbData.MembershipStatus,
			Name = context.NgbData.Name,
			PlayerCount = context.NgbData.PlayerCount,
			Region = context.NgbData.Region,
			SocialAccounts = socialAccounts.GetValueOrDefault(ngb, Enumerable.Empty<SocialAccount>()),
			CurrentStats = stats,
			HistoricalStats = historicalStats,
			Website = context.NgbData.Website,
			AvatarUri = avatarUri,
			AdminEmails = adminEmails,
		};
	}

	[HttpPut("{ngb}")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task UpdateNgb([FromRoute] NgbIdentifier ngb, [FromBody] NgbUpdateModel model)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var permissionConstraint = userContext.Roles.OfType<NgbAdminRole>().FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();

		if (!permissionConstraint.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		var context = await this.ngbContextProvider.GetNgbContextAsync(ngb);

		var ngbData = new NgbData
		{
			Name = model.Name,
			Country = model.Country,
			Acronym = model.Acronym,
			Website = model.Website,
			PlayerCount = model.PlayerCount,
			Region = context.NgbData.Region,
			MembershipStatus = context.NgbData.MembershipStatus,
		};

		await this.ngbContextProvider.UpdateNgbInfoAsync(ngb, ngbData);

		_ = await this.socialAccountsProvider.UpdateNgbSocialAccounts(ngb, model.SocialAccounts);
	}

	[HttpPut("{ngb}/avatar")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task<Uri> UpdateNgbAvatar([FromRoute] NgbIdentifier ngb, IFormFile avatarBlob)
	{
		var avatarUri = await this.updateUserAvatarCommand.UpdateNgbAvatarAsync(
			ngb,
			avatarBlob.ContentType,
			avatarBlob.OpenReadStream(),
			this.HttpContext.RequestAborted);
		return avatarUri;
	}

	[HttpPost("{ngb}/admins")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NgbAdminCreationStatus))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(NgbAdminCreationStatus))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NgbAdminCreationStatus))]
	public async Task<NgbAdminCreationStatus> AddNgbAdmin([FromRoute] NgbIdentifier ngb, [FromBody] NgbAdminCreationModel adminModel)
	{
		if (!Email.TryParse(adminModel.Email, out var email))
		{
			this.Response.StatusCode = StatusCodes.Status400BadRequest;
			return NgbAdminCreationStatus.InvalidEmail;
		}

		var result = await this.updateNgbAdminRoleCommand.AddNgbAdminRoleAsync(ngb, email, adminModel.CreateAccountIfNotExists);
		switch (result)
		{
			case IUpdateNgbAdminRoleCommand.AddRoleResult.UserDoesNotExist:
				this.Response.StatusCode = StatusCodes.Status404NotFound;
				return NgbAdminCreationStatus.UserDoesNotExist;
			case IUpdateNgbAdminRoleCommand.AddRoleResult.RoleAdded:
				return NgbAdminCreationStatus.AdminRoleAdded;
			case IUpdateNgbAdminRoleCommand.AddRoleResult.UserCreatedWithRole:
				return NgbAdminCreationStatus.AdminUserCreated;
			default: throw new InvalidOperationException($"Unexpected result {result}");
		}
	}

	[HttpDelete("{ngb}/admins")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
	public async Task DeleteNgbAdmin([FromRoute] NgbIdentifier ngb, [FromQuery] string email)
	{
		if (!Email.TryParse(email, out var email_))
		{
			this.Response.StatusCode = StatusCodes.Status400BadRequest;
			return;
		}

		var result = await this.updateNgbAdminRoleCommand.DeleteNgbAdminRoleAsync(ngb, email_);
		this.Response.StatusCode = result ? StatusCodes.Status200OK : StatusCodes.Status404NotFound;
		return;
	}

	[HttpPut("api/v2/admin/[controller]/{ngb}")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)]
	public async Task AdminUpdateNgb([FromRoute] NgbIdentifier ngb, [FromBody] AdminNgbUpdateModel model)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		var context = await this.ngbContextProvider.GetNgbContextAsync(ngb);

		var ngbData = new NgbData
		{
			Name = model.Name,
			Country = model.Country,
			Acronym = model.Acronym,
			Website = model.Website,
			PlayerCount = model.PlayerCount,
			Region = model.Region,
			MembershipStatus = model.MembershipStatus,
		};

		await this.ngbContextProvider.UpdateNgbInfoAsync(ngb, ngbData);

		_ = await this.socialAccountsProvider.UpdateNgbSocialAccounts(ngb, model.SocialAccounts);
	}

	[HttpPost("api/v2/admin/[controller]/{ngb}")]
	[Tags("Ngb")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)]
	public async Task AdminCreateNgb([FromRoute] NgbIdentifier ngb, [FromBody] AdminNgbUpdateModel model)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		try
		{
			_ = await this.ngbContextProvider.GetNgbContextAsync(ngb);
			throw new InvalidOperationException($"NGB {ngb} already exists!");
		}
		catch (NotFoundException)
		{
			// expected
		}

		var ngbData = new NgbData
		{
			Name = model.Name,
			Country = model.Country,
			Acronym = model.Acronym,
			Website = model.Website,
			PlayerCount = model.PlayerCount,
			Region = model.Region,
			MembershipStatus = model.MembershipStatus,
		};

		await this.ngbContextProvider.CreateNgbAsync(ngb, ngbData);
	}

	/// <summary>
	/// List the teams registered under the NGB.
	/// </summary>
	[HttpGet("{ngb}/teams")]
	[Tags("Team")]
	public async Task<Filtered<NgbTeamViewModel>> GetNgbTeams([FromRoute] NgbIdentifier ngb, [FromQuery] FilteringParameters filtering)
	{
		var socialAccounts = await this.socialAccountsProvider.QueryTeamSocialAccounts(NgbConstraint.Single(ngb));
		var emptySocialAccounts = Enumerable.Empty<SocialAccount>();
		return this.teamContextProvider.GetTeams(NgbConstraint.Single(ngb))
			.Select(team => new NgbTeamViewModel
			{
				TeamId = team.TeamId,
				City = team.TeamData.City,
				GroupAffiliation = team.TeamData.GroupAffiliation,
				Name = team.TeamData.Name,
				Status = team.TeamData.Status,
				State = team.TeamData.State,
				Country = team.TeamData.Country,
				JoinedAt = DateOnly.FromDateTime(team.TeamData.JoinedAt),
				SocialAccounts = socialAccounts.GetValueOrDefault(team.TeamId, emptySocialAccounts),
				LogoUrl = team.TeamData.LogoUrl,
				Description = team.TeamData.Description,
				ContactEmail = team.TeamData.ContactEmail,
			}).AsFiltered();
	}

	[HttpPost("{ngb}/teams")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task<NgbTeamViewModel> CreateNgbTeam([FromRoute] NgbIdentifier ngb, [FromBody] NgbTeamViewModel viewModel)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var permissionConstraint = userContext.Roles.OfType<NgbAdminRole>().FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();

		if (!permissionConstraint.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		if (viewModel.TeamId != default)
		{
			throw new ArgumentException("TeamId must not be specified when creating a team. Did you mean to use PUT method to update a team?");
		}

		var teamData = new TeamData
		{
			Name = viewModel.Name,
			City = viewModel.City,
			State = viewModel.State,
			Country = viewModel.Country,
			Status = viewModel.Status,
			GroupAffiliation = viewModel.GroupAffiliation,
			JoinedAt = viewModel.JoinedAt.ToDateTime(default, DateTimeKind.Utc),
			LogoUrl = viewModel.LogoUrl,
			Description = viewModel.Description,
			ContactEmail = viewModel.ContactEmail,
		};
		var team = await this.teamContextProvider.CreateTeamAsync(ngb, teamData);
		var socialAccounts = await this.socialAccountsProvider.UpdateTeamSocialAccounts(team.TeamId, viewModel.SocialAccounts);
		return new NgbTeamViewModel
		{
			TeamId = team.TeamId,
			City = team.TeamData.City,
			GroupAffiliation = team.TeamData.GroupAffiliation,
			Name = team.TeamData.Name,
			Status = team.TeamData.Status,
			State = team.TeamData.State,
			Country = team.TeamData.Country,
			JoinedAt = DateOnly.FromDateTime(team.TeamData.JoinedAt),
			SocialAccounts = socialAccounts,
			LogoUrl = team.TeamData.LogoUrl,
			Description = team.TeamData.Description,
			ContactEmail = team.TeamData.ContactEmail,
		};
	}

	[HttpPut("{ngb}/teams/{teamId}")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task<NgbTeamViewModel> UpdateNgbTeam([FromRoute] NgbIdentifier ngb, [FromRoute] TeamIdentifier teamId, [FromBody] NgbTeamViewModel viewModel)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var permissionConstraint = userContext.Roles.OfType<NgbAdminRole>().FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();

		if (!permissionConstraint.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		if (viewModel.TeamId != teamId)
		{
			throw new ArgumentException("Team id mismatch between URL and request body.");
		}

		var teamData = new TeamData
		{
			Name = viewModel.Name,
			City = viewModel.City,
			State = viewModel.State,
			Country = viewModel.Country,
			Status = viewModel.Status,
			GroupAffiliation = viewModel.GroupAffiliation,
			JoinedAt = viewModel.JoinedAt.ToDateTime(default, DateTimeKind.Utc),
			LogoUrl = viewModel.LogoUrl,
			Description = viewModel.Description,
			ContactEmail = viewModel.ContactEmail,
		};
		var team = await this.teamContextProvider.UpdateTeamAsync(ngb, teamId, teamData);
		var socialAccounts = await this.socialAccountsProvider.UpdateTeamSocialAccounts(team.TeamId, viewModel.SocialAccounts);
		return new NgbTeamViewModel
		{
			TeamId = team.TeamId,
			City = team.TeamData.City,
			GroupAffiliation = team.TeamData.GroupAffiliation,
			Name = team.TeamData.Name,
			Status = team.TeamData.Status,
			State = team.TeamData.State,
			Country = team.TeamData.Country,
			JoinedAt = DateOnly.FromDateTime(team.TeamData.JoinedAt),
			SocialAccounts = socialAccounts,
			LogoUrl = team.TeamData.LogoUrl,
			Description = team.TeamData.Description,
			ContactEmail = team.TeamData.ContactEmail,
		};
	}

	[HttpDelete("{ngb}/teams/{teamId}")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> DeleteNgbTeam([FromRoute] NgbIdentifier ngb, [FromRoute] TeamIdentifier teamId)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var permissionConstraint = userContext.Roles.OfType<NgbAdminRole>().FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();

		if (!permissionConstraint.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		// we have to first get the team to validate it belongs to the NGB
		if (!await this.teamContextProvider.CheckTeamExistsInNgbAsync(ngb, teamId))
		{
			throw new AccessDeniedException(teamId.ToString());
		}

		_ = await this.socialAccountsProvider.UpdateTeamSocialAccounts(teamId, []);
		await this.teamContextProvider.DeleteTeamAsync(ngb, teamId);
		return this.NoContent();
	}

	/// <summary>
	/// Add a team manager to a team.
	/// NGB Admins can manage any team in their jurisdiction.
	/// Team Managers can only add managers to their own teams.
	/// </summary>
	[HttpPost("{ngb}/teams/{teamId}/managers")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamManagerCreationStatus))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
	public async Task<TeamManagerCreationStatus> AddTeamManager(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamIdentifier teamId,
		[FromBody] TeamManagerCreationModel managerModel)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// Verify team exists and belongs to the specified NGB
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Single(ngb));
		if (team == null)
		{
			throw new NotFoundException($"Team {teamId} not found");
		}

		// Verify user has permission (either NGB admin for this NGB, or team manager for this team)
		var hasNgbAdminPermission = userContext.Roles
			.OfType<NgbAdminRole>()
			.Any(role => role.Ngb.AppliesTo(team.NgbId));

		var hasTeamManagerPermission = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!hasNgbAdminPermission && !hasTeamManagerPermission)
		{
			throw new AccessDeniedException($"No permission for team {teamId}");
		}

		// Parse and validate email
		if (!Email.TryParse(managerModel.Email, out var email))
		{
			this.Response.StatusCode = StatusCodes.Status400BadRequest;
			return TeamManagerCreationStatus.InvalidEmail;
		}

		// Add manager
		var result = await this.updateTeamManagerRoleCommand.AddTeamManagerRoleAsync(
			teamId, email, managerModel.CreateAccountIfNotExists, userContext.UserId);

		return result switch
		{
			IUpdateTeamManagerRoleCommand.AddRoleResult.UserDoesNotExist =>
				TeamManagerCreationStatus.UserDoesNotExist,
			IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded =>
				TeamManagerCreationStatus.ManagerRoleAdded,
			IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole =>
				TeamManagerCreationStatus.ManagerUserCreated,
			_ => throw new InvalidOperationException($"Unexpected result {result}")
		};
	}

	/// <summary>
	/// Remove a team manager from a team.
	/// NGB Admins can manage any team in their jurisdiction.
	/// Team Managers can only remove managers from their own teams.
	/// </summary>
	[HttpDelete("{ngb}/teams/{teamId}/managers")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task DeleteTeamManager(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamIdentifier teamId,
		[FromQuery] string email)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// Verify team exists and belongs to the specified NGB
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Single(ngb));
		if (team == null)
		{
			throw new NotFoundException($"Team {teamId} not found");
		}

		// Verify user has permission (either NGB admin for this NGB, or team manager for this team)
		var hasNgbAdminPermission = userContext.Roles
			.OfType<NgbAdminRole>()
			.Any(role => role.Ngb.AppliesTo(team.NgbId));

		var hasTeamManagerPermission = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!hasNgbAdminPermission && !hasTeamManagerPermission)
		{
			throw new AccessDeniedException($"No permission for team {teamId}");
		}

		// Parse and validate email
		if (!Email.TryParse(email, out var email_))
		{
			this.Response.StatusCode = StatusCodes.Status400BadRequest;
			return;
		}

		// Remove manager
		var result = await this.updateTeamManagerRoleCommand.DeleteTeamManagerRoleAsync(
			teamId, email_);

		this.Response.StatusCode = result
			? StatusCodes.Status200OK
			: StatusCodes.Status404NotFound;
	}

	/// <summary>
	/// List team managers for a team (NGB Admin or Team Manager).
	/// </summary>
	[HttpGet("{ngb}/teams/{teamId}/managers")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
	public async Task<IEnumerable<TeamManagerViewModel>> GetTeamManagers(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamIdentifier teamId)
	{
		// Get managers with NGB constraint - will return empty if team doesn't belong to NGB
		var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Single(ngb));

		return managers.Select(m => new TeamManagerViewModel
		{
			Id = m.UserId,
			Name = m.Name,
			Email = m.Email
		});
	}

	/// <summary>
	/// List members (referees) associated with a team.
	/// </summary>
	[HttpGet("{ngb}/teams/{teamId}/members")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
	public Filtered<TeamMemberViewModel> GetTeamMembers(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamIdentifier teamId,
		[FromQuery] FilteringParameters filtering)
	{
		// Get members as queryable - NGB validation is done at the lower level
		var membersQuery = this.teamContextProvider.QueryTeamMembers(teamId, NgbConstraint.Single(ngb));

		// Convert to view model
		return membersQuery
			.Select(m => new TeamMemberViewModel
			{
				UserId = m.UserId,
				Name = m.Name
			})
			.AsFiltered();
	}

	/// <summary>
	/// List all tournament invites for a team.
	/// Team managers can view invites for their own teams.
	/// NGB admins can view invites for any team in their jurisdiction.
	/// </summary>
	[HttpGet("{ngb}/teams/{teamId}/tournamentInvites")]
	[Tags("Team")]
	[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
	public async Task<IEnumerable<TournamentInviteViewModel>> GetTeamTournamentInvites(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamIdentifier teamId)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		// Verify team exists and belongs to the specified NGB
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Single(ngb));
		if (team == null)
		{
			throw new NotFoundException($"Team {teamId} not found");
		}

		// Verify user has permission (either NGB admin for this NGB, or team manager for this team)
		var hasNgbAdminPermission = userContext.Roles
			.OfType<NgbAdminRole>()
			.Any(role => role.Ngb.AppliesTo(team.NgbId));

		var hasTeamManagerPermission = userContext.Roles
			.OfType<TeamManagerRole>()
			.Any(role => role.Team.AppliesTo(teamId));

		if (!hasNgbAdminPermission && !hasTeamManagerPermission)
		{
			throw new AccessDeniedException($"No permission for team {teamId}");
		}

		// Get all tournament invites for this specific team
		var invites = await this.tournamentContextProvider.GetTeamInvitesAsync(teamId, this.HttpContext.RequestAborted);

		return invites.Select(i => new TournamentInviteViewModel
		{
			ParticipantType = i.ParticipantType,
			ParticipantId = i.ParticipantId,
			ParticipantName = i.ParticipantName,
			Status = i.GetStatus(),
			InitiatorUserId = i.InitiatorUserId,
			CreatedAt = i.CreatedAt,
			TournamentManagerApproval = new ApprovalStatusViewModel
			{
				Status = i.TournamentManagerApproval,
				Date = i.TournamentManagerApprovalDate
			},
			ParticipantApproval = new ApprovalStatusViewModel
			{
				Status = i.ParticipantApproval,
				Date = i.ParticipantApprovalDate
			}
		});
	}
}
