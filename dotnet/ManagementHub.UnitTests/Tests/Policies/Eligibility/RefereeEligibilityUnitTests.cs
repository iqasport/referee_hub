using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tests.Policies.Eligibility;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ManagementHub.UnitTests.Tests.Policies.Eligibility;

internal static class RefereeEligibilityUnitTestsExtensions
{
	public static void Includes(this Dictionary<Test, bool> result, Test test) => Assert.True(result[test], $"Result should include {test.Name}");
	public static void Excludes(this Dictionary<Test, bool> result, Test test) => Assert.False(result[test], $"Result should not include {test.Name}");
}

public class RefereeEligibilityUnitTests
{
	private readonly Mock<IUserContextProvider> userContextProvider = new();
	private readonly Mock<IRefereeContextProvider> refereeContextProvider = new();
	private readonly Mock<ISystemClock> clock = new();

	private static readonly UserIdentifier TestUserId = UserIdentifier.NewUserId();

	private static readonly DateTime TestCurrentDateTime = new DateTime(2023, 04, 01, 21, 37, 42, DateTimeKind.Utc);

	private RefereeEligibilityChecker SetupChecker()
	{
		return new RefereeEligibilityChecker(
			new IRefereeEligibilityPolicy[]
			{
				new HasRequiredCertificationEligibilityPolicy(this.refereeContextProvider.Object),
				new NumberOfAttemptsEligibilityPolicy(this.refereeContextProvider.Object),
				new PaymentEligibilityPolicy(this.refereeContextProvider.Object),
				new RefereeAttemptEligibilityPolicy(this.refereeContextProvider.Object, this.clock.Object),
				new RefereeCertifiedEligibilityPolicy(this.refereeContextProvider.Object),
				new RefereeLanguageEligibilityPolicy(this.userContextProvider.Object),
			},
			Mock.Of<ILogger<RefereeEligibilityChecker>>());
	}

	private async Task<Dictionary<Test, bool>> ExecuteChecksAsync(IEnumerable<Test> tests)
	{
		var checker = this.SetupChecker();
		var results = new Dictionary<Test, bool>();

		foreach (var test in tests)
		{
			results[test] = await checker.CheckRefereeEligibilityAsync(test, TestUserId);
		}

		return results;
	}

	private static readonly Test[] AllTests = new[]
	{
		TestData.Assistant18,
		TestData.Assistant20,
		TestData.Assistant22,
		TestData.Flag18,
		TestData.Flag20,
		TestData.Flag22,
		TestData.Head18,
		TestData.Head20,
		TestData.Head22,
		TestData.Scorekeeper18,
		TestData.Scorekeeper20,
		TestData.Scorekeeper22,
		TestData.RecertAssistant22,
		TestData.RecertFlag22,
		TestData.RecertHead22,
	};

	private void SetupReferee(IEnumerable<Certification> certifications, IEnumerable<CertificationVersion> payments, IEnumerable<TestAttempt> attempts, LanguageIdentifier? lang = null)
	{
		var refereeContext = new Mock<IRefereeTestContext>();
		refereeContext.Setup(r => r.TestAttempts).Returns(attempts);
		refereeContext.Setup(r => r.AcquiredCertifications).Returns(certifications.ToHashSet());
		refereeContext.Setup(r => r.HeadCertificationsPaid).Returns(payments);
		refereeContext.Setup(r => r.UserId).Returns(TestUserId);

		this.refereeContextProvider.Setup(p => p.GetRefereeTestContextAsync(TestUserId))
			.ReturnsAsync(refereeContext.Object);

		var userContext = new Mock<IUserContext>();
		userContext.Setup(u => u.UserId).Returns(TestUserId);
		userContext.Setup(u => u.UserData).Returns(new UserData(new Email("test@test.com"), "John", "Smith")
		{
			UserLang = lang ?? LanguageIdentifier.Default,
		});

		this.userContextProvider.Setup(p => p.GetUserContextAsync(TestUserId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(userContext.Object);

		this.clock.Setup(c => c.UtcNow).Returns(TestCurrentDateTime);
	}

	

	[Fact]
	public async Task ReturnsAllAssistantTests_WhenRefHasNoCerts()
	{
		this.SetupReferee(Array.Empty<Certification>(), Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Assistant18,
			TestData.Assistant20,
			TestData.Assistant22,
		});

		result.Includes(TestData.Assistant18);
		result.Includes(TestData.Assistant20);
		result.Includes(TestData.Assistant22);
	}

	[Fact]
	public async Task ReturnsNoneFlagOrHeadTests_WhenRefHasNoCerts()
	{
		this.SetupReferee(Array.Empty<Certification>(), Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Flag20,
			TestData.Flag22,
			TestData.Head18,
			TestData.Head20,
			TestData.Head22,
		});

		result.Excludes(TestData.Flag18);
		result.Excludes(TestData.Flag20);
		result.Excludes(TestData.Flag22);
		result.Excludes(TestData.Head18);
		result.Excludes(TestData.Head20);
		result.Excludes(TestData.Head22);
	}

	[Fact]
	public async Task ReturnsFlagTests_WhenRefHasAssistantCertifications()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
		}, Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Assistant18,
			TestData.Assistant20,
			TestData.Assistant22,
			TestData.Flag18,
			TestData.Flag20,
			TestData.Flag22,
		});

		result.Excludes(TestData.Assistant18);
		result.Excludes(TestData.Assistant20);
		result.Includes(TestData.Assistant22);
		result.Includes(TestData.Flag18);
		result.Includes(TestData.Flag20);
		result.Excludes(TestData.Flag22);
	}

	[Fact]
	public async Task ReturnsHeadTests_WhenRefHasFlagCertificationAndPayment()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, new[]
		{
			CertificationVersion.Eighteen,
			CertificationVersion.Twenty,
		}, Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Flag20,
			TestData.Flag22,
			TestData.Head18,
			TestData.Head20,
			TestData.Head22,
		});

		result.Excludes(TestData.Flag18);
		result.Excludes(TestData.Flag20);
		result.Excludes(TestData.Flag22);
		result.Includes(TestData.Head18);
		result.Includes(TestData.Head20);
		result.Excludes(TestData.Head22);
	}

	[Fact]
	public async Task ReturnsNoHeadTests_WhenRefHasFlagCertificationButNoPayment()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Flag20,
			TestData.Flag22,
			TestData.Head18,
			TestData.Head20,
			TestData.Head22,
		});

		result.Excludes(TestData.Flag18);
		result.Excludes(TestData.Flag20);
		result.Excludes(TestData.Flag22);
		result.Excludes(TestData.Head18);
		result.Excludes(TestData.Head20);
		result.Excludes(TestData.Head22);
	}

	[Fact]
	public async Task ReturnTests_WhenNotInCooldownPeriod_BasedOnStartedAt()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, new[]
		{
			CertificationVersion.Twenty,
		}, new[]
		{
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-2),
				TestId = TestData.Assistant22.TestId,
				UserId = TestUserId,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-2),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-4),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
			},
		});

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Head20,
			TestData.Assistant22,
		});

		result.Includes(TestData.Flag18);
		result.Includes(TestData.Head20);
		result.Includes(TestData.Assistant22);
	}

	[Fact]
	public async Task ReturnsNoTests_WhenInCooldownPeriod_BasedOnStartedAt()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, new[]
		{
			CertificationVersion.Twenty,
		}, new[]
		{
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1),
				TestId = TestData.Assistant22.TestId,
				UserId = TestUserId,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-3),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
			},
		});

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Head20,
			TestData.Assistant22,
		});

		result.Excludes(TestData.Flag18);
		result.Excludes(TestData.Head20);
		result.Excludes(TestData.Assistant22);
	}

	[Fact]
	public async Task ReturnTests_WhenNotInCooldownPeriod_BasedOnFinishedAt()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, new[]
		{
			CertificationVersion.Twenty,
		}, new[]
		{
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Assistant22.TimeLimit).AddMinutes(-5),
				TestId = TestData.Assistant22.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Assistant22.TimeLimit).AddSeconds(-1),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit).AddMinutes(-5),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit).AddSeconds(-1),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit).AddMinutes(-5),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit).AddSeconds(-1),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
			},
		});

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Head20,
			TestData.Assistant22,
		});

		result.Includes(TestData.Flag18);
		result.Includes(TestData.Head20);
		result.Includes(TestData.Assistant22);
	}

	[Fact]
	public async Task ReturnsNoTests_WhenInCooldownPeriod_BasedOnFinishedAt()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, new[]
		{
			CertificationVersion.Twenty,
		}, new[]
		{
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Assistant22.TimeLimit),
				TestId = TestData.Assistant22.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Assistant22.TimeLimit).AddSeconds(10),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit).AddSeconds(10),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit).AddSeconds(10),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
			},
		});

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Flag18,
			TestData.Head20,
			TestData.Assistant22,
		});

		result.Excludes(TestData.Flag18);
		result.Excludes(TestData.Head20);
		result.Excludes(TestData.Assistant22);
	}
}
