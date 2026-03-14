using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface ITournamentContextProvider
{
	/// <summary>
	/// Query tournaments with filtering for private tournaments and IsCurrentUserInvolved computation.
	/// All filtering and computation is done at the database level via joins.
	/// </summary>
	/// <param name="userId">Current user ID. Used to filter private tournaments and compute IsCurrentUserInvolved via database joins.</param>
	IQueryable<ITournamentContext> QueryTournaments(UserIdentifier userId);

	Task<ITournamentContext> GetTournamentContextAsync(TournamentIdentifier tournamentId, UserIdentifier userId, CancellationToken cancellationToken = default);

	Task<TournamentIdentifier> CreateTournamentAsync(TournamentData tournamentData, UserIdentifier creatorUserId, CancellationToken cancellationToken = default);

	Task UpdateTournamentAsync(TournamentIdentifier tournamentId, TournamentData tournamentData, CancellationToken cancellationToken = default);

	Task<Uri?> GetTournamentBannerUriAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task<HashSet<TournamentIdentifier>> GetUserInvolvedTournamentIdsAsync(IEnumerable<TournamentIdentifier> tournamentIds, UserIdentifier userId, CancellationToken cancellationToken = default);

	Task<IEnumerable<ManagerInfo>> GetTournamentManagersAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task AddTournamentManagerAsync(TournamentIdentifier tournamentId, UserIdentifier userId, UserIdentifier addedByUserId, CancellationToken cancellationToken = default);

	Task<bool> RemoveTournamentManagerAsync(TournamentIdentifier tournamentId, UserIdentifier userId, CancellationToken cancellationToken = default);

	// Phase 3: Invite management
	Task<IEnumerable<InviteInfo>> GetTournamentInvitesAsync(TournamentIdentifier tournamentId, UserIdentifier? filterByParticipant = null, CancellationToken cancellationToken = default);

	Task<IEnumerable<InviteInfo>> GetTeamInvitesAsync(TeamIdentifier teamId, CancellationToken cancellationToken = default);

	Task<InviteInfo> CreateTeamInviteAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId, UserIdentifier initiatorUserId, CancellationToken cancellationToken = default);

	Task<InviteInfo?> GetTeamInviteAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId, CancellationToken cancellationToken = default);

	Task UpdateInviteApprovalAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId, bool isTournamentManager, bool approved, CancellationToken cancellationToken = default);

	// Phase 3: Team participant management
	Task<IEnumerable<TeamParticipantInfo>> GetTournamentTeamParticipantsAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default);

	Task AddTeamParticipantAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId, CancellationToken cancellationToken = default);

	Task RemoveTeamParticipantAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId, CancellationToken cancellationToken = default);

	// Phase 4: Roster management
	Task UpdateParticipantRosterAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId, RosterUpdateData rosterData, CancellationToken cancellationToken = default);
}

