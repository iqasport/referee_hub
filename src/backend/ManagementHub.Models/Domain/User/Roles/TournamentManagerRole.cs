using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Models.Domain.User.Roles;

/// <summary>
/// Role that allows a person to manage everything associated with a tournament.
/// </summary>
public record TournamentManagerRole(TournamentConstraint Tournament) : ITournamentUserRole
{
}
