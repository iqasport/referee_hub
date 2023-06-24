using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
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

	public NgbsController(INgbContextProvider ngbContextProvider, ITeamContextProvider teamContextProvider)
	{
		this.ngbContextProvider = ngbContextProvider;
		this.teamContextProvider = teamContextProvider;
	}

	[HttpGet]
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
		});
	}

	[HttpGet("{ngb}/teams")]
	public IEnumerable<NgbTeamViewModel> GetNgbTeam([FromRoute] NgbIdentifier ngb)
	{
		return this.teamContextProvider.GetTeams(NgbConstraint.Single(ngb)).Select(team => new NgbTeamViewModel
		{
			TeamId = team.TeamId,
			City = team.TeamData.City,
			GroupAffiliation = team.TeamData.GroupAffiliation,
			Name = team.TeamData.Name,
			Status = team.TeamData.Status,
			State = team.TeamData.State,
		});
	}
}
