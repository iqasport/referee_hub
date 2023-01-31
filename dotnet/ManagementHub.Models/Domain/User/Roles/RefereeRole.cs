using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role of a referee. Allows taking tests, being shown in the referee list.
/// </summary>
public class RefereeRole : IUserRole
{
    /// <summary>
    /// Indicates whether the referee is active and should be included in NGB statistics.
    /// </summary>
    public bool IsActive { get; set; }

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
    public TeamIdentifier? PlayingTeam { get; set; }

    /// <summary>
    /// Team the referee is coaching.
    /// </summary>
    public TeamIdentifier? CoachingTeam { get; set; }
}
