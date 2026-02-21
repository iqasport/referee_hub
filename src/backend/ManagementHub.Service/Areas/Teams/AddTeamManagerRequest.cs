using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Service.Areas.Teams;

/// <summary>
/// Request model for adding a team manager.
/// </summary>
public class AddTeamManagerRequest
{
	/// <summary>
	/// Email address of the user to add as manager.
	/// </summary>
	[Required]
	[EmailAddress]
	public required string Email { get; init; }
}
