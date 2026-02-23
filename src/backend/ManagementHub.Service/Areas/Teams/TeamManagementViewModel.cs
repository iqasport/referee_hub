using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Ngbs;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// View model for team management page with all necessary data.
/// </summary>
public class TeamManagementViewModel
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
	/// The country the team is based in.
	/// </summary>
	public required string Country { get; set; }

	/// <summary>
	/// Team status.
	/// </summary>
	public required TeamStatus Status { get; set; }

	/// <summary>
	/// Team group affiliation.
	/// </summary>
	public required TeamGroupAffiliation GroupAffiliation { get; set; }

	/// <summary>
	/// URL to the team's logo image.
	/// </summary>
	public string? LogoUrl { get; set; }

	/// <summary>
	/// Team description.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Team contact email.
	/// </summary>
	public string? ContactEmail { get; set; }

	/// <summary>
	/// Team social media accounts.
	/// </summary>
	public required IEnumerable<SocialAccount> SocialAccounts { get; set; }

	/// <summary>
	/// Team managers.
	/// </summary>
	public required IEnumerable<TeamManagerViewModel> Managers { get; set; }

	/// <summary>
	/// Team members (players).
	/// </summary>
	public required IEnumerable<TeamMemberViewModel> Members { get; set; }

	/// <summary>
	/// Pending invitations for this team.
	/// </summary>
	public required IEnumerable<TeamInvitationViewModel> PendingInvites { get; set; }
}
