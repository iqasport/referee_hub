using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Abstraction;

public interface ITeamUserRole : IUserRole
{
	TeamConstraint Team { get; }
}
