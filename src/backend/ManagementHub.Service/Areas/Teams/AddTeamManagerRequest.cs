using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Request model for adding a team manager.
/// </summary>
public class AddTeamManagerRequest
{
	/// <summary>
	/// Email address or username of the user to add as manager.
	/// </summary>
	[Required]
	public required string Email { get; init; }
}
