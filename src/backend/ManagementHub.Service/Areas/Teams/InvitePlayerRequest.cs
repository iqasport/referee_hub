using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Request model for inviting a player to a team.
/// </summary>
public class InvitePlayerRequest
{
	/// <summary>
	/// Email address or username of the player to invite.
	/// </summary>
	[Required]
	public required string Email { get; set; }
}
