using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Abstraction.Contexts;
public interface ITeamContext
{
	TeamIdentifier TeamId { get; }

	NgbIdentifier NgbId { get; }

	TeamData TeamData { get; }
}
