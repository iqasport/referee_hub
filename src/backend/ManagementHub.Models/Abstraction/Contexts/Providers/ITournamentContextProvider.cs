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
	/// Query tournaments with filtering for private tournaments and IsCurrentUserInvolved computation.
	/// All filtering and computation is done at the database level via joins.
	/// </summary>
	/// <param name="userId">Current user ID. Used to filter private tournaments and compute IsCurrentUserInvolved via database joins.</param>
	IQueryable<ITournamentContext> QueryTournaments(UserIdentifier userId);

	Task<ITournamentContext> GetTournamentContextAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task<TournamentIdentifier> CreateTournamentAsync(TournamentData tournamentData, UserIdentifier creatorUserId, CancellationToken cancellationToken = default);

	Task UpdateTournamentAsync(TournamentIdentifier tournamentId, TournamentData tournamentData, CancellationToken cancellationToken = default);

	Task<Uri?> GetTournamentBannerUriAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task<HashSet<TournamentIdentifier>> GetUserInvolvedTournamentIdsAsync(IEnumerable<TournamentIdentifier> tournamentIds, UserIdentifier userId, CancellationToken cancellationToken = default);
}
