using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Models.Abstraction.Commands.Mailers;

public interface ISendTournamentInviteEmail
{
	Task SendTournamentInviteEmailAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		Uri hostUri,
		CancellationToken cancellationToken);
}
