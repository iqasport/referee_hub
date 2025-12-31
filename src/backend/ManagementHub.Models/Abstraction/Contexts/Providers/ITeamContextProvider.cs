using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface ITeamContextProvider
{
	Task<ITeamContext> CreateTeamAsync(NgbIdentifier ngb, TeamData teamData);
	Task<ITeamContext> UpdateTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId, TeamData teamData);
	Task<ITeamContext?> GetTeamAsync(TeamIdentifier teamId);
	Task<bool> CheckTeamExistsInNgbAsync(NgbIdentifier ngb, TeamIdentifier teamId);
	Task DeleteTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId);
	IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs);
	Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId);
}
