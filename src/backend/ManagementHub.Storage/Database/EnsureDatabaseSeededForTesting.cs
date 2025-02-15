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

	protected override async Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
		try
		{
			var ngbCount = await dbContext.NationalGoverningBodies.CountAsync(stoppingToken);
			if (ngbCount > 0)
			{
				this.logger.LogInformation(-0x48302e00, "Database not empty. Skipping seeding.");
				return;
			}

			this.logger.LogInformation(-0x48302dff, "Ensuring database is seeded...");

			await this.SeedDatabaseAsync(dbContext, stoppingToken);

			this.logger.LogInformation(-0x48302dfe, "Ensuring database is seeded completed.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(-0x48302dfd, ex, "Error while seeding database.");
			throw;
		}
	}

	private async Task SeedDatabaseAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
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
				NationalGoverningBody = ngbs.Last(),
				GroupAffiliation = TeamGroupAffiliation.Community,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
			},
			new Team
			{
				City = "Los Angeles",
				Country = "USA",
				Name = "LA Bisons",
				NationalGoverningBody = ngbs.Last(),
				GroupAffiliation = TeamGroupAffiliation.University,
				CreatedAt = DateTime.UtcNow,
				JoinedAt = DateTime.UtcNow,
				Status = TeamStatus.Competitive,
				UpdatedAt = DateTime.UtcNow,
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
			},
		};

		dbContext.Teams.AddRange(teams);

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

		dbContext.Users.AddRange(referee, ngbAdmin, iqaAdmin, refereeWithEmptyName);

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
			NationalGoverningBody = ngbs.Last(),
			UpdatedAt = DateTime.UtcNow,
			User = ngbAdmin,
		});

		dbContext.RefereeLocations.Add(new RefereeLocation
		{
			Referee = referee,
			AssociationType = RefereeNgbAssociationType.Primary,
			NationalGoverningBody = ngbs.Last(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});

		dbContext.RefereeTeams.Add(new RefereeTeam
		{
			Referee = referee,
			AssociationType = RefereeTeamAssociationType.Player,
			Team = teams.First(),
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

		await dbContext.SaveChangesAsync(stoppingToken);
	}
}
