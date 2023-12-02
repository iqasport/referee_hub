using System.Text.Json;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.Referees;

public class RefereeViewModel
{
	/// <summary>
	/// User ID of the referee.
	/// </summary>
	public required UserIdentifier UserId { get; set; }

	/// <summary>
	/// Name of the referee (or "Anonymous" if they disallow exporting).
	/// </summary>
	public required string Name { get; set; }

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
	public TeamIndicator? PlayingTeam { get; set; }

	/// <summary>
	/// Team the referee is coaching.
	/// </summary>
	public TeamIndicator? CoachingTeam { get; set; }

	/// <summary>
	/// Certifications acquired by this referee.
	/// </summary>
	public HashSet<Certification> AcquiredCertifications { get; set; } = new HashSet<Certification>();

	/// <summary>
	/// User attributes of this referee.
	/// </summary>
	public IReadOnlyDictionary<string, JsonDocument>? Attributes { get; set; }
}

public class TeamIndicator
{
	public required TeamIdentifier Id { get; set; }
	public required string Name { get; set; }
}
