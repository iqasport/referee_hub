using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentViewModel : TournamentModel
{
	public required TournamentIdentifier Id { get; set; }
	public string? BannerImageUrl { get; set; }
	public bool IsCurrentUserInvolved { get; set; }
}
