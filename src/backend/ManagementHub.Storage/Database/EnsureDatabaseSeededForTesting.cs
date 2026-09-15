using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

public class EnsureDatabaseSeededForTesting : DatabaseStartupService
{
	private readonly int additionalSeedReferees;
	private readonly int additionalSeedTeamInvites;
	private readonly int additionalSeedTransfers;

	public EnsureDatabaseSeededForTesting(IServiceProvider serviceProvider, ILogger<EnsureDatabaseSeededForTesting> logger, IConfiguration configuration) : base(serviceProvider, logger)
	{
		this.additionalSeedReferees = this.GetNonNegativeInt(configuration, "Services:AdditionalSeedReferees", 0);
		this.additionalSeedTeamInvites = this.GetNonNegativeInt(configuration, "Services:AdditionalSeedTeamInvites", 0);
		this.additionalSeedTransfers = this.GetNonNegativeInt(configuration, "Services:AdditionalSeedTransfers", 0);
	}

	protected override Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
		try
		{
			// Use synchronous operations to ensure seeding completes before host starts
			// This is important for testing and dev environments
			var ngbCount = dbContext.NationalGoverningBodies.Count();
			if (ngbCount > 0)
			{
				this.logger.LogInformation(-0x48302e00, "Database not empty. Skipping seeding.");
				return Task.CompletedTask;
			}

			this.logger.LogInformation(-0x48302dff, "Ensuring database is seeded...");

			this.SeedDatabase(dbContext);

			this.logger.LogInformation(-0x48302dfe, "Ensuring database is seeded completed.");
			return Task.CompletedTask;
		}
		catch (Exception ex)
		{
			this.logger.LogError(-0x48302dfd, ex, "Error while seeding database.");
			throw;
		}
	}

	private void SeedDatabase(ManagementHubDbContext dbContext)
	{
		var ngbs = this.SeedNgbs(dbContext);
		var teams = this.SeedTeams(dbContext, ngbs);
		this.SeedTournaments(dbContext);
		var certifications = this.EnsureCertifications(dbContext);
		var languages = this.SeedLanguages(dbContext);
		var users = this.SeedUsersAndRoles(dbContext);
		this.SeedUserAssociations(dbContext, ngbs, teams, users);
		var tests = this.SeedTests(dbContext, certifications, languages);
		this.SeedTestHistoryAndCertifications(dbContext, tests, certifications, users.Referee, users.RecertTestReferee);
		this.SeedQuestions(dbContext, tests);
		this.SeedAdditionalDevelopmentData(dbContext, ngbs, teams, users);

		dbContext.SaveChanges();
	}

	private int GetNonNegativeInt(IConfiguration configuration, string key, int fallback)
	{
		var raw = configuration[key];
		if (string.IsNullOrWhiteSpace(raw))
		{
			return fallback;
		}

		if (int.TryParse(raw, out var parsed) && parsed >= 0)
		{
			return parsed;
		}

		this.logger.LogWarning("Invalid numeric configuration for {ConfigKey}: {ConfigValue}. Falling back to {FallbackValue}", key, raw, fallback);
		return fallback;
	}

	private void SeedAdditionalDevelopmentData(
		ManagementHubDbContext dbContext,
		IReadOnlyList<NationalGoverningBody> ngbs,
		IReadOnlyList<Team> teams,
		SeedUsers users)
	{
		if (this.additionalSeedReferees == 0 && this.additionalSeedTeamInvites == 0 && this.additionalSeedTransfers == 0)
		{
			return;
		}

		this.logger.LogInformation(
			"Seeding additional dev dataset: {RefereeCount} referees, {InviteCount} team invites, {TransferCount} transfers.",
			this.additionalSeedReferees,
			this.additionalSeedTeamInvites,
			this.additionalSeedTransfers);

		var now = DateTime.UtcNow;
		var random = new Random(421337);
		const string passwordHash = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne";

		var extraReferees = this.CreateExtraReferees(now, passwordHash);
		this.SeedExtraRefereeAssociations(dbContext, ngbs, teams, extraReferees, now);

		var inviteTargets = this.BuildInviteTargets(extraReferees, users);
		var seedData = new AdditionalSeedData(this.additionalSeedTeamInvites, this.additionalSeedTransfers);

		this.SeedAdditionalTeamInvites(dbContext, teams, users, inviteTargets, now, seedData);
		this.SeedAdditionalTransfers(teams, users, inviteTargets, now, random, seedData);

		dbContext.TeamInvitations.AddRange(seedData.TeamInvitations);
		dbContext.TeamPlayerActivities.AddRange(seedData.TeamPlayerActivities);
		dbContext.NgbTransferApprovals.AddRange(seedData.TransferApprovals);
	}

	private List<User> CreateExtraReferees(DateTime now, string passwordHash)
	{
		var extraReferees = new List<User>(this.additionalSeedReferees);
		for (var i = 1; i <= this.additionalSeedReferees; i++)
		{
			extraReferees.Add(new User
			{
				CreatedAt = now.AddMinutes(-(i + 10)),
				Email = $"dev.referee.{i:D4}@example.test",
				EncryptedPassword = passwordHash,
				FirstName = "Dev",
				LastName = $"Referee{i:D4}",
			});
		}

		return extraReferees;
	}

	private void SeedExtraRefereeAssociations(
		ManagementHubDbContext dbContext,
		IReadOnlyList<NationalGoverningBody> ngbs,
		IReadOnlyList<Team> teams,
		IReadOnlyList<User> extraReferees,
		DateTime now)
	{
		dbContext.Users.AddRange(extraReferees);
		dbContext.Roles.AddRange(extraReferees.Select(referee => new Role
		{
			AccessType = UserAccessType.Referee,
			User = referee,
			CreatedAt = now,
		}));

		dbContext.RefereeLocations.AddRange(extraReferees.Select((referee, index) => new RefereeLocation
		{
			Referee = referee,
			AssociationType = RefereeNgbAssociationType.Primary,
			NationalGoverningBody = ngbs[index % ngbs.Count],
			CreatedAt = now.AddDays(-(index % 90)),
			UpdatedAt = now,
		}));

		var playerAssociations = new List<RefereeTeam>(extraReferees.Count);
		for (var index = 0; index < extraReferees.Count; index++)
		{
			if (index % 3 == 0)
			{
				continue;
			}

			playerAssociations.Add(new RefereeTeam
			{
				Referee = extraReferees[index],
				AssociationType = RefereeTeamAssociationType.Player,
				Team = teams[index % teams.Count],
				CreatedAt = now.AddDays(-(index % 120)),
				UpdatedAt = now,
			});
		}

		dbContext.RefereeTeams.AddRange(playerAssociations);
	}

	private IReadOnlyList<User> BuildInviteTargets(IReadOnlyList<User> extraReferees, SeedUsers users)
	{
		return extraReferees.Count > 0
			? extraReferees
			: new List<User> { users.Referee, users.PlayerSarah, users.CoachMike, users.RecertTestReferee };
	}

	private void SeedAdditionalTeamInvites(
		ManagementHubDbContext dbContext,
		IReadOnlyList<Team> teams,
		SeedUsers users,
		IReadOnlyList<User> inviteTargets,
		DateTime now,
		AdditionalSeedData seedData)
	{
		for (var i = 1; i <= this.additionalSeedTeamInvites; i++)
		{
			var targetUser = inviteTargets[(i - 1) % inviteTargets.Count];
			var destinationTeam = teams[(i - 1) % teams.Count];
			var createdAt = now.AddHours(-i);

			var invitation = new TeamInvitation
			{
				Team = destinationTeam,
				Email = targetUser.Email,
				Initiator = users.TeamManager,
				CreatedAt = createdAt,
			};

			var resultActivityType = this.ApplyInviteOutcomeAndMembership(dbContext, invitation, targetUser, destinationTeam, createdAt, i);
			seedData.TeamInvitations.Add(invitation);
			seedData.TeamPlayerActivities.Add(BuildInviteCreatedActivity(destinationTeam, targetUser, users.TeamManager, createdAt));

			if (resultActivityType != TeamPlayerActivityType.InviteCreated)
			{
				seedData.TeamPlayerActivities.Add(new TeamPlayerActivity
				{
					Team = destinationTeam,
					User = targetUser,
					Email = targetUser.Email,
					Initiator = users.TeamManager,
					ActivityType = resultActivityType,
					CreatedAt = createdAt.AddMinutes(45),
				});
			}
		}
	}

	private TeamPlayerActivityType ApplyInviteOutcomeAndMembership(
		ManagementHubDbContext dbContext,
		TeamInvitation invitation,
		User targetUser,
		Team destinationTeam,
		DateTime createdAt,
		int index)
	{
		if (index % 6 == 0)
		{
			invitation.RevokedAt = createdAt.AddMinutes(30);
			return TeamPlayerActivityType.InviteRevoked;
		}

		if (index % 5 == 0)
		{
			invitation.DeclinedAt = createdAt.AddMinutes(40);
			invitation.RespondedByUser = targetUser;
			return TeamPlayerActivityType.InviteDeclined;
		}

		if (index % 4 != 0)
		{
			return TeamPlayerActivityType.InviteCreated;
		}

		invitation.AcceptedAt = createdAt.AddMinutes(20);
		invitation.RespondedByUser = targetUser;

		var acceptedAt = invitation.AcceptedAt.Value;

		// Check for existing player membership first in the change tracker, then the database.
		RefereeTeam? existingMembership = dbContext.ChangeTracker.Entries<RefereeTeam>()
			.Select(e => e.Entity)
			.FirstOrDefault(rt =>
				((rt.Referee != null && rt.Referee == targetUser) || (rt.RefereeId.HasValue && rt.RefereeId.Value == targetUser.Id))
				&& rt.AssociationType == RefereeTeamAssociationType.Player);

		if (existingMembership == null)
		{
			existingMembership = dbContext.RefereeTeams
				.FirstOrDefault(rt => rt.RefereeId == targetUser.Id && rt.AssociationType == RefereeTeamAssociationType.Player);
		}

		if (existingMembership != null)
		{
			// Update existing membership to the new team instead of inserting a duplicate.
			existingMembership.Team = destinationTeam;
			existingMembership.UpdatedAt = acceptedAt;
		}
		else
		{
			dbContext.RefereeTeams.Add(new RefereeTeam
			{
				Referee = targetUser,
				AssociationType = RefereeTeamAssociationType.Player,
				Team = destinationTeam,
				CreatedAt = acceptedAt,
				UpdatedAt = acceptedAt,
			});
		}

		return TeamPlayerActivityType.InviteAccepted;
	}

	private void SeedAdditionalTransfers(
		IReadOnlyList<Team> teams,
		SeedUsers users,
		IReadOnlyList<User> inviteTargets,
		DateTime now,
		Random random,
		AdditionalSeedData seedData)
	{
		var eligibleTransferTeams = teams
			.Where(team =>
				team.GroupAffiliation != TeamGroupAffiliation.National
				&& team.GroupAffiliation != TeamGroupAffiliation.NotApplicable)
			.ToArray();

		if (this.additionalSeedTransfers > 0 && eligibleTransferTeams.Length < 2)
		{
			this.logger.LogWarning(
				"Skipping additional transfer seed data because fewer than 2 eligible playing teams are available (found {EligibleTeamCount}).",
				eligibleTransferTeams.Length);
		}

		var transferSeedCount = eligibleTransferTeams.Length >= 2 ? this.additionalSeedTransfers : 0;
		for (var i = 1; i <= transferSeedCount; i++)
		{
			var originTeam = eligibleTransferTeams[(i + 1) % eligibleTransferTeams.Length];
			var targetUser = inviteTargets[(i * 7) % inviteTargets.Count];
			var destinationTeam = Enumerable.Range(0, eligibleTransferTeams.Length)
				.Select(offset => eligibleTransferTeams[(i + 2 + offset) % eligibleTransferTeams.Length])
				.FirstOrDefault(team =>
					!ReferenceEquals(team, originTeam)
					&& !HasPendingInvite(seedData.TeamInvitations, team, targetUser.Email));

			if (destinationTeam == null)
			{
				this.logger.LogWarning(
					"Skipping additional transfer seed data because no unused destination is available for invite {InviteIndex}.",
					i);
				continue;
			}

			var createdAt = now.AddHours(-(this.additionalSeedTeamInvites + i));
			var originNgb = originTeam.NationalGoverningBody;
			var destinationNgb = destinationTeam.NationalGoverningBody;
			if (originNgb == null || destinationNgb == null)
			{
				continue;
			}

			var transferInvite = new TeamInvitation
			{
				Team = destinationTeam,
				Email = targetUser.Email,
				Initiator = users.TeamManager,
				CreatedAt = createdAt,
				OriginTeam = originTeam,
				IsInternalTransfer = ReferenceEquals(originNgb, destinationNgb),
			};

			if (i % 9 == 0)
			{
				transferInvite.RevokedAt = createdAt.AddMinutes(50);
			}

			seedData.TeamInvitations.Add(transferInvite);
			seedData.TeamPlayerActivities.Add(BuildInviteCreatedActivity(destinationTeam, targetUser, users.TeamManager, createdAt));

			if (transferInvite.RevokedAt != null)
			{
				seedData.TeamPlayerActivities.Add(new TeamPlayerActivity
				{
					Team = destinationTeam,
					User = targetUser,
					Email = targetUser.Email,
					Initiator = users.TeamManager,
					ActivityType = TeamPlayerActivityType.InviteRevoked,
					CreatedAt = transferInvite.RevokedAt.Value,
				});
			}

			this.AddTransferApprovalsForInvite(seedData.TransferApprovals, transferInvite, originNgb, destinationNgb, createdAt, random, users, i);
		}
	}

	private static bool HasPendingInvite(IEnumerable<TeamInvitation> invitations, Team team, string email)
	{
		return invitations.Any(invitation =>
			ReferenceEquals(invitation.Team, team)
			&& invitation.Email == email
			&& invitation.RevokedAt == null
			&& invitation.AcceptedAt == null
			&& invitation.DeclinedAt == null);
	}

	private static TeamPlayerActivity BuildInviteCreatedActivity(Team destinationTeam, User targetUser, User initiator, DateTime createdAt)
	{
		return new TeamPlayerActivity
		{
			Team = destinationTeam,
			User = targetUser,
			Email = targetUser.Email,
			Initiator = initiator,
			ActivityType = TeamPlayerActivityType.InviteCreated,
			CreatedAt = createdAt,
		};
	}

	private void AddTransferApprovalsForInvite(
		ICollection<NgbTransferApproval> transferApprovals,
		TeamInvitation transferInvite,
		NationalGoverningBody originNgb,
		NationalGoverningBody destinationNgb,
		DateTime createdAt,
		Random random,
		SeedUsers users,
		int index)
	{
		if (ReferenceEquals(originNgb, destinationNgb))
		{
			transferApprovals.Add(new NgbTransferApproval
			{
				TeamInvitation = transferInvite,
				Ngb = originNgb,
				IsOriginNgb = true,
				CreatedAt = createdAt,
				ApprovedAt = index % 3 == 0 ? createdAt.AddMinutes(15) : null,
				ReviewedByUser = index % 3 == 0 ? users.NgbAdmin : null,
			});
			return;
		}

		var originApproval = new NgbTransferApproval
		{
			TeamInvitation = transferInvite,
			Ngb = originNgb,
			IsOriginNgb = true,
			CreatedAt = createdAt,
		};

		var destinationApproval = new NgbTransferApproval
		{
			TeamInvitation = transferInvite,
			Ngb = destinationNgb,
			IsOriginNgb = false,
			CreatedAt = createdAt,
		};

		if (index % 7 == 0)
		{
			originApproval.RejectedAt = createdAt.AddMinutes(20 + random.Next(10));
			originApproval.ReviewedByUser = users.NgbAdmin;
		}
		else
		{
			originApproval.ApprovedAt = createdAt.AddMinutes(10 + random.Next(20));
			originApproval.ReviewedByUser = users.NgbAdmin;

			if (index % 2 == 0)
			{
				destinationApproval.ApprovedAt = createdAt.AddMinutes(40 + random.Next(20));
				destinationApproval.ReviewedByUser = users.NgbAdmin;
			}
		}

		transferApprovals.Add(originApproval);
		transferApprovals.Add(destinationApproval);
	}

	private sealed class AdditionalSeedData
	{
		public AdditionalSeedData(int additionalSeedTeamInvites, int additionalSeedTransfers)
		{
			this.TeamInvitations = new List<TeamInvitation>(additionalSeedTeamInvites + additionalSeedTransfers);
			this.TeamPlayerActivities = new List<TeamPlayerActivity>(additionalSeedTeamInvites + additionalSeedTransfers);
			this.TransferApprovals = new List<NgbTransferApproval>(additionalSeedTransfers * 2);
		}

		public List<TeamInvitation> TeamInvitations { get; }
		public List<TeamPlayerActivity> TeamPlayerActivities { get; }
		public List<NgbTransferApproval> TransferApprovals { get; }
	}

	private NationalGoverningBody[] SeedNgbs(ManagementHubDbContext dbContext)
	{
		var ngbs = new[]
		{
			new NationalGoverningBody
			{
				CountryCode = "ARG",
				Name = "Asociación de Quidditch Argentina",
				Country = "Argentina",
				Region = NgbRegion.SouthAmerica,
				MembershipStatus = NgbMembershipStatus.Full,
				PlayerCount = 75,
				Website = "https://www.facebook.com/asociaciondequidditch.arg/",
				CreatedAt = DateTime.UtcNow,
			},
			new NationalGoverningBody
			{
				CountryCode = "AUS",
				Name = "Quidditch Australia",
				Country = "Australia",
				Region = NgbRegion.Asia,
				MembershipStatus = NgbMembershipStatus.Full,
				PlayerCount = 700,
				Website = "https://www.quidditch.info/",
				CreatedAt = DateTime.UtcNow,
			},
			new NationalGoverningBody
			{
				CountryCode = "BRA",
				Name = "Associação Brasileira de Quadball",
				Country = "Brazil",
				Region = NgbRegion.SouthAmerica,
				MembershipStatus = NgbMembershipStatus.Developing,
				PlayerCount = 319,
				Website = "https://abrquadribol.wordpress.com/",
				CreatedAt = DateTime.UtcNow,
			},
			new NationalGoverningBody
			{
				CountryCode = "POL",
				Name = "Polska Liga Quidditcha",
				Country = "Poland",
				Region = NgbRegion.Europe,
				MembershipStatus = NgbMembershipStatus.Full,
				PlayerCount = 110,
				Website = "https://polskaligaquidditcha.pl/",
				CreatedAt = DateTime.UtcNow,
			},
			new NationalGoverningBody
			{
				CountryCode = "USA",
				Name = "US Quadball",
				Country = "United States",
				Region = NgbRegion.NorthAmerica,
				MembershipStatus = NgbMembershipStatus.Full,
				PlayerCount = 1681,
				Website = "https://www.usquadball.org/",
				CreatedAt = DateTime.UtcNow,
			},
			new NationalGoverningBody
			{
				CountryCode = "DEU",
				Name = "QBund",
				Country = "Germany",
				Region = NgbRegion.Europe,
				MembershipStatus = NgbMembershipStatus.Full,
				PlayerCount = 600,
				Website = "https://www.usquadball.org/",
				CreatedAt = DateTime.UtcNow,
			},
		};

		dbContext.NationalGoverningBodies.AddRange(ngbs);
		return ngbs;
	}

	private List<Team> SeedTeams(ManagementHubDbContext dbContext, NationalGoverningBody[] ngbs)
	{
		var teams = new List<Team>
		{
			new Team
			{
				City = "New York",
				Country = "USA",
				Name = "Yankees",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
				GroupAffiliation = TeamGroupAffiliation.Community,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "New York's premier community quidditch team",
				ContactEmail = "contact@yankees-quidditch.example.com",
			},
			new Team
			{
				City = "Los Angeles",
				Country = "USA",
				Name = "LA Bisons",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
				GroupAffiliation = TeamGroupAffiliation.University,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "University of Los Angeles competitive quidditch team",
				ContactEmail = "labisons@university.example.edu",
			},
			new Team
			{
				City = "Buenos Aires",
				Country = "Argentina",
				Name = "BA Jacks",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "ARG"),
				GroupAffiliation = TeamGroupAffiliation.Community,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "Buenos Aires community quidditch team",
				ContactEmail = "info@bajacks.example.com.ar",
			},
			new Team
			{
				City = "Chicago",
				Country = "USA",
				Name = "Chicago Youth Quidditch",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
				GroupAffiliation = TeamGroupAffiliation.Youth,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "Youth development quidditch program in Chicago",
				ContactEmail = "youth@chicagoquidditch.example.org",
			},
			new Team
			{
				City = "Washington DC",
				Country = "USA",
				Name = "Team USA",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
				GroupAffiliation = TeamGroupAffiliation.National,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "United States National Quidditch Team",
				ContactEmail = "teamusa@usquadball.example.org",
			},
			new Team
			{
				City = "Sydney",
				Country = "Australia",
				Name = "Australia National Team",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "AUS"),
				GroupAffiliation = TeamGroupAffiliation.National,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "Australian National Quidditch Team",
				ContactEmail = "teamaus@quidditch.example.au",
			},
			new Team
			{
				City = "Berlin",
				Country = "Germany",
				Name = "Germany National Team",
				NationalGoverningBody = ngbs.Single(n => n.CountryCode == "DEU"),
				GroupAffiliation = TeamGroupAffiliation.National,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				Description = "German National Quidditch Team",
				ContactEmail = "teamgermany@qbund.example.de",
			},
		};

		dbContext.Teams.AddRange(teams);
		return teams;
	}

	private void SeedTournaments(ManagementHubDbContext dbContext)
	{
		var tournaments = new[]
		{
			new Tournament
			{
				UniqueId = Models.Domain.Tournament.TournamentIdentifier.NewTournamentId().ToString(),
				Name = "US Quadball Cup 2024",
				Description = "National club championship tournament",
				Type = TournamentType.Club,
				StartDate = new DateOnly(2024, 8, 15),
				EndDate = new DateOnly(2024, 8, 18),
				Country = "USA",
				City = "Richmond",
				Organizer = "US Quadball",
				IsPrivate = false,
				IsRegistrationOpen = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new Tournament
			{
				UniqueId = Models.Domain.Tournament.TournamentIdentifier.NewTournamentId().ToString(),
				Name = "World Cup 2024",
				Description = "International national teams championship",
				Type = TournamentType.National,
				StartDate = new DateOnly(2024, 7, 20),
				EndDate = new DateOnly(2024, 7, 23),
				Country = "USA",
				City = "Minneapolis",
				Organizer = "IQA",
				IsPrivate = false,
				IsRegistrationOpen = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new Tournament
			{
				UniqueId = Models.Domain.Tournament.TournamentIdentifier.NewTournamentId().ToString(),
				Name = "Youth Quidditch Championship 2024",
				Description = "Youth development tournament for players under 18",
				Type = TournamentType.Youth,
				StartDate = new DateOnly(2024, 6, 10),
				EndDate = new DateOnly(2024, 6, 12),
				Country = "USA",
				City = "Chicago",
				Organizer = "US Quadball Youth",
				IsPrivate = false,
				IsRegistrationOpen = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new Tournament
			{
				UniqueId = Models.Domain.Tournament.TournamentIdentifier.NewTournamentId().ToString(),
				Name = "Fantasy Tournament 2027",
				Description = "Fun fantasy league tournament - Future event for testing team registration",
				Type = TournamentType.Fantasy,
				StartDate = new DateOnly(2027, 9, 5),
				EndDate = new DateOnly(2027, 9, 7),
				Country = "USA",
				City = "Portland",
				Organizer = "Fantasy Quadball League",
				IsPrivate = false,
				IsRegistrationOpen = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
		};

		dbContext.Tournaments.AddRange(tournaments);
	}

	private List<Certification> EnsureCertifications(ManagementHubDbContext dbContext)
	{
		var certifications = dbContext.Certifications.ToList();
		var existingCertifications = certifications.Select(c => (c.Level, c.Version)).ToHashSet();

		foreach (var version in Enum.GetValues<CertificationVersion>())
		{
			foreach (var level in Enum.GetValues<CertificationLevel>())
			{
				if (existingCertifications.Contains((level, (CertificationVersion?)version))) continue;

				var certification = new Certification
				{
					CreatedAt = DateTime.UtcNow,
					Level = level,
					Version = version,
				};

				certifications.Add(certification);
				dbContext.Certifications.Add(certification);
			}
		}

		return certifications;
	}

	private List<Language> SeedLanguages(ManagementHubDbContext dbContext)
	{
		var languages = new List<Language>(8)
		{
			new Language { ShortName = "en", ShortRegion = "US" },
			new Language { ShortName = "en", ShortRegion = "GB" },
			new Language { ShortName = "pt", ShortRegion = "BR" },
			new Language { ShortName = "es", ShortRegion = "ES" },
			new Language { ShortName = "es", ShortRegion = "419" },
			new Language { ShortName = "fr" },
			new Language { ShortName = "it" },
			new Language { ShortName = "de" },
		};

		dbContext.Languages.AddRange(languages);
		return languages;
	}

	private SeedUsers SeedUsersAndRoles(ManagementHubDbContext dbContext)
	{
		var referee = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "referee@example.com",
			EncryptedPassword = "$2a$11$zEL0W9EagvALrxHLEQwH7eaCxJl45ewy7SfzQ140Zf1vWgL0IyCDm", // "password"
			FirstName = "Jimmy",
			LastName = "Referee",
		};

		var ngbAdmin = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "ngb_admin@example.com",
			EncryptedPassword = "$2a$11$XsCgv0LdJ74WK7N0LBknSOHeb64FXsazAcV1.9PJzNmTra./nWWYm", // "password"
			FirstName = "Jason",
			LastName = "NgbAdmin",
		};

		var iqaAdmin = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "iqa_admin@example.com",
			EncryptedPassword = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne", // "password"
			FirstName = "Jonathan",
			LastName = "IqaAdmin",
		};

		var refereeWithEmptyName = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "empty@example.com",
			EncryptedPassword = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne", // "password"
			FirstName = null,
			LastName = null,
			UniqueId = "U_abcdefghijklmnopqrstuvwxyy"
		};

		var teamManager = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "team_manager@example.com",
			EncryptedPassword = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne", // "password"
			FirstName = "Tom",
			LastName = "TeamManager",
		};

		var playerSarah = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "sarah.player@example.com",
			EncryptedPassword = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne", // "password"
			FirstName = "Sarah",
			LastName = "Player",
		};

		var coachMike = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "mike.coach@example.com",
			EncryptedPassword = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne", // "password"
			FirstName = "Mike",
			LastName = "Coach",
		};

		var recertTestReferee = new User
		{
			CreatedAt = DateTime.UtcNow,
			Email = "recert.test@example.com",
			EncryptedPassword = "$2a$11$YURdUdxxppPle1z32ZExtu8Jk7lXJxpcckfOtpznfw3VT2zsZmzne", // "password"
			FirstName = "Recert",
			LastName = "TestReferee",
		};

		dbContext.Users.AddRange(referee, ngbAdmin, iqaAdmin, refereeWithEmptyName, teamManager, playerSarah, coachMike, recertTestReferee);

		dbContext.Roles.AddRange(
			new Role { AccessType = UserAccessType.Referee, User = referee, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.NgbAdmin, User = ngbAdmin, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.IqaAdmin, User = iqaAdmin, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.Referee, User = refereeWithEmptyName, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.Referee, User = teamManager, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.Referee, User = playerSarah, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.Referee, User = coachMike, CreatedAt = DateTime.UtcNow },
			new Role { AccessType = UserAccessType.Referee, User = recertTestReferee, CreatedAt = DateTime.UtcNow }
		);

		return new SeedUsers(referee, ngbAdmin, iqaAdmin, teamManager, playerSarah, coachMike, recertTestReferee);
	}

	private void SeedUserAssociations(ManagementHubDbContext dbContext, NationalGoverningBody[] ngbs, List<Team> teams, SeedUsers users)
	{
		dbContext.UserAttributes.AddRange(
			new UserAttribute { User = users.Referee, Prefix = string.Empty, Key = "accessibility", Attribute = """{ "timeExtension": 20 }""" },
			new UserAttribute { User = users.Referee, Prefix = "USA", Key = "usqid", Attribute = """ "deadbeef" """ },
			new UserAttribute { User = users.Referee, Prefix = "POL", Key = "international", Attribute = """true""" }
		);

		dbContext.NationalGoverningBodyAdmins.Add(new NationalGoverningBodyAdmin
		{
			CreatedAt = DateTime.UtcNow,
			NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
			UpdatedAt = DateTime.UtcNow,
			User = users.NgbAdmin,
		});

		dbContext.RefereeLocations.Add(new RefereeLocation
		{
			Referee = users.Referee,
			AssociationType = RefereeNgbAssociationType.Primary,
			NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});

		var yankees = teams.First();

		dbContext.RefereeTeams.AddRange(
			new RefereeTeam
			{
				Referee = users.Referee,
				AssociationType = RefereeTeamAssociationType.Player,
				Team = yankees,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new RefereeTeam
			{
				Referee = users.PlayerSarah,
				AssociationType = RefereeTeamAssociationType.Player,
				Team = yankees,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new RefereeTeam
			{
				Referee = users.CoachMike,
				AssociationType = RefereeTeamAssociationType.Coach,
				Team = yankees,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			}
		);

		dbContext.TeamManagers.Add(new TeamManager
		{
			User = users.TeamManager,
			Team = yankees,
			AddedBy = users.TeamManager,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});
	}

	private Test[] SeedTests(ManagementHubDbContext dbContext, List<Certification> certifications, List<Language> languages)
	{
		var tests = new[]
		{
			new Test
			{
				Active = false,
				Certification = certifications.First(c => c.Level == CertificationLevel.Assistant),
				Description = "This is an inactive test for AR you shouldn't see.",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 80,
				Name = "INACTIVE Assitant Ref 2018",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 10,
			},
			new Test
			{
				Active = false,
				Certification = certifications.First(c => c.Level == CertificationLevel.Assistant && c.Version == CertificationVersion.Twenty),
				Description = "Previous AR test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 80,
				Name = "Assitant Ref 2020",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 10,
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Assistant),
				Description = "Latest AR test",
				NewLanguage = languages.Last(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 80,
				Name = "Assitant Ref 2022",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 10,
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Flag),
				Description = "Latest FR test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Snitch,
				MinimumPassPercentage = 80,
				Name = "Flag Ref 2022",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 10,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Head),
				Description = "Latest HR test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Head,
				MinimumPassPercentage = 80,
				Name = "Head Ref 2022",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 15,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 8,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Scorekeeper),
				Description = "Latest SC test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Scorekeeper,
				MinimumPassPercentage = 80,
				Name = "Scorekeeper 2022",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 5,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.FlagRunner),
				Description = "Latest FRN test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.FlagRunner,
				MinimumPassPercentage = 80,
				Name = "FlagRunner 2022",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 5,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Assistant),
				Description = "Latest AR test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 80,
				Name = "Assitant Ref 2022 - Recertification",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = true,
				TimeLimit = 10,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Flag),
				Description = "Latest FR test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Snitch,
				MinimumPassPercentage = 80,
				Name = "Flag Ref 2022 - Recertification",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = true,
				TimeLimit = 10,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Head),
				Description = "Latest HR test",
				NewLanguage = languages.First(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Head,
				MinimumPassPercentage = 80,
				Name = "Head Ref 2022 - Recertification",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = true,
				TimeLimit = 15,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 12,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Scorekeeper),
				Description = "60min log SK",
				NewLanguage = languages.Last(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Scorekeeper,
				MinimumPassPercentage = 80,
				Name = "SK 2022 - 50",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 60,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 2,
			},
			new Test
			{
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.FlagRunner),
				Description = "60min log FRN",
				NewLanguage = languages.Last(),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.FlagRunner,
				MinimumPassPercentage = 80,
				Name = "FRN 2022 - 50",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 60,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 2,
			},
			new Test
			{
				Active = true,
				Certification = certifications.First(c => c.Level == CertificationLevel.Assistant && c.Version == CertificationVersion.Eighteen),
				Description = "Legacy AR benchmark for filter coverage",
				NewLanguage = languages.First(l => l.ShortName == "en" && l.ShortRegion == "GB"),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 80,
				Name = "Assistant Ref 2018 - Legacy",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 15,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.First(c => c.Level == CertificationLevel.Assistant && c.Version == CertificationVersion.Twenty),
				Description = "Mid-cycle AR benchmark for filter coverage",
				NewLanguage = languages.First(l => l.ShortName == "pt" && l.ShortRegion == "BR"),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 80,
				Name = "Assistant Ref 2020 - BR",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 15,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 5,
			},
			new Test
			{
				Active = true,
				Certification = certifications.First(c => c.Level == CertificationLevel.Assistant && c.Version == CertificationVersion.TwentyFour),
				Description = "Current AR certification exam",
				NewLanguage = languages.First(l => l.ShortName == "es" && l.ShortRegion == "419"),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Assistant,
				MinimumPassPercentage = 85,
				Name = "Assistant Ref 2024 - LATAM",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 20,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 8,
			},
			new Test
			{
				Active = true,
				Certification = certifications.First(c => c.Level == CertificationLevel.Scorekeeper && c.Version == CertificationVersion.TwentyFour),
				Description = "Current SK exam in German",
				NewLanguage = languages.First(l => l.ShortName == "de"),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Scorekeeper,
				MinimumPassPercentage = 80,
				Name = "Scorekeeper 2024 - DE",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 25,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 10,
			},
			new Test
			{
				Active = true,
				Certification = certifications.First(c => c.Level == CertificationLevel.Flag && c.Version == CertificationVersion.TwentyFour),
				Description = "Current Flag exam in French",
				NewLanguage = languages.First(l => l.ShortName == "fr"),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Snitch,
				MinimumPassPercentage = 82,
				Name = "Flag Ref 2024 - FR",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 25,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 8,
			},
			new Test
			{
				Active = true,
				Certification = certifications.First(c => c.Level == CertificationLevel.Head && c.Version == CertificationVersion.TwentyFour),
				Description = "Current Head exam in English",
				NewLanguage = languages.First(l => l.ShortName == "en" && l.ShortRegion == "US"),
				CreatedAt = DateTime.UtcNow,
				Level = TestLevel.Head,
				MinimumPassPercentage = 85,
				Name = "Head Ref 2024 - EN",
				NegativeFeedback = "You failed",
				PositiveFeedback = "You passed",
				Recertification = false,
				TimeLimit = 35,
				UniqueId = Models.Domain.Tests.TestIdentifier.NewTestId().ToString(),
				TestableQuestionCount = 12,
			},
		};

		dbContext.Tests.AddRange(tests);
		return tests;
	}

	private void SeedTestHistoryAndCertifications(
		ManagementHubDbContext dbContext,
		Test[] tests,
		List<Certification> certifications,
		User referee,
		User recertTestReferee)
	{
		dbContext.TestResults.Add(new TestResult
		{
			CreatedAt = DateTime.UtcNow.AddDays(-50),
			Duration = "00:10:00",
			MinimumPassPercentage = 80,
			Passed = true,
			Percentage = 80,
			PointsAvailable = 25,
			PointsScored = 20,
			Referee = referee,
			Test = tests[1],
			TestLevel = TestLevel.Assistant,
		});

		dbContext.TestResults.Add(new TestResult
		{
			CreatedAt = DateTime.UtcNow.AddDays(-5),
			Duration = "00:10:00",
			MinimumPassPercentage = 80,
			Passed = false,
			Percentage = 20,
			PointsAvailable = 25,
			PointsScored = 5,
			Referee = referee,
			Test = tests[2],
			TestLevel = TestLevel.Assistant,
		});

		dbContext.RefereeCertifications.Add(new RefereeCertification
		{
			Certification = tests[1].Certification!,
			CreatedAt = DateTime.UtcNow.AddDays(-50),
			ReceivedAt = DateTime.UtcNow.AddDays(-50),
			Referee = referee,
		});

		var flagRecertTest = tests.First(t => t.Recertification == true && t.Certification!.Level == CertificationLevel.Flag);
		var previousVersionForFlagRecert = flagRecertTest.Certification!.Version!.Value - 1;

		dbContext.RefereeCertifications.Add(new RefereeCertification
		{
			Certification = certifications.First(c => c.Level == CertificationLevel.Flag && c.Version == previousVersionForFlagRecert),
			CreatedAt = DateTime.UtcNow.AddDays(-30),
			ReceivedAt = DateTime.UtcNow.AddDays(-30),
			Referee = recertTestReferee,
		});
	}

	private void SeedQuestions(ManagementHubDbContext dbContext, IEnumerable<Test> tests)
	{
		foreach (var test in tests)
		{
			var questions = Enumerable.Range(1, test.TestableQuestionCount).Select(i => new Question
			{
				Test = test,
				Description = $"Question {i}",
				PointsAvailable = 1,
				SequenceId = i,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				Answers = Enumerable.Range(1, 4).Select(j => new Answer
				{
					Correct = (i % 4) + 1 == j,
					Description = $"{((i % 4) + 1 == j ? "Correct " : string.Empty)}Answer {j}",
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
				}).ToArray(),
			});

			dbContext.Questions.AddRange(questions);
		}
	}

	private sealed record SeedUsers(
		User Referee,
		User NgbAdmin,
		User IqaAdmin,
		User TeamManager,
		User PlayerSarah,
		User CoachMike,
		User RecertTestReferee);
}
