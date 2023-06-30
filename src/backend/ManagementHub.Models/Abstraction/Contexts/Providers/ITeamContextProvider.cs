using System.Linq;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;
public interface ITeamContextProvider
{
	IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs);
}
