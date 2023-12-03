using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbTeamViewModel
{
	/// <summary>
	/// Team identifier.
	/// </summary>
	public required TeamIdentifier TeamId { get; set; }

	/// <summary>
	/// Team name.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// The city the team is based in.
	/// </summary>
	public required string City { get; set; }

	/// <summary>
	/// The state the team is based in.
	/// </summary>
	public string? State { get; set; }

	/// <summary>
	/// The country the team is based in (for multi country Ngbs).
	/// </summary>
	public string? Country { get; set; }

	/// <summary>
	/// Team status.
	/// </summary>
	public required TeamStatus Status { get; set; }

	/// <summary>
	/// Team group affiliation.
	/// </summary>
	public required TeamGroupAffiliation GroupAffiliation { get; set; }

	public required DateOnly JoinedAt { get; set; }

	public required IEnumerable<SocialAccount> SocialAccounts { get; set; }

}
