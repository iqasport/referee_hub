using Amazon.S3.Model;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
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
	private readonly ISocialAccountsProvider socialAccountsProvider;

	public NgbsController(IUserContextAccessor contextAccessor, INgbContextProvider ngbContextProvider, ITeamContextProvider teamContextProvider, ISocialAccountsProvider socialAccountsProvider)
	{
		this.contextAccessor = contextAccessor;
		this.ngbContextProvider = ngbContextProvider;
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
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
	public async Task<NgbInfoViewModel> GetNgbInfo([FromRoute] NgbIdentifier ngb)
	{
		var context = await this.ngbContextProvider.GetNgbContextAsync(ngb);

		var socialAccounts = await this.socialAccountsProvider.QueryNgbSocialAccounts(NgbConstraint.Single(ngb));
		var stats = await this.ngbContextProvider.GetCurrentNgbStatsAsync(ngb);
		var historicalStats = await this.ngbContextProvider.GetHistoricalNgbStatsAsync(ngb);
		var avatarUri = await this.ngbContextProvider.GetNgbAvatarUriAsync(ngb);

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
		} catch (NotFoundException)
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
			JoinedAt = viewModel.JoinedAt.ToDateTime(default),
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
			JoinedAt = viewModel.JoinedAt.ToDateTime(default),
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
		if(!await this.teamContextProvider.CheckTeamExistsInNgbAsync(ngb, teamId))
		{
			throw new AccessDeniedException(teamId.ToString());
		}

		_ = await this.socialAccountsProvider.UpdateTeamSocialAccounts(teamId, []);
		await this.teamContextProvider.DeleteTeamAsync(ngb, teamId);
		return this.NoContent();
	}
}
