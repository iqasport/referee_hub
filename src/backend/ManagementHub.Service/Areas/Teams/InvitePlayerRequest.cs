using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Request model for inviting a player to a team.
/// </summary>
public class InvitePlayerRequest
{
	/// <summary>
	/// Email address of the player to invite.
	/// </summary>
	[Required]
	[EmailAddress]
	public required string Email { get; set; }
}
