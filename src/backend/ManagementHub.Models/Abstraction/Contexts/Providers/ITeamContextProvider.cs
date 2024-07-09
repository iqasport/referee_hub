using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;
public interface ITeamContextProvider
{
	Task<ITeamContext> CreateTeamAsync(NgbIdentifier ngb, TeamData teamData);
	Task<ITeamContext> UpdateTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId, TeamData teamData);
	Task<ITeamContext> GetTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId);
	Task DeleteTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId);
	IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs);
}
