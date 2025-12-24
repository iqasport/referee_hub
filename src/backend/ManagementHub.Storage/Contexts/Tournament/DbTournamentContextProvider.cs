using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Tournament;

public record DbTournamentContext(
	TournamentIdentifier Id,
	string Name,
	string Description,
	DateOnly StartDate,
	DateOnly EndDate,
	TournamentType Type,
	string Country,
	string City,
	string? Place,
	string Organizer,
	bool IsPrivate,
	bool IsCurrentUserInvolved) : ITournamentContext;

public class DbTournamentContextProvider : ITournamentContextProvider
{
	private readonly ManagementHubDbContext dbContext;
	private readonly IAttachmentRepository attachmentRepository;
	private readonly IAccessFileCommand accessFileCommand;
	private readonly CollectionFilteringContext filteringContext;
	private readonly IDatabaseTransactionProvider transactionProvider;
	private readonly ILogger<DbTournamentContextProvider> logger;

	public DbTournamentContextProvider(
		ManagementHubDbContext dbContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFileCommand,
		CollectionFilteringContext filteringContext,
		IDatabaseTransactionProvider transactionProvider,
		ILogger<DbTournamentContextProvider> logger)
	{
		this.dbContext = dbContext;
		this.attachmentRepository = attachmentRepository;
		this.accessFileCommand = accessFileCommand;
		this.filteringContext = filteringContext;
		this.transactionProvider = transactionProvider;
		this.logger = logger;
	}

	public IQueryable<ITournamentContext> QueryTournaments(UserIdentifier userId)
	{
		var filter = this.filteringContext.FilteringParameters.Filter;
		filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";

		IQueryable<Models.Data.Tournament> filteredTournaments;
		if (this.dbContext.Database.IsNpgsql())
		{
			filteredTournaments = string.IsNullOrEmpty(filter)
				? this.dbContext.Tournaments
				: this.dbContext.Tournaments
					.Where(t => EF.Functions.ILike(t.Name, filter)
						|| EF.Functions.ILike(t.Description, filter)
						|| (t.Country != null && EF.Functions.ILike(t.Country, filter))
						|| (t.City != null && EF.Functions.ILike(t.City, filter))
						|| (t.Place != null && EF.Functions.ILike(t.Place, filter)));
		}
		else
		{
			filteredTournaments = string.IsNullOrEmpty(filter)
				? this.dbContext.Tournaments
				: this.dbContext.Tournaments
					.Where(t => EF.Functions.Like(t.Name, filter)
						|| EF.Functions.Like(t.Description, filter)
						|| (t.Country != null && EF.Functions.Like(t.Country, filter))
						|| (t.City != null && EF.Functions.Like(t.City, filter))
						|| (t.Place != null && EF.Functions.Like(t.Place, filter)));
		}

		if (this.filteringContext.FilteringMetadata != null)
		{
			this.filteringContext.FilteringMetadata.TotalCount = filteredTournaments.Count();
		}

		return this.QueryTournamentsInternal(filteredTournaments.Page(this.filteringContext.FilteringParameters), userId);
	}

	public async Task<ITournamentContext> GetTournamentContextAsync(TournamentIdentifier tournamentId, UserIdentifier userId, CancellationToken cancellationToken = default)
	{
		var constrainedQuery = this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentId.ToString());

		var tournament = await this.BuildTournamentContextQuery(constrainedQuery, userId)
			.SingleOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		return tournament;
	}

	public async Task<TournamentIdentifier> CreateTournamentAsync(TournamentData tournamentData, UserIdentifier creatorUserId, CancellationToken cancellationToken = default)
	{
		var tournamentId = TournamentIdentifier.NewTournamentId();
		var now = DateTime.UtcNow;

		var tournament = new Models.Data.Tournament
		{
			UniqueId = tournamentId.ToString(),
			Name = tournamentData.Name,
			Description = tournamentData.Description,
			StartDate = tournamentData.StartDate,
			EndDate = tournamentData.EndDate,
			Type = tournamentData.Type,
			Country = tournamentData.Country,
			City = tournamentData.City,
			Place = tournamentData.Place,
			Organizer = tournamentData.Organizer,
			IsPrivate = tournamentData.IsPrivate,
			CreatedAt = now,
			UpdatedAt = now,
		};

		this.dbContext.Tournaments.Add(tournament);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		// Add creator as tournament manager
		var user = await this.dbContext.Users
			.WithIdentifier(creatorUserId)
			.Select(u => new { u.Id })
			.SingleAsync(cancellationToken);

		var tournamentManager = new TournamentManager
		{
			TournamentId = tournament.Id,
			UserId = user.Id,
			AddedByUserId = user.Id,
			CreatedAt = now,
			UpdatedAt = now,
		};

		this.dbContext.TournamentManagers.Add(tournamentManager);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Created tournament {TournamentId} with manager {UserId}", tournamentId, creatorUserId);

		return tournamentId;
	}

	public async Task UpdateTournamentAsync(TournamentIdentifier tournamentId, TournamentData tournamentData, CancellationToken cancellationToken = default)
	{
		var tournament = await this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentId.ToString())
			.SingleOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		tournament.Name = tournamentData.Name;
		tournament.Description = tournamentData.Description;
		tournament.StartDate = tournamentData.StartDate;
		tournament.EndDate = tournamentData.EndDate;
		tournament.Type = tournamentData.Type;
		tournament.Country = tournamentData.Country;
		tournament.City = tournamentData.City;
		tournament.Place = tournamentData.Place;
		tournament.Organizer = tournamentData.Organizer;
		tournament.IsPrivate = tournamentData.IsPrivate;
		tournament.UpdatedAt = DateTime.UtcNow;

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Updated tournament {TournamentId}", tournamentId);
	}

	public async Task<Uri?> GetTournamentBannerUriAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default)
	{
		var attachment = await this.attachmentRepository.GetAttachmentAsync(tournamentId, "banner", cancellationToken);
		if (attachment == null)
		{
			return null;
		}

		return await this.accessFileCommand.GetFileAccessUriAsync(attachment.Blob.Key, TimeSpan.FromSeconds(20), cancellationToken);
	}

	public async Task<HashSet<TournamentIdentifier>> GetUserInvolvedTournamentIdsAsync(IEnumerable<TournamentIdentifier> tournamentIds, UserIdentifier userId, CancellationToken cancellationToken = default)
	{
		var tournamentIdsList = tournamentIds.Select(t => t.ToString()).ToList();

		// Get the user's database ID
		var user = await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(u => new { u.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (user == null)
		{
			return new HashSet<TournamentIdentifier>();
		}

		// Check if user is tournament manager
		var managerTournamentIds = await this.dbContext.TournamentManagers
			.Where(tm => tm.UserId == user.Id && tournamentIdsList.Contains(tm.Tournament.UniqueId))
			.Select(tm => tm.Tournament.UniqueId)
			.ToListAsync(cancellationToken);

		// Phase 3: Check if user is team manager for participating teams
		var teamManagerTournamentIds = await this.dbContext.TeamManagers
			.Where(teamMgr => teamMgr.UserId == user.Id)
			.Join(
				this.dbContext.TournamentTeamParticipants,
				teamMgr => teamMgr.TeamId,
				participant => participant.TeamId,
				(teamMgr, participant) => new { participant.Tournament.UniqueId })
			.Where(p => tournamentIdsList.Contains(p.UniqueId))
			.Select(p => p.UniqueId)
			.Distinct()
			.ToListAsync(cancellationToken);

		// Phase 4 will add: Check if user is on roster

		return managerTournamentIds
			.Concat(teamManagerTournamentIds)
			.Select(id => TournamentIdentifier.Parse(id))
			.ToHashSet();
	}

	public async Task<IEnumerable<ManagerInfo>> GetTournamentManagersAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();

		var managers = await this.dbContext.TournamentManagers
			.Where(tm => tm.Tournament.UniqueId == tournamentIdString)
			.Select(tm => new ManagerInfo
			{
				UserId = tm.User.UniqueId != null
					? UserIdentifier.Parse(tm.User.UniqueId)
					: UserIdentifier.FromLegacyUserId(tm.User.Id),
				Name = $"{tm.User.FirstName} {tm.User.LastName}",
				Email = tm.User.Email
			})
			.ToListAsync(cancellationToken);

		return managers;
	}

	public async Task AddTournamentManagerAsync(
		TournamentIdentifier tournamentId,
		UserIdentifier userId,
		UserIdentifier addedByUserId,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();

		// Verify tournament exists
		var tournament = await this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentIdString)
			.Select(t => new { t.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		// Get user's database ID
		var user = await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(u => new { u.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (user == null)
		{
			throw new NotFoundException(userId.ToString());
		}

		// Get addedBy user's database ID
		var addedByUser = await this.dbContext.Users
			.WithIdentifier(addedByUserId)
			.Select(u => new { u.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (addedByUser == null)
		{
			throw new NotFoundException(addedByUserId.ToString());
		}

		// Check if already a manager (idempotent - don't error if already manager)
		var existingManager = await this.dbContext.TournamentManagers
			.Where(tm => tm.TournamentId == tournament.Id && tm.UserId == user.Id)
			.FirstOrDefaultAsync(cancellationToken);

		if (existingManager != null)
		{
			this.logger.LogInformation("User {UserId} is already a manager of tournament {TournamentId}",
				userId, tournamentId);
			return;
		}

		// Add manager
		var now = DateTime.UtcNow;
		var tournamentManager = new TournamentManager
		{
			TournamentId = tournament.Id,
			UserId = user.Id,
			AddedByUserId = addedByUser.Id,
			CreatedAt = now,
			UpdatedAt = now,
		};

		this.dbContext.TournamentManagers.Add(tournamentManager);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Added manager {UserId} to tournament {TournamentId} by {AddedByUserId}",
			userId, tournamentId, addedByUserId);
	}

	public async Task<bool> RemoveTournamentManagerAsync(
		TournamentIdentifier tournamentId,
		UserIdentifier userId,
		CancellationToken cancellationToken = default)
	{
		await using var transaction = await this.transactionProvider.BeginAsync();

		var tournamentIdString = tournamentId.ToString();

		// Get tournament database ID
		var tournament = await this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentIdString)
			.Select(t => new { t.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		// Check manager count first - if only 1 manager exists, throw exception
		var managerCount = await this.dbContext.TournamentManagers
			.Where(tm => tm.TournamentId == tournament.Id)
			.CountAsync(cancellationToken);

		if (managerCount <= 1)
		{
			throw new InvalidOperationException("Cannot remove the last manager of a tournament");
		}

		// Get user's database ID
		var user = await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(u => new { u.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (user == null)
		{
			return false; // User doesn't exist, can't be a manager
		}

		// Find and remove the manager
		var manager = await this.dbContext.TournamentManagers
			.Where(tm => tm.TournamentId == tournament.Id && tm.UserId == user.Id)
			.FirstOrDefaultAsync(cancellationToken);

		if (manager == null)
		{
			return false; // User is not a manager
		}

		this.dbContext.TournamentManagers.Remove(manager);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		await transaction.CommitAsync(cancellationToken);

		this.logger.LogInformation("Removed manager {UserId} from tournament {TournamentId}", userId, tournamentId);

		return true;
	}

	private IQueryable<ITournamentContext> QueryTournamentsInternal(IQueryable<Models.Data.Tournament> tournaments, UserIdentifier userId)
	{
		// Get the user's unique ID for the Where clause
		var userUniqueId = userId.ToString();

		// Order and filter the tournaments BEFORE projection to allow EF Core to translate the query
		var filteredTournaments = tournaments
			// Sort by whether user is involved (tournaments where user is manager or team manager of participant appear first)
			// This is computed as a subquery that EF Core can translate to SQL
			.OrderByDescending(t => t.TournamentManagers.Any(tm => tm.User.UniqueId == userUniqueId) ||
				t.TournamentTeamParticipants.Any(p => p.Team.TeamManagers.Any(teamMgr => teamMgr.User.UniqueId == userUniqueId)))
			// Then sort by start date (most recent first)
			.ThenByDescending(t => t.StartDate)
			// Filter private tournaments at the entity level BEFORE projection
			// Only show private tournaments where the user is involved (is a tournament manager OR team manager of participant)
			// Public tournaments are visible to everyone
			.Where(t => !t.IsPrivate ||
				t.TournamentManagers.Any(tm => tm.User.UniqueId == userUniqueId) ||
				t.TournamentTeamParticipants.Any(p => p.Team.TeamManagers.Any(teamMgr => teamMgr.User.UniqueId == userUniqueId)));

		return this.BuildTournamentContextQuery(filteredTournaments, userId);
	}

	/// <summary>
	/// Builds a tournament context query with joins to related tables and IsCurrentUserInvolved computation.
	/// This shared function is used by both single and batch tournament queries to ensure consistent logic.
	/// </summary>
	/// <param name="tournaments">Constrained IQueryable of tournaments (can be filtered by ID, search terms, etc.)</param>
	/// <param name="userId">The user identifier for computing IsCurrentUserInvolved</param>
	/// <returns>IQueryable of ITournamentContext with all joins and computations applied</returns>
	private IQueryable<ITournamentContext> BuildTournamentContextQuery(IQueryable<Models.Data.Tournament> tournaments, UserIdentifier userId)
	{
		// Get the user's unique ID for joins
		// This will be used in the select projection for database-level joins
		var userUniqueId = userId.ToString();

		return tournaments.AsNoTracking()
			.Select(t => new DbTournamentContext(
				TournamentIdentifier.Parse(t.UniqueId),
				t.Name,
				t.Description,
				t.StartDate,
				t.EndDate,
				t.Type,
				t.Country,
				t.City,
				t.Place,
				t.Organizer,
				t.IsPrivate,
				// IsCurrentUserInvolved: computed via database join
				// User is involved if they manage this tournament OR manage a participating team
				// Phase 4 will extend: || user is on a roster
				t.TournamentManagers.Any(tm => tm.User.UniqueId == userUniqueId) ||
				t.TournamentTeamParticipants.Any(p => p.Team.TeamManagers.Any(teamMgr => teamMgr.User.UniqueId == userUniqueId))));
	}

	// Phase 3: Invite management methods

	public async Task<IEnumerable<InviteInfo>> GetTournamentInvitesAsync(
		TournamentIdentifier tournamentId,
		UserIdentifier? filterByParticipant = null,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();

		var query = this.dbContext.TournamentInvites
			.Where(i => i.Tournament.UniqueId == tournamentIdString);

		// If filtering by participant, filter by user's teams
		if (filterByParticipant != null)
		{
			var userUniqueId = filterByParticipant.ToString();
			// Get team IDs where user is a team manager
			var userTeamIds = await this.dbContext.TeamManagers
				.Where(tm => tm.User.UniqueId == userUniqueId)
				.Select(tm => new TeamIdentifier(tm.TeamId).ToString())
				.ToListAsync(cancellationToken);

			// If user is not a team manager of any team, return empty list
			if (!userTeamIds.Any())
			{
				return Enumerable.Empty<InviteInfo>();
			}

			query = query.Where(i => userTeamIds.Contains(i.ParticipantId));
		}

		var invites = await query
			.Select(i => new InviteInfo
			{
				TournamentId = TournamentIdentifier.Parse(i.Tournament.UniqueId),
				ParticipantType = i.ParticipantType == "team" ? ParticipantType.Team : ParticipantType.Team,
				ParticipantId = i.ParticipantId,
				ParticipantName = i.ParticipantType == "team"
					? this.dbContext.Teams.Where(t => new TeamIdentifier(t.Id).ToString() == i.ParticipantId).Select(t => t.Name).FirstOrDefault() ?? "Unknown"
					: "Unknown",
				InitiatorUserId = i.Initiator.UniqueId != null
					? UserIdentifier.Parse(i.Initiator.UniqueId)
					: UserIdentifier.FromLegacyUserId(i.Initiator.Id),
				CreatedAt = i.CreatedAt,
				TournamentManagerApproval = i.TournamentManagerApproval,
				TournamentManagerApprovalDate = i.TournamentManagerApprovalDate,
				ParticipantApproval = i.ParticipantApproval,
				ParticipantApprovalDate = i.ParticipantApprovalDate
			})
			.ToListAsync(cancellationToken);

		return invites;
	}

	public async Task<InviteInfo> CreateTeamInviteAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		UserIdentifier initiatorUserId,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();
		var participantId = teamId.ToString();

		// Get tournament database ID
		var tournament = await this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentIdString)
			.Select(t => new { t.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		// Get initiator's database ID
		var initiator = await this.dbContext.Users
			.WithIdentifier(initiatorUserId)
			.Select(u => new { u.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (initiator == null)
		{
			throw new NotFoundException(initiatorUserId.ToString());
		}

		// Check if user is tournament manager
		var isTournamentManager = await this.dbContext.TournamentManagers
			.AnyAsync(tm => tm.TournamentId == tournament.Id && tm.UserId == initiator.Id, cancellationToken);

		// Check if user is team manager
		var isTeamManager = await this.dbContext.TeamManagers
			.AnyAsync(tm => tm.TeamId == teamId.Id && tm.UserId == initiator.Id, cancellationToken);

		var now = DateTime.UtcNow;

		var invite = new TournamentInvite
		{
			TournamentId = tournament.Id,
			ParticipantType = "team",
			ParticipantId = participantId,
			InitiatorUserId = initiator.Id,
			CreatedAt = now,
			// Auto-approve if user has both roles
			TournamentManagerApproval = isTournamentManager ? ApprovalStatus.Approved : ApprovalStatus.Pending,
			TournamentManagerApprovalDate = isTournamentManager ? now : null,
			ParticipantApproval = isTeamManager ? ApprovalStatus.Approved : ApprovalStatus.Pending,
			ParticipantApprovalDate = isTeamManager ? now : null,
		};

		this.dbContext.TournamentInvites.Add(invite);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Created invite for tournament {TournamentId} team {TeamId}",
			tournamentId, teamId);

		// Fetch the created invite to return with proper team name
		var createdInvite = await this.GetTeamInviteAsync(tournamentId, teamId, cancellationToken);
		return createdInvite!;
	}

	public async Task<InviteInfo?> GetTeamInviteAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();
		var participantId = teamId.ToString();

		var invite = await this.dbContext.TournamentInvites
			.Where(i => i.Tournament.UniqueId == tournamentIdString && i.ParticipantId == participantId)
			.OrderByDescending(i => i.CreatedAt)
			.FirstOrDefaultAsync(cancellationToken);

		if (invite == null)
		{
			return null;
		}

		return new InviteInfo
		{
			TournamentId = TournamentIdentifier.Parse(invite.Tournament.UniqueId),
			ParticipantType = ParticipantType.Team,
			ParticipantId = invite.ParticipantId,
			ParticipantName = await this.dbContext.Teams
				.Where(t => new TeamIdentifier(t.Id).ToString() == invite.ParticipantId)
				.Select(t => t.Name)
				.FirstOrDefaultAsync(cancellationToken) ?? "Unknown",
			InitiatorUserId = invite.Initiator.UniqueId != null
				? UserIdentifier.Parse(invite.Initiator.UniqueId)
				: UserIdentifier.FromLegacyUserId(invite.Initiator.Id),
			CreatedAt = invite.CreatedAt,
			TournamentManagerApproval = invite.TournamentManagerApproval,
			TournamentManagerApprovalDate = invite.TournamentManagerApprovalDate,
			ParticipantApproval = invite.ParticipantApproval,
			ParticipantApprovalDate = invite.ParticipantApprovalDate
		};
	}

	public async Task UpdateInviteApprovalAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		bool isTournamentManager,
		bool approved,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();
		var participantId = teamId.ToString();

		var invite = await this.dbContext.TournamentInvites
			.Where(i => i.Tournament.UniqueId == tournamentIdString && i.ParticipantId == participantId)
			.OrderByDescending(i => i.CreatedAt)
			.FirstOrDefaultAsync(cancellationToken);

		if (invite == null)
		{
			throw new NotFoundException($"Invite for tournament {tournamentId} and team {teamId}");
		}

		var now = DateTime.UtcNow;
		var newStatus = approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;

		if (isTournamentManager)
		{
			invite.TournamentManagerApproval = newStatus;
			invite.TournamentManagerApprovalDate = now;
		}
		else
		{
			invite.ParticipantApproval = newStatus;
			invite.ParticipantApprovalDate = now;
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Updated invite approval for tournament {TournamentId} team {TeamId}: {ApproverType} = {Status}",
			tournamentId, teamId, isTournamentManager ? "TournamentManager" : "Participant", newStatus);
	}

	// Phase 3: Participant management methods

	public async Task<IEnumerable<TeamParticipantInfo>> GetTournamentTeamParticipantsAsync(
		TournamentIdentifier tournamentId,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();

		var participants = await this.dbContext.TournamentTeamParticipants
			.Where(p => p.Tournament.UniqueId == tournamentIdString)
			.Select(p => new TeamParticipantInfo
			{
				TeamId = new TeamIdentifier(p.TeamId),
				TeamName = p.TeamName,
				CreatedAt = p.CreatedAt
			})
			.ToListAsync(cancellationToken);

		return participants;
	}

	public async Task AddTeamParticipantAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();

		// Get tournament database ID
		var tournament = await this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentIdString)
			.Select(t => new { t.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		// Get team and its name
		var team = await this.dbContext.Teams
			.Where(t => t.Id == teamId.Id)
			.Select(t => new { t.Id, t.Name })
			.FirstOrDefaultAsync(cancellationToken);

		if (team == null)
		{
			throw new NotFoundException(teamId.ToString());
		}

		// Check if already a participant (idempotent)
		var existingParticipant = await this.dbContext.TournamentTeamParticipants
			.AnyAsync(p => p.TournamentId == tournament.Id && p.TeamId == team.Id, cancellationToken);

		if (existingParticipant)
		{
			this.logger.LogInformation("Team {TeamId} is already a participant of tournament {TournamentId}",
				teamId, tournamentId);
			return;
		}

		var now = DateTime.UtcNow;

		var participant = new TournamentTeamParticipant
		{
			TournamentId = tournament.Id,
			TeamId = team.Id,
			TeamName = team.Name,
			CreatedAt = now,
			UpdatedAt = now,
		};

		this.dbContext.TournamentTeamParticipants.Add(participant);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Added team {TeamId} as participant to tournament {TournamentId}",
			teamId, tournamentId);
	}

	public async Task RemoveTeamParticipantAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		CancellationToken cancellationToken = default)
	{
		var tournamentIdString = tournamentId.ToString();

		// Get tournament database ID
		var tournament = await this.dbContext.Tournaments
			.Where(t => t.UniqueId == tournamentIdString)
			.Select(t => new { t.Id })
			.FirstOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			throw new NotFoundException(tournamentId.ToString());
		}

		// Find and remove the participant
		var participant = await this.dbContext.TournamentTeamParticipants
			.Where(p => p.TournamentId == tournament.Id && p.TeamId == teamId.Id)
			.FirstOrDefaultAsync(cancellationToken);

		if (participant == null)
		{
			this.logger.LogInformation("Team {TeamId} is not a participant of tournament {TournamentId}",
				teamId, tournamentId);
			return;
		}

		this.dbContext.TournamentTeamParticipants.Remove(participant);
		await this.dbContext.SaveChangesAsync(cancellationToken);

		this.logger.LogInformation("Removed team {TeamId} as participant from tournament {TournamentId}",
			teamId, tournamentId);
	}
}
