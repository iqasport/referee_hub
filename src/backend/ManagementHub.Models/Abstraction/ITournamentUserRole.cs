using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Models.Abstraction;

public interface ITournamentUserRole : IUserRole
{
	TournamentConstraint Tournament { get; }
}
