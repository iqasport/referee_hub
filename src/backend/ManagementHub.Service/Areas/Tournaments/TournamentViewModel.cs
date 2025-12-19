namespace ManagementHub.Service.Areas.Tournaments;

public class TournamentViewModel : TournamentModel
{
	public string Id { get; set; } = null!;
	public string? BannerImageUrl { get; set; }
	public bool IsCurrentUserInvolved { get; set; }
}
