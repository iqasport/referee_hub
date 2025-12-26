using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Domain.User.Roles;

public record TeamManagerRole(TeamConstraint Team) : ITeamUserRole
{
}
