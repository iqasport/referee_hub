using ManagementHub.Models.Domain.General;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbInfoViewModel : NgbViewModel
{
	public required NgbStatsViewModel Stats { get; set; }
	public required IEnumerable<SocialAccount> SocialAccounts { get; set; }
}
