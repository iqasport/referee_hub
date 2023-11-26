using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.User;
using User = ManagementHub.Models.Data.User;

public record class DbUserContext(UserIdentifier UserId, UserData UserData, IEnumerable<IUserRole> Roles, IDictionary<string, JsonDocument> Attributes) : IUserContext
{
}

public class DbUserContextFactory
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<Role> roles;
	private readonly IQueryable<NationalGoverningBodyAdmin> nationalGoverningBodyAdmins;
	private readonly IQueryable<RefereeTeam> refereeTeams;
	private readonly IQueryable<RefereeLocation> refereeLocations;
	private readonly IQueryable<Language> languages;
	private readonly ILogger<DbUserContextFactory> logger;

	public DbUserContextFactory(
		IQueryable<User> users,
		IQueryable<Role> roles,
		IQueryable<NationalGoverningBodyAdmin> nationalGoverningBodyAdmins,
		IQueryable<RefereeTeam> refereeTeams,
		IQueryable<RefereeLocation> refereeLocations,
		IQueryable<Language> languages,
		ILogger<DbUserContextFactory> logger)
	{
		this.users = users;
		this.roles = roles;
		this.nationalGoverningBodyAdmins = nationalGoverningBodyAdmins;
		this.refereeTeams = refereeTeams;
		this.refereeLocations = refereeLocations;
		this.languages = languages;
		this.logger = logger;
	}

	public async Task<DbUserContext> LoadAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(-0x58e19300, "Loading user context for user ({userId}).", userId);
		// TODO: optimize it later into a database level view
		var userData = await this.users.AsNoTracking().WithIdentifier(userId)
			.Include(u => u.Language)
			.Select(user => new UserData(new Email(user.Email), user.FirstName ?? string.Empty, user.LastName ?? string.Empty)
			{
				UserLang = user.Language != null ? new LanguageIdentifier(user.Language.ShortName, user.Language.ShortRegion) : LanguageIdentifier.Default,
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (userData == null)
		{
			throw new NotFoundException(userId.ToString());
		}

		var dbRoles = await this.users.WithIdentifier(userId)
			.Join(this.roles, u => u.Id, r => r.UserId, (_, r) => r)
			.ToListAsync(cancellationToken);

		var roles = new List<IUserRole>(16); // temporary constant - should be modified if the number of roles increases
		foreach (var dbRole in dbRoles)
		{
			roles.AddRange(await this.ConvertFromDbRoleAsync(userId, dbRole, cancellationToken));
		}

		var attributes = await this.users.WithIdentifier(userId)
			.Include(u => u.Attributes)
			.Select(u => u.Attributes.ToDictionary(ua => ua.Key, ua => JsonDocument.Parse(ua.Attribute, new JsonDocumentOptions())))
			.SingleAsync(cancellationToken);

		this.logger.LogInformation(-0x58e192ff, "Returning user context with roles: {roles}.", string.Join(", ", roles));

		return new DbUserContext(userId, userData, roles, attributes);
	}

	// TODO: (before db integration) move this out into separate role providers? so that it's more testable
	private async Task<IEnumerable<IUserRole>> ConvertFromDbRoleAsync(UserIdentifier userId, Role role, CancellationToken cancellationToken)
	{
		switch (role.AccessType)
		{
			case UserAccessType.Referee:
				{
					var properties = await this.users.WithIdentifier(userId)
						.Include(u => u.RefereeTeams).ThenInclude(rt => rt.Team)
						.Include(u => u.RefereeLocations).ThenInclude(rl => rl.NationalGoverningBody)
						.Select(u => new
						{
							RefereeTeams = u.RefereeTeams.Select(rt => new { rt.AssociationType, rt.TeamId }),
							RefereeLocations = u.RefereeLocations.Select(rt => new { rt.AssociationType, NgbId = rt.NationalGoverningBody.CountryCode }),
						})
						.SingleAsync(cancellationToken);

					var playingTeam = properties.RefereeTeams.FirstOrDefault(team => team.AssociationType == RefereeTeamAssociationType.Player);
					var coachingTeam = properties.RefereeTeams.FirstOrDefault(team => team.AssociationType == RefereeTeamAssociationType.Coach);
					var primaryNgb = properties.RefereeLocations.FirstOrDefault(ngb => ngb.AssociationType == RefereeNgbAssociationType.Primary);
					var secondaryNgb = properties.RefereeLocations.FirstOrDefault(ngb => ngb.AssociationType == RefereeNgbAssociationType.Secondary);

					return new IUserRole[] {
						new RefereeRole
						{
							IsActive = true,
							PlayingTeam = playingTeam is not null ? new TeamIdentifier(playingTeam.TeamId ?? throw new Exception("I don't know why this is nullable")) : null,
							CoachingTeam = coachingTeam is not null ? new TeamIdentifier(coachingTeam.TeamId ?? throw new Exception("I don't know why this is nullable")) : null,
							PrimaryNgb = primaryNgb is not null ? NgbIdentifier.Parse(primaryNgb.NgbId) : null,
							SecondaryNgb = secondaryNgb is not null ? NgbIdentifier.Parse(secondaryNgb.NgbId) : null,
						},
					};
				}
			case UserAccessType.NgbAdmin:
				{
					var ngbConstraint = NgbConstraint.Set(await this.users.WithIdentifier(userId)
						.Include(u => u.NationalGoverningBodyAdmin).ThenInclude(a => a.NationalGoverningBody)
						.Select(u => NgbIdentifier.Parse(u.NationalGoverningBodyAdmin.NationalGoverningBody.CountryCode))
						.ToListAsync(cancellationToken));

					return new IUserRole[]
					{
						new NgbAdminRole(ngbConstraint),
						new NgbStatsManagerRole(ngbConstraint),
						new NgbStatsViewerRole(ngbConstraint),
						new NgbUserAdminRole(ngbConstraint),
						new RefereeManagerRole(ngbConstraint),
						new RefereeViewerRole(ngbConstraint),
					};
				}
			case UserAccessType.IqaAdmin:
				{
					var ngbConstraint = NgbConstraint.Any;

					return new IUserRole[]
					{
						new NgbAdminRole(ngbConstraint),
						new NgbStatsManagerRole(ngbConstraint),
						new NgbStatsViewerRole(ngbConstraint),
						new NgbUserAdminRole(ngbConstraint),
						new RefereeManagerRole(ngbConstraint),
						new RefereeViewerRole(ngbConstraint),
						new IqaAdminRole(),
						new RefereeAdminRole(),
						new TechAdminRole(),
						new TestAdminRole(),
					};
				}
		}

		return Array.Empty<IUserRole>();
	}
}
