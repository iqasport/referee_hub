using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Collections;
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
	private readonly ILogger<DbTournamentContextProvider> logger;

	public DbTournamentContextProvider(
		ManagementHubDbContext dbContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFileCommand,
		CollectionFilteringContext filteringContext,
		ILogger<DbTournamentContextProvider> logger)
	{
		this.dbContext = dbContext;
		this.attachmentRepository = attachmentRepository;
		this.accessFileCommand = accessFileCommand;
		this.filteringContext = filteringContext;
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

		// Phase 3 will add: Check if user is team manager for participating teams
		// Phase 4 will add: Check if user is on roster

		return managerTournamentIds
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
			// Already a manager, nothing to do
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

		this.logger.LogInformation("Removed manager {UserId} from tournament {TournamentId}", userId, tournamentId);

		return true;
	}

	private IQueryable<ITournamentContext> QueryTournamentsInternal(IQueryable<Models.Data.Tournament> tournaments, UserIdentifier userId)
	{
		// Get the user's unique ID for the Where clause
		var userUniqueId = userId.ToString();
		
		// Order and filter the tournaments BEFORE projection to allow EF Core to translate the query
		var filteredTournaments = tournaments
			// Sort by whether user is involved (tournaments where user is manager appear first)
			// This is computed as a subquery that EF Core can translate to SQL
			.OrderByDescending(t => t.TournamentManagers.Any(tm => tm.User.UniqueId == userUniqueId))
			// Then sort by start date (most recent first)
			.ThenByDescending(t => t.StartDate)
			// Filter private tournaments at the entity level BEFORE projection
			// Only show private tournaments where the user is involved (is a tournament manager)
			// Public tournaments are visible to everyone
			.Where(t => !t.IsPrivate || t.TournamentManagers.Any(tm => tm.User.UniqueId == userUniqueId));
		
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
				// User is involved if they manage this tournament
				// Phase 3 will extend: || user is a team manager for participating teams
				// Phase 4 will extend: || user is on a roster
				t.TournamentManagers.Any(tm => tm.User.UniqueId == userUniqueId)));
	}
}
