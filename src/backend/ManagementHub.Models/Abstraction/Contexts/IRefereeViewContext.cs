using System.Collections.Generic;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;
public interface IRefereeViewContext
{
	/// <summary>
	/// User ID of the referee.
	/// </summary>
	UserIdentifier UserId { get; }

	/// <summary>
	/// Name of the referee (or "Anonymous" if they disallow exporting).
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// Primary NGB this referee is located in.
	/// </summary>
	NgbIdentifier? PrimaryNgb { get; }

	/// <summary>
	/// Secondary NGB this referee is located in.
	/// </summary>
	NgbIdentifier? SecondaryNgb { get; }

	/// <summary>
	/// Team the referee is playing for.
	/// </summary>
	TeamIdentifier? PlayingTeam { get; }

	/// <summary>
	/// Team the referee is coaching.
	/// </summary>
	TeamIdentifier? CoachingTeam { get; }

	/// <summary>
	/// Certifications acquired by this referee.
	/// </summary>
	HashSet<Certification> AcquiredCertifications { get; }

	IDictionary<TeamIdentifier, ITeamContext> TeamContext { get; }

	/// <summary>
	/// Attributes of the user.
	/// </summary>
	UserAttributes Attributes { get; }
}
