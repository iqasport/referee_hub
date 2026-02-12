using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Actions related to Teams.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ISocialAccountsProvider socialAccountsProvider;

	public TeamsController(
		ITeamContextProvider teamContextProvider,
		ISocialAccountsProvider socialAccountsProvider)
	{
		this.teamContextProvider = teamContextProvider;
		this.socialAccountsProvider = socialAccountsProvider;
	}

	/// <summary>
	/// Get all national teams across all NGBs.
	/// </summary>
	/// <summary>
	/// Get all national teams across all NGBs.
	/// </summary>
	/// <remarks>
	/// This endpoint materializes all teams before filtering to avoid LINQ translation issues.
	/// Since national teams are typically few in number (&lt;100), the performance impact is minimal.
	/// </remarks>
	[HttpGet("national")]
	[Tags("Team")]
	public async Task<Filtered<NgbTeamViewModel>> GetNationalTeams([FromQuery] FilteringParameters filtering)
	{
		var socialAccounts = await this.socialAccountsProvider.QueryTeamSocialAccounts(NgbConstraint.Any);
		var emptySocialAccounts = Enumerable.Empty<SocialAccount>();
		// Materialize the query before filtering to avoid LINQ translation issues
		// This is acceptable because national teams are few in number
		var teams = await this.teamContextProvider.GetTeams(NgbConstraint.Any).ToListAsync();
		return teams
			.Where(team => team.TeamData.GroupAffiliation == TeamGroupAffiliation.National)
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
}
