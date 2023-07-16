using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbStatsViewModel
{
	/// <summary>
	/// note: key with value -1 will represent uncertified referees
	/// </summary>
	public Dictionary<CertificationLevel, int> RefereeCountByHighestObtainedLevelForCurrentRulebook { get; set; } = new();
	public Dictionary<TeamGroupAffiliation, int> TeamCountByGroupAffiliation { get; set; } = new();
	public Dictionary<TeamStatus, int> TeamCountByStatus { get; set; } = new();

	public int RefereeCount => this.RefereeCountByHighestObtainedLevelForCurrentRulebook.Values.Sum();
	public int TeamCount => this.TeamCountByStatus.Values.Sum();
}
