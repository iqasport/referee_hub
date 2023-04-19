namespace ManagementHub.Storage.Contexts.Referee;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
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
}

public class DbRefereeViewContextFactory
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<Role> roles;
	private readonly IQueryable<RefereeCertification> certifications;
	private readonly IQueryable<RefereeLocation> locations;
	private readonly IQueryable<RefereeTeam> teams;
	private readonly IQueryable<NationalGoverningBody> nationalGoverningBody;
	private readonly ILogger<DbRefereeViewContextFactory> logger;

	public DbRefereeViewContextFactory(
		IQueryable<User> users,
		IQueryable<Role> roles,
		IQueryable<RefereeCertification> certifications,
		IQueryable<RefereeLocation> locations, 
		IQueryable<RefereeTeam> teams, 
		IQueryable<NationalGoverningBody> nationalGoverningBody, 
		ILogger<DbRefereeViewContextFactory> logger)
	{
		this.users = users;
		this.roles = roles;
		this.certifications = certifications;
		this.locations = locations;
		this.teams = teams;
		this.nationalGoverningBody = nationalGoverningBody;
		this.logger = logger;
	}

	private IQueryable<DbRefereeViewContext> QueryRefereesByNgb(IQueryable<User> users, NgbConstraint ngbs)
	{
		// select users who are referees
		var referees = users.Join(this.roles.Where(r => r.AccessType == UserAccessType.Referee), u => u.Id, r => r.UserId, (u, _) => u)
			.GroupJoin(this.locations, u => u.Id, l => l.RefereeId, (u, l) => new { User = u, Locations = l })
			.SelectMany(g => g.Locations.DefaultIfEmpty(), (g, location) => new { g.User, Location = location })
			.GroupJoin(this.teams, g => g.User.Id, t => t.RefereeId, (g, t) => new
			{
				g.User,
				g.Location,
				Teams = t,
			})
			.SelectMany(g => g.Teams.DefaultIfEmpty(), (g, team) => new
			{
				g.User,
				g.Location,
				Team = team,
			})
			.GroupJoin(this.certifications.Include(c => c.Certification), g => g.User.Id, c => c.RefereeId, (g, c) => new
			{
				g.User,
				g.Location,
				g.Team,
				Certifications = c,
			})
			.SelectMany(g => g.Certifications.DefaultIfEmpty(), (g, certification) => new
			{
				g.User,
				g.Location,
				g.Team,
				Certification = certification
			})
			.GroupBy(g => new { g.User.Id, g.User.UniqueId, g.User.ExportName, g.User.FirstName, g.User.LastName }, (u, g) => new
			{
				User = u,
				PrimaryLocationId = g.Where(gx => gx.Location != null && gx.Location.AssociationType == RefereeNgbAssociationType.Primary).Select(gx => gx.Location.NationalGoverningBodyId).SingleOrDefault(),
				SecondaryLocationId = g.Where(gx => gx.Location != null && gx.Location.AssociationType == RefereeNgbAssociationType.Secondary).Select(gx => gx.Location.NationalGoverningBodyId).SingleOrDefault(),
				CoachingTeamId = g.Where(gx => gx.Team != null && gx.Team.AssociationType == RefereeTeamAssociationType.Coach).Select(gx => gx.Team.TeamId).SingleOrDefault(),
				PlayingTeamId = g.Where(gx => gx.Team != null && gx.Team.AssociationType == RefereeTeamAssociationType.Player).Select(gx => gx.Team.TeamId).SingleOrDefault(),
				Certifications = g.Where(gx => gx.Certification != null).Select(gx => new DomainCertification(gx.Certification.Certification.Level, gx.Certification.Certification.Version))
			});

		if (!ngbs.AppliesToAny)
		{
			// if there's an NgbConstraint, select these referees who's set of locations intersects with the set of NGBs in the constraint
			referees = referees
				.Where(g => this.nationalGoverningBody.WithConstraint(ngbs).Select(ngb => ngb.Id).Contains(g.PrimaryLocationId) || this.nationalGoverningBody.WithConstraint(ngbs).Select(ngb => ngb.Id).Contains(g.SecondaryLocationId));
		}

		return referees.Select(g => new DbRefereeViewContext
		{
			UserId = g.User.UniqueId != null ? UserIdentifier.Parse(g.User.UniqueId) : UserIdentifier.FromLegacyUserId(g.User.Id),
			DisplayName = g.User.ExportName != false ? $"{g.User.FirstName} {g.User.LastName}" : "Anonymous referee",
			AcquiredCertifications = g.Certifications.ToHashSet(),
			CoachingTeam = g.CoachingTeamId != null ? new TeamIdentifier(g.CoachingTeamId.Value) : null,
			PlayingTeam = g.PlayingTeamId != null ? new TeamIdentifier(g.PlayingTeamId.Value) : null,
			PrimaryNgb = g.PrimaryLocationId != default ? new NgbIdentifier(g.PrimaryLocationId) : null,
			SecondaryNgb = g.SecondaryLocationId != default ? new NgbIdentifier(g.SecondaryLocationId) : null,
		});
	}

	public async Task<DbRefereeViewContext> LoadAsync(UserIdentifier userId, NgbConstraint ngbs, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Loading referee profile for user ({userId}) with constraint [{ngbConstraint}]", userId, ngbs);
		var referee = await this.QueryRefereesByNgb(this.users.WithIdentifier(userId), ngbs)
			.SingleOrDefaultAsync(cancellationToken);

		if (referee == null)
		{
			throw new NotFoundException($"Could not find referee data for user ({userId}) under the constraint [{ngbs}].");
		}

		return referee;
	}

	public IQueryable<DbRefereeViewContext> QueryReferees(NgbConstraint ngbs)
	{
		this.logger.LogInformation(0, "Creating referee profile query with constraint [{ngbConstraint}]", ngbs);

		return this.QueryRefereesByNgb(this.users, ngbs);
	}
}
