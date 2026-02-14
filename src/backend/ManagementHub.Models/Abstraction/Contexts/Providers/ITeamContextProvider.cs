using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface ITeamContextProvider
{
	Task<ITeamContext> CreateTeamAsync(NgbIdentifier ngb, TeamData teamData);
	Task<ITeamContext> UpdateTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId, TeamData teamData);
	Task<ITeamContext?> GetTeamAsync(TeamIdentifier teamId, NgbConstraint ngbs);
	Task<bool> CheckTeamExistsInNgbAsync(NgbIdentifier ngb, TeamIdentifier teamId);
	Task DeleteTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId);
	IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs, TeamGroupAffiliation? groupAffiliation = null);
	IQueryable<TeamMemberInfo> QueryTeamMembers(TeamIdentifier teamId, NgbConstraint ngbs);
	Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId, NgbConstraint ngbs);
}
