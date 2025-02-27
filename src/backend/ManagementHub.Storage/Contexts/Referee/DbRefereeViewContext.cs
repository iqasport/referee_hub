﻿namespace ManagementHub.Storage.Contexts.Referee;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Contexts.Team;
using ManagementHub.Storage.Contexts.User;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using DomainCertification = ManagementHub.Models.Domain.Tests.Certification;

public class DbRefereeViewContext : IRefereeViewContext
{
	/// <summary>
	/// User ID of the referee.
	/// </summary>
	public UserIdentifier UserId { get; set; }

	/// <summary>
	/// Name of the referee (or "Anonymous" if they disallow exporting).
	/// </summary>
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>
	/// Primary NGB this referee is located in.
	/// </summary>
	public NgbIdentifier? PrimaryNgb { get; set; }

	/// <summary>
	/// Secondary NGB this referee is located in.
	/// </summary>
	public NgbIdentifier? SecondaryNgb { get; set; }

	/// <summary>
	/// Team the referee is playing for.
	/// </summary>
	public TeamIdentifier? PlayingTeam { get; set; }

	/// <summary>
	/// Team the referee is coaching.
	/// </summary>
	public TeamIdentifier? CoachingTeam { get; set; }

	/// <summary>
	/// Certifications acquired by this referee.
	/// </summary>
	public required HashSet<DomainCertification> AcquiredCertifications { get; set; }

	public required List<ITeamContext> TeamContextList { get; set; }
	public IDictionary<TeamIdentifier, ITeamContext> TeamContext => this.TeamContextList.DistinctBy(t => t.TeamId).ToDictionary(t => t.TeamId);

	public required UserAttributes Attributes { get; set; }
}

public class DbRefereeViewContextFactory
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<DbRefereeViewContextFactory> logger;
	private readonly CollectionFilteringContext filteringContext;

	public DbRefereeViewContextFactory(
		ManagementHubDbContext dbContext,
		ILogger<DbRefereeViewContextFactory> logger,
		CollectionFilteringContext filteringContext)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.filteringContext = filteringContext;
	}

	private IQueryable<DbRefereeViewContext> QueryRefereesByNgb(IQueryable<User> users, NgbConstraint ngbs)
	{
		// select users who are referees
		IQueryable<User> referees = users
			.AsSplitQuery()
			.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.AccessType == UserAccessType.Referee))
			.Include(u => u.RefereeLocations).ThenInclude(rl => rl.NationalGoverningBody)
			.Include(u => u.RefereeTeams).ThenInclude(rl => rl.Team).ThenInclude(t => t!.NationalGoverningBody)
			.Include(u => u.RefereeCertifications).ThenInclude(rc => rc.Certification)
			.Include(u => u.Attributes.Where(ua => ngbs.AppliesToAny || ((IEnumerable<NgbIdentifier>)ngbs).Select(i => i.NgbCode).Contains(ua.Prefix)));

		if (!ngbs.AppliesToAny)
		{
			// if there's an NgbConstraint, select these referees who's set of locations intersects with the set of NGBs in the constraint
			referees = referees
				.Where(u => this.dbContext.NationalGoverningBodies.WithConstraint(ngbs).Select(ngb => ngb.Id).Intersect(u.RefereeLocations.Select(rl => rl.NationalGoverningBodyId)).Any());
		}

		// we do ordering here in order to get decent paging consistency
		referees = referees.OrderBy(u => u.FirstName);

		var filter = this.filteringContext.FilteringParameters.Filter;
		filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";
		if (!string.IsNullOrEmpty(filter))
		{
			if (this.dbContext.Database.IsNpgsql())
			{
				referees = referees.Where(u =>
					EF.Functions.ILike(u.FirstName!, filter) ||
					EF.Functions.ILike(u.LastName!, filter));
			}
			else
			{
				referees = referees.Where(u =>
					EF.Functions.Like(u.FirstName!, filter) ||
					EF.Functions.Like(u.LastName!, filter));
			}
		}

		if (this.filteringContext.FilteringMetadata != null)
		{
			//TODO: figure out some way to make this async in a nice way
			this.filteringContext.FilteringMetadata.TotalCount = referees.Count();
		}

		referees = referees.Page(this.filteringContext.FilteringParameters);

		return referees.Select(u => new DbRefereeViewContext
		{
			UserId = u.UniqueId != null ? UserIdentifier.Parse(u.UniqueId) : UserIdentifier.FromLegacyUserId(u.Id),
			DisplayName = u.ExportName != false || !string.IsNullOrWhiteSpace(u.FirstName + u.LastName) ? $"{u.FirstName} {u.LastName}" : "Anonymous referee",
			AcquiredCertifications = u.RefereeCertifications.Select(rc => DomainCertification.New(rc.Certification.Level, rc.Certification.Version)).ToHashSet(),
			CoachingTeam = u.RefereeTeams.Where(rt => rt.AssociationType == RefereeTeamAssociationType.Coach).Select(rt => new TeamIdentifier(rt.Team!.Id)).Cast<TeamIdentifier?>().FirstOrDefault(),
			PlayingTeam = u.RefereeTeams.Where(rt => rt.AssociationType == RefereeTeamAssociationType.Player).Select(rt => new TeamIdentifier(rt.Team!.Id)).Cast<TeamIdentifier?>().FirstOrDefault(),
			PrimaryNgb = u.RefereeLocations.Where(rt => rt.AssociationType == RefereeNgbAssociationType.Primary).Select(rt => NgbIdentifier.Parse(rt.NationalGoverningBody.CountryCode)).Cast<NgbIdentifier?>().FirstOrDefault(),
			SecondaryNgb = u.RefereeLocations.Where(rt => rt.AssociationType == RefereeNgbAssociationType.Secondary).Select(rt => NgbIdentifier.Parse(rt.NationalGoverningBody.CountryCode)).Cast<NgbIdentifier?>().FirstOrDefault(),
			TeamContextList = u.RefereeTeams.Select(rt => rt.Team!).Select(tt => new DbTeamContext(new TeamIdentifier(tt.Id), new NgbIdentifier(tt.NationalGoverningBody!.CountryCode), new TeamData
			{
				Name = tt.Name,
				City = tt.City,
				State = tt.State,
				Country = tt.Country,
				GroupAffiliation = tt.GroupAffiliation!.Value,
				Status = tt.Status!.Value,
				JoinedAt = tt.JoinedAt ?? new DateTime(),
			})).Cast<ITeamContext>().ToList(),
			Attributes = new UserAttributes(u.Attributes.Select(ua => new Models.Domain.User.UserAttribute(ua.Prefix, ua.Key, JsonDocument.Parse(ua.Attribute, DbUserContextFactory.UserAttributesParseOptions))).ToList())
		});
	}

	public async Task<DbRefereeViewContext> LoadAsync(UserIdentifier userId, NgbConstraint ngbs, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x42d8be00, "Loading referee profile for user ({userId}) with constraint [{ngbConstraint}]", userId, ngbs);
		var referee = await this.QueryRefereesByNgb(this.dbContext.Users.WithIdentifier(userId), ngbs)
			.SingleOrDefaultAsync(cancellationToken);

		if (referee == null)
		{
			throw new NotFoundException($"Could not find referee data for user ({userId}) under the constraint [{ngbs}].");
		}

		return referee;
	}

	public IQueryable<DbRefereeViewContext> QueryReferees(NgbConstraint ngbs)
	{
		this.logger.LogInformation(0x42d8be01, "Creating referee profile query with constraint [{ngbConstraint}]", ngbs);

		return this.QueryRefereesByNgb(this.dbContext.ActiveUsers(new SystemClock(), CertificationVersion.TwentyFour), ngbs);
	}
}
