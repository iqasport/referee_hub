using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUpdateTournamentBannerCommand
{
	Task<Uri> UpdateTournamentBannerAsync(
		TournamentIdentifier tournamentId,
		string contentType,
		Stream content,
		CancellationToken cancellationToken);
}
