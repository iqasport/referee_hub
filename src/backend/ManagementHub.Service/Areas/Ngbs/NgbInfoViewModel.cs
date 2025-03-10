using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.General;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbInfoViewModel : NgbViewModel
{
	public required INgbStatsContext CurrentStats { get; set; }
	public required IEnumerable<INgbStatsContext> HistoricalStats { get; set; }
	public required IEnumerable<SocialAccount> SocialAccounts { get; set; }
	public required Uri? AvatarUri { get; set; }
	public required IEnumerable<string> AdminEmails { get; set; }
}
