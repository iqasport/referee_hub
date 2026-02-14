using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Service.Areas.Referees;

public class RefereeUpdateViewModel
{
	/// <summary>
	/// Primary NGB this referee is located in.
	/// </summary>
	public NgbIdentifier? PrimaryNgb { get; set; }

	/// <summary>
	/// Secondary NGB this referee is located in.
	/// </summary>
	public NgbIdentifier? SecondaryNgb { get; set; }

	/// <summary>
	/// Team the referee is playing for.
	/// </summary>
	public RefereeTeamUpdater? PlayingTeam { get; set; }

	/// <summary>
	/// Team the referee is coaching.
	/// </summary>
	public RefereeTeamUpdater? CoachingTeam { get; set; }

	/// <summary>
	/// National team the referee is playing for.
	/// </summary>
	public RefereeTeamUpdater? NationalTeam { get; set; }
}

public class RefereeTeamUpdater
{
	public TeamIdentifier Id { get; set; }
}
