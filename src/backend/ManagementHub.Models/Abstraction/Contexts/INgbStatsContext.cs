using System.Collections.Generic;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Abstraction.Contexts;
public interface INgbStatsContext
{
	/// <summary>
	/// note: key with value -1 will represent uncertified referees
	/// </summary>
	Dictionary<CertificationLevel, int> RefereeCountByHighestObtainedLevelForCurrentRulebook { get; } 
	Dictionary<TeamGroupAffiliation, int> TeamCountByGroupAffiliation { get; }
	Dictionary<TeamStatus, int> TeamCountByStatus { get; }
}
