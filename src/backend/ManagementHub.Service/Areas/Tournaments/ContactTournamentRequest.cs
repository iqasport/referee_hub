using System.ComponentModel.DataAnnotations;

namespace ManagementHub.Service.Areas.Tournaments;

public class ContactTournamentRequest
{
	[Required]
	[StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters")]
	public required string Message { get; set; }
}
