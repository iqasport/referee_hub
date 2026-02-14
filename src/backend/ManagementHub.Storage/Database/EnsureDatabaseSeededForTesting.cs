using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

public class EnsureDatabaseSeededForTesting : DatabaseStartupService
{
	public EnsureDatabaseSeededForTesting(IServiceProvider serviceProvider, ILogger<EnsureDatabaseSeededForTesting> logger) : base(serviceProvider, logger)
	{
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
				LogoUrl = "https://ui-avatars.com/api/?name=Yankees&background=003087&color=fff&size=128",
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
				LogoUrl = "https://ui-avatars.com/api/?name=LA+Bisons&background=FFB81C&color=003087&size=128",
				Description = "University of Los Angeles competitive quidditch team",
				ContactEmail = "labisons@university.example.edu",
			},
			new Team
			{
				City = "Buenos Aires",
				Country = "Argentina",
				Name = "BA Jacks",
				NationalGoverningBody = ngbs.First(),
				GroupAffiliation = TeamGroupAffiliation.Community,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
				LogoUrl = "https://ui-avatars.com/api/?name=BA+Jacks&background=75AADB&color=fff&size=128",
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
				LogoUrl = "https://ui-avatars.com/api/?name=Chicago+Youth&background=C8102E&color=fff&size=128",
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
				LogoUrl = "https://ui-avatars.com/api/?name=Team+USA&background=B22234&color=fff&size=128",
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
				LogoUrl = "https://ui-avatars.com/api/?name=Australia&background=00843D&color=FFCD00&size=128",
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
				LogoUrl = "https://ui-avatars.com/api/?name=Germany&background=000000&color=DD0000&size=128",
				Description = "German National Quidditch Team",
				ContactEmail = "teamgermany@qbund.example.de",
			},
		};

		dbContext.Teams.AddRange(teams);

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

		var certifications = new List<Certification>(32);
		foreach (var version in Enum.GetValues<CertificationVersion>())
			foreach (var level in Enum.GetValues<CertificationLevel>())
				certifications.Add(new Certification
				{
					CreatedAt = DateTime.UtcNow,
					Level = level,
					Version = version,
				});
		dbContext.Certifications.AddRange(certifications);

		var languages = new List<Language>(8)
		{
			new Language{ ShortName = "en", ShortRegion = "US"},
			new Language{ ShortName = "en", ShortRegion = "GB"},
			new Language{ ShortName = "pt", ShortRegion = "BR"},
			new Language{ ShortName = "es", ShortRegion = "ES"},
			new Language{ ShortName = "es", ShortRegion = "419"},
			new Language{ ShortName = "fr" },
			new Language{ ShortName = "it" },
			new Language{ ShortName = "de" },
		};
		dbContext.Languages.AddRange(languages);

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
			new Role
			{
				AccessType = UserAccessType.Referee,
				User = referee,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.NgbAdmin,
				User = ngbAdmin,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.IqaAdmin,
				User = iqaAdmin,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.Referee,
				User = refereeWithEmptyName,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.Referee,
				User = teamManager,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.Referee,
				User = playerSarah,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.Referee,
				User = coachMike,
				CreatedAt = DateTime.UtcNow,
			},
			new Role
			{
				AccessType = UserAccessType.Referee,
				User = recertTestReferee,
				CreatedAt = DateTime.UtcNow,
			});

		dbContext.UserAttributes.AddRange(
			new UserAttribute
			{
				User = referee,
				Prefix = string.Empty,
				Key = "accessibility",
				Attribute = """{ "timeExtension": 20 }""",
			},
			new UserAttribute
			{
				User = referee,
				Prefix = "USA",
				Key = "usqid",
				Attribute = """ "deadbeef" """,
			},
			new UserAttribute
			{
				User = referee,
				Prefix = "POL",
				Key = "international",
				Attribute = """true""",
			});

		dbContext.NationalGoverningBodyAdmins.Add(new NationalGoverningBodyAdmin
		{
			CreatedAt = DateTime.UtcNow,
			NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
			UpdatedAt = DateTime.UtcNow,
			User = ngbAdmin,
		});

		dbContext.RefereeLocations.Add(new RefereeLocation
		{
			Referee = referee,
			AssociationType = RefereeNgbAssociationType.Primary,
			NationalGoverningBody = ngbs.Single(n => n.CountryCode == "USA"),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});

		dbContext.RefereeTeams.AddRange(
			new RefereeTeam
			{
				Referee = referee,
				AssociationType = RefereeTeamAssociationType.Player,
				Team = teams.First(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new RefereeTeam
			{
				Referee = playerSarah,
				AssociationType = RefereeTeamAssociationType.Player,
				Team = teams.First(), // Yankees team
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			},
			new RefereeTeam
			{
				Referee = coachMike,
				AssociationType = RefereeTeamAssociationType.Coach,
				Team = teams.First(), // Yankees team
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			});

		// Add team manager assignment for integration tests
		dbContext.TeamManagers.Add(new TeamManager
		{
			User = teamManager,
			Team = teams.First(), // Yankees team
			AddedBy = teamManager, // Self-assigned for bootstrap
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});

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
				Recertification =false,
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
		};
		dbContext.Tests.AddRange(tests);

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

		// Add Flag certification (previous version) to recert test referee so they're eligible for Flag recert test
		// The Flag recert test requires Flag certification from the previous version
		// Since the Flag recert test is for the latest version (2022), we need the previous version (2020)
		var flagRecertTest = tests.First(t => t.Recertification == true && t.Certification!.Level == CertificationLevel.Flag);
		var previousVersionForFlagRecert = flagRecertTest.Certification!.Version!.Value - 1;
		dbContext.RefereeCertifications.Add(new RefereeCertification
		{
			Certification = certifications.First(c => c.Level == CertificationLevel.Flag && c.Version == previousVersionForFlagRecert),
			CreatedAt = DateTime.UtcNow.AddDays(-30),
			ReceivedAt = DateTime.UtcNow.AddDays(-30),
			Referee = recertTestReferee,
		});

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

		dbContext.SaveChanges();
	}
}
