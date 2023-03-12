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
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts;

public record class DbUserContext(UserIdentifier UserId, UserData UserData, IEnumerable<IUserRole> Roles) : IUserContext
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
		this.logger.LogInformation(0, "Loading user context for user ({userId}).", userId);
		// TODO: optimize it later into a database level view
		var userData = await this.users.WithIdentifier(userId)
			// THIS LEFT JOIN IN PURE LINQ
			.GroupJoin(this.languages, u => u.LanguageId, l => l.Id, (u, l) => new { User = u, Languages = l })
			.SelectMany(join => join.Languages.DefaultIfEmpty(), (join, l) => new { join.User, Language = l})
			// UNTIL HERE
			.Select(join => new UserData(new Email(join.User.Email), join.User.FirstName ?? string.Empty, join.User.LastName ?? string.Empty)
			{
				UserLang = join.Language != null ? new LanguageIdentifier(join.Language.ShortName, join.Language.ShortRegion) : LanguageIdentifier.Default,
			})
			.SingleAsync(cancellationToken);
		var dbRoles = await this.roles.Where(role => role.UserId == userId.Id).ToListAsync(cancellationToken);

		var roles = new List<IUserRole>(16); // temporary constant - should be modified if the number of roles increases
		foreach (var dbRole in dbRoles)
		{
			roles.AddRange(await this.ConvertFromDbRoleAsync(dbRole, cancellationToken));
		}

		this.logger.LogInformation(0, "Returning user context with roles: {roles}.", string.Join(", ", roles));

		return new DbUserContext(userId, userData, roles);
	}

	// TODO: (before db integration) move this out into separate role providers? so that it's more testable
	private async Task<IEnumerable<IUserRole>> ConvertFromDbRoleAsync(Role role, CancellationToken cancellationToken)
	{
		switch (role.AccessType)
		{
			case UserAccessType.Referee:
				{
					var teams = await this.refereeTeams
						.Where(team => team.RefereeId == role.UserId)
						.Select(team => new RefereeTeam { AssociationType = team.AssociationType, TeamId = team.TeamId })
						.ToListAsync(cancellationToken);
					var ngbs = await this.refereeLocations
						.Where(location => location.RefereeId == role.UserId)
						.Select(location => new RefereeLocation { AssociationType = location.AssociationType, NationalGoverningBodyId = location.NationalGoverningBodyId })
						.ToListAsync(cancellationToken);

					var playingTeam = teams.FirstOrDefault(team => team.AssociationType == RefereeTeamAssociationType.Player);
					var coachingTeam = teams.FirstOrDefault(team => team.AssociationType == RefereeTeamAssociationType.Coach);
					var primaryNgb = ngbs.FirstOrDefault(ngb => ngb.AssociationType == RefereeNgbAssociationType.Primary);
					var secondaryNgb = ngbs.FirstOrDefault(ngb => ngb.AssociationType == RefereeNgbAssociationType.Secondary);

					return new IUserRole[] {
						new RefereeRole
						{
							IsActive = true,
							PlayingTeam = playingTeam is not null ? new TeamIdentifier(playingTeam.TeamId ?? throw new Exception("I don't know why this is nullable")) : null,
							CoachingTeam = coachingTeam is not null ? new TeamIdentifier(coachingTeam.TeamId ?? throw new Exception("I don't know why this is nullable")) : null,
							PrimaryNgb = primaryNgb is not null ? new NgbIdentifier(primaryNgb.NationalGoverningBodyId) : null,
							SecondaryNgb = secondaryNgb is not null ? new NgbIdentifier(secondaryNgb.NationalGoverningBodyId) : null,
						},
					};
				}
			case UserAccessType.NgbAdmin:
				{
					var ngbConstraint = new NgbConstraint(await this.nationalGoverningBodyAdmins
						.Where(ngbAdmin => ngbAdmin.UserId == role.UserId)
						.Select(ngbAdmin => new NgbIdentifier(ngbAdmin.NationalGoverningBodyId))
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
