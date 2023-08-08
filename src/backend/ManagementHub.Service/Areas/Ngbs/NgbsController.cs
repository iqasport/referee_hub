using Amazon.S3.Model;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Exceptions;
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
	private readonly INgbContextProvider ngbContextProvider;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ISocialAccountsProvider socialAccountsProvider;

	public NgbsController(INgbContextProvider ngbContextProvider, ITeamContextProvider teamContextProvider, ISocialAccountsProvider socialAccountsProvider)
	{
		this.ngbContextProvider = ngbContextProvider;
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
	}

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

	[HttpGet("{ngb}")]
	[Tags("Ngb")]
	public async Task<NgbInfoViewModel> GetNgbInfo([FromRoute] NgbIdentifier ngb)
	{
		var context = await this.ngbContextProvider.GetNgbContextAsync(ngb);
		if (context == null)
		{
			throw new NotFoundException(ngb.ToString());
		}

		var socialAccountsTask = this.socialAccountsProvider.QueryNgbSocialAccounts(NgbConstraint.Single(ngb));
		var statsTask = this.ngbContextProvider.GetCurrentNgbStatsAsync(ngb);
		var historicalStatsTask = this.ngbContextProvider.GetHistoricalNgbStatsAsync(ngb);
		var avatarUriTask = this.ngbContextProvider.GetNgbAvatarUriAsync(ngb);

		await Task.WhenAll(socialAccountsTask, statsTask, historicalStatsTask, avatarUriTask);

		var socialAccounts = await socialAccountsTask;
		var stats = await statsTask;
		var historicalStats = await historicalStatsTask;
		var avatarUri = await avatarUriTask;

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
			SocialAccounts = socialAccounts.GetValueOrDefault(team.TeamId, emptySocialAccounts),
		}).AsFiltered();
	}

}
