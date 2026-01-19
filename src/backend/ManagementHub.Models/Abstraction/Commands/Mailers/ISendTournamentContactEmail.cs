using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Misc;

namespace ManagementHub.Models.Abstraction.Commands.Mailers;

public interface ISendTournamentContactEmail
{
	Task SendTournamentContactEmailAsync(
		TournamentIdentifier tournamentId,
		UserIdentifier senderId,
		[SensitiveData] string message,
		CancellationToken cancellationToken);
}
