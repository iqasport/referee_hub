using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Exceptions;
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
	public IEnumerable<NgbViewModel> GetNgbs()
	{
		return this.ngbContextProvider.QueryNgbs().Select(ngb => new NgbViewModel
		{
			CountryCode = ngb.NgbId.NgbCode,
			Name = ngb.NgbData.Name,
			Acronym = ngb.NgbData.Acronym,
			Country = ngb.NgbData.Country,
			MembershipStatus = ngb.NgbData.MembershipStatus,
			Region = ngb.NgbData.Region,
			Website = ngb.NgbData.Website,
		});
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

		var socialAccounts = await this.socialAccountsProvider.QueryNgbSocialAccounts(NgbConstraint.Single(ngb));

		var stats = await this.ngbContextProvider.GetNgbStatsAsync(ngb);

		return new NgbInfoViewModel
		{
			Acronym = context.NgbData.Acronym,
			Country = context.NgbData.Country,
			CountryCode = context.NgbId.NgbCode,
			MembershipStatus = context.NgbData.MembershipStatus,
			Name = context.NgbData.Name,
			Region = context.NgbData.Region,
			SocialAccounts = socialAccounts.GetValueOrDefault(ngb, Enumerable.Empty<SocialAccount>()),
			Stats = new NgbStatsViewModel
			{
				RefereeCountByHighestObtainedLevelForCurrentRulebook = stats.RefereeCountByHighestObtainedLevelForCurrentRulebook,
				TeamCountByGroupAffiliation = stats.TeamCountByGroupAffiliation,
				TeamCountByStatus = stats.TeamCountByStatus,
			},
			Website = context.NgbData.Website,
		};
	}

	[HttpGet("{ngb}/teams")]
	[Tags("Team")]
	public async Task<IEnumerable<NgbTeamViewModel>> GetNgbTeams([FromRoute] NgbIdentifier ngb)
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
		});
	}

}
