using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
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
			this.logger.LogInformation(0, "Ensuring database is seeded...");

			await this.SeedDatabaseAsync(dbContext, stoppingToken);

			this.logger.LogInformation(0, "Ensuring database is seeded completed.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Error while seeding database.");
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
		};
		dbContext.NationalGoverningBodies.AddRange(ngbs);

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
			new Language{ ShortName = "de" },
			new Language{ ShortName = "it" },
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

		dbContext.Users.AddRange(referee, ngbAdmin, iqaAdmin);

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
			});

		dbContext.NationalGoverningBodyAdmins.Add(new NationalGoverningBodyAdmin
		{
			CreatedAt = DateTime.UtcNow,
			NationalGoverningBody = ngbs.Last(),
			UpdatedAt = DateTime.UtcNow,
			User = ngbAdmin,
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
				Active = true,
				Certification = certifications.Last(c => c.Level == CertificationLevel.Assistant),
				Description = "Latest AR test",
				NewLanguage = languages.First(),
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
		};
		dbContext.Tests.AddRange(tests);

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
			Test = tests[1],
			TestLevel = TestLevel.Assistant,
		});

		var questions = Enumerable.Range(1, 10).Select(i => new Question
		{
			Test = tests[1],
			Description = $"Question {i}",
			PointsAvailable = 1,
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

		await dbContext.SaveChangesAsync(stoppingToken);
	}
}
