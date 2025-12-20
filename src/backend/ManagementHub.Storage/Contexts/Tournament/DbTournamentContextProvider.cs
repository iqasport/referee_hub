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
	bool IsPrivate) : ITournamentContext;

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

	public IQueryable<ITournamentContext> QueryTournaments(bool includePrivate = false)
	{
		var filter = this.filteringContext.FilteringParameters.Filter;
		filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";

		IQueryable<Models.Data.Tournament> filteredTournaments;
		if (this.dbContext.Database.IsNpgsql())
		{
			filteredTournaments = string.IsNullOrEmpty(filter)
				? this.dbContext.Tournaments
				: this.dbContext.Tournaments
					.Where(t => EF.Functions.ILike(t.Name, filter) || EF.Functions.ILike(t.Description, filter));
		}
		else
		{
			filteredTournaments = string.IsNullOrEmpty(filter)
				? this.dbContext.Tournaments
				: this.dbContext.Tournaments
					.Where(t => EF.Functions.Like(t.Name, filter) || EF.Functions.Like(t.Description, filter));
		}

		// Filter out private tournaments unless explicitly requested
		if (!includePrivate)
		{
			filteredTournaments = filteredTournaments.Where(t => !t.IsPrivate);
		}

		if (this.filteringContext.FilteringMetadata != null)
		{
			this.filteringContext.FilteringMetadata.TotalCount = filteredTournaments.Count();
		}

		return this.QueryTournamentsInternal(filteredTournaments.Page(this.filteringContext.FilteringParameters));
	}

	public async Task<ITournamentContext> GetTournamentContextAsync(TournamentIdentifier tournamentId, CancellationToken cancellationToken = default)
	{
		var tournament = await this.QueryTournamentsInternal(this.dbContext.Tournaments.Where(t => t.UniqueId == tournamentId.ToString()))
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

	private IQueryable<ITournamentContext> QueryTournamentsInternal(IQueryable<Models.Data.Tournament> tournaments)
	{
		return tournaments.AsNoTracking()
			.OrderByDescending(t => t.StartDate)
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
				t.IsPrivate));
	}
}
