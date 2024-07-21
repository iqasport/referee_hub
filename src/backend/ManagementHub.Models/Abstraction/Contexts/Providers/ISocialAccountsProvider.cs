using System.Collections.Generic;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;
public interface ISocialAccountsProvider
{
	Task<Dictionary<TeamIdentifier, IEnumerable<SocialAccount>>> QueryTeamSocialAccounts(NgbConstraint ngbConstraint);

	Task<Dictionary<NgbIdentifier, IEnumerable<SocialAccount>>> QueryNgbSocialAccounts(NgbConstraint ngbConstraint);

	Task<IEnumerable<SocialAccount>> UpdateTeamSocialAccounts(TeamIdentifier teamId, IEnumerable<SocialAccount> socialAccounts);
	Task<IEnumerable<SocialAccount>> GetTeamSocialAccounts(TeamIdentifier teamId);
	Task<IEnumerable<SocialAccount>> UpdateNgbSocialAccounts(NgbIdentifier ngb, IEnumerable<SocialAccount> socialAccounts);
}
