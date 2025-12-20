using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface ITournamentContextProvider
{
	/// <summary>
	/// Query tournaments with optional filtering for private tournaments.
	/// </summary>
	/// <param name="userManagedTournamentIds">Tournament IDs that the current user manages. Used to filter private tournaments - only shows private tournaments that the user manages.</param>
	IQueryable<ITournamentContext> QueryTournaments(IEnumerable<TournamentIdentifier>? userManagedTournamentIds = null);

	Task<ITournamentContext> GetTournamentContextAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task<TournamentIdentifier> CreateTournamentAsync(TournamentData tournamentData, UserIdentifier creatorUserId, CancellationToken cancellationToken = default);

	Task UpdateTournamentAsync(TournamentIdentifier tournamentId, TournamentData tournamentData, CancellationToken cancellationToken = default);

	Task<Uri?> GetTournamentBannerUriAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task<HashSet<TournamentIdentifier>> GetUserInvolvedTournamentIdsAsync(IEnumerable<TournamentIdentifier> tournamentIds, UserIdentifier userId, CancellationToken cancellationToken = default);
}
