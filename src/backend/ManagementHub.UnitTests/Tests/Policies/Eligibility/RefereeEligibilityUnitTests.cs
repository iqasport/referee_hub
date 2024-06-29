using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options.Contextual;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace ManagementHub.UnitTests.Tests.Policies.Eligibility;

internal static class RefereeEligibilityUnitTestsExtensions
{
	[ThreadStatic] public static ITestOutputHelper? TestOutput;

	public static void Includes(this Dictionary<Test, RefereeEligibilityResult> result, Test test)
	{
		TestOutput?.WriteLine($"{test.Title}: {result[test]}");
		Assert.True(RefereeEligibilityResult.Eligible == result[test], $"Result should include {test.Title}");
	}

	public static void Excludes(this Dictionary<Test, RefereeEligibilityResult> result, Test test)
	{
		TestOutput?.WriteLine($"{test.Title}: {result[test]}");
		Assert.False(RefereeEligibilityResult.Eligible == result[test], $"Result should not include {test.Title}");
	}
}

public class RefereeEligibilityUnitTests
{
	private readonly Mock<IRefereeContextProvider> refereeContextProvider = new();
	private readonly Mock<ISystemClock> clock = new();
	private readonly TestContextualOptions<TestPolicyOverride> mockContextualOptions = new(new());

	private static readonly UserIdentifier TestUserId = UserIdentifier.NewUserId();

	private static readonly DateTime TestCurrentDateTime = new DateTime(2023, 04, 01, 21, 37, 42, DateTimeKind.Utc);

	public RefereeEligibilityUnitTests(ITestOutputHelper testOutput) => RefereeEligibilityUnitTestsExtensions.TestOutput = testOutput;

	private RefereeEligibilityChecker SetupChecker()
	{
		return new RefereeEligibilityChecker(
			new IRefereeEligibilityPolicy[]
			{
				new HasRequiredCertificationEligibilityPolicy(this.refereeContextProvider.Object),
				new NumberOfAttemptsEligibilityPolicy(this.refereeContextProvider.Object, this.mockContextualOptions),
				new PaymentEligibilityPolicy(this.refereeContextProvider.Object),
				new RefereeAttemptEligibilityPolicy(this.refereeContextProvider.Object, this.clock.Object),
				new RefereeCertifiedEligibilityPolicy(this.refereeContextProvider.Object),
			},
			Mock.Of<ILogger<RefereeEligibilityChecker>>());
	}

	private async Task<Dictionary<Test, RefereeEligibilityResult>> ExecuteChecksAsync(IEnumerable<Test> tests)
	{
		var checker = this.SetupChecker();
		var results = new Dictionary<Test, RefereeEligibilityResult>();

		foreach (var test in tests)
		{
			results[test] = await checker.CheckRefereeEligibilityAsync(test, TestUserId, default);
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

		this.refereeContextProvider.Setup(p => p.GetRefereeTestContextAsync(TestUserId, default))
			.ReturnsAsync(refereeContext.Object);

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
				Level = CertificationLevel.Assistant,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-2),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
				Level = CertificationLevel.Flag,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-4),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
				Level = CertificationLevel.Head,
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
				Level = CertificationLevel.Assistant,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
				Level = CertificationLevel.Flag,
			},
			new TestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-3),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
				Level = CertificationLevel.Head,
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
				Level = CertificationLevel.Assistant,
				PassPercentage = TestData.Assistant22.PassPercentage,
				Passed = false,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit).AddMinutes(-5),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit).AddSeconds(-1),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
				Level = CertificationLevel.Flag,
				PassPercentage = TestData.Flag18.PassPercentage,
				Passed = false,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit).AddMinutes(-5),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit).AddSeconds(-1),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
				Level = CertificationLevel.Head,
				PassPercentage = TestData.Head20.PassPercentage,
				Passed = false,
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
				FinishedAt = TestCurrentDateTime.AddDays(-1).AddSeconds(10),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
				Level = CertificationLevel.Assistant,
				PassPercentage = TestData.Assistant22.PassPercentage,
				Passed = false,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-1).Add(-1 * TestData.Flag18.TimeLimit),
				TestId = TestData.Flag18.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-1).AddSeconds(10),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
				Level = CertificationLevel.Flag,
				PassPercentage = TestData.Flag18.PassPercentage,
				Passed = false,
			},
			new FinishedTestAttempt
			{
				StartedAt = TestCurrentDateTime.AddDays(-3).Add(-1 * TestData.Head20.TimeLimit),
				TestId = TestData.Head20.TestId,
				UserId = TestUserId,
				FinishedAt = TestCurrentDateTime.AddDays(-3).AddSeconds(10),
				FinishMethod = TestAttemptFinishMethod.Timeout,
				Score = 0,
				Level = CertificationLevel.Head,
				PassPercentage = TestData.Head20.PassPercentage,
				Passed = false,
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
	public async Task ReturnsNoRecertTests_WhenRefereeHasNoCerts()
	{
		this.SetupReferee(Array.Empty<Certification>(), Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.RecertAssistant22,
			TestData.RecertFlag22,
			TestData.RecertHead22,
		});

		result.Excludes(TestData.RecertAssistant22);
		result.Excludes(TestData.RecertFlag22);
		result.Excludes(TestData.RecertHead22);
	}

	[Fact]
	public async Task ReturnsAssistantRecertTests_WhenRefereeHasAssistantCert()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
		}, Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.RecertAssistant22,
			TestData.RecertFlag22,
			TestData.RecertHead22,
		});

		result.Includes(TestData.RecertAssistant22);
		result.Excludes(TestData.RecertFlag22);
		result.Excludes(TestData.RecertHead22);
	}

	[Fact]
	public async Task ReturnsFlagRecertTests_WhenRefereeHasFlagCert()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		}, Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.RecertAssistant22,
			TestData.RecertFlag22,
			TestData.RecertHead22,
		});

		result.Excludes(TestData.RecertAssistant22);
		result.Includes(TestData.RecertFlag22);
		result.Excludes(TestData.RecertHead22);
	}

	[Fact]
	public async Task ReturnsNoRecertTests_WhenRefereeHasSameYearCerts_AssistantPreviousYear()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
		}, Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.RecertAssistant22,
			TestData.RecertFlag22,
			TestData.RecertHead22,
		});

		result.Excludes(TestData.RecertAssistant22);
		result.Excludes(TestData.RecertFlag22);
		result.Excludes(TestData.RecertHead22);
	}

	[Fact]
	public async Task ReturnsNoRecertTests_WhenRefereeHasSameYearCerts_FlagPreviousYear()
	{
		this.SetupReferee(new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
		}, Array.Empty<CertificationVersion>(), Array.Empty<TestAttempt>());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.RecertAssistant22,
			TestData.RecertFlag22,
			TestData.RecertHead22,
		});

		result.Excludes(TestData.RecertAssistant22);
		result.Excludes(TestData.RecertFlag22);
		result.Excludes(TestData.RecertHead22);
	}

	[Fact]
	public async Task ReturnsNoTests_WhenRefereeHasPassedAttemptThreshold()
	{
		// has 6 attempts for AR test and 1 for recert
		var attempts = Enumerable.Range(1, 6).Select(i => new TestAttempt
		{
			TestId = TestData.Assistant18.TestId,
			Level = CertificationLevel.Assistant,
			StartedAt = TestCurrentDateTime.AddDays(-i - 1),
			UserId = TestUserId,
		}).Concat(new[]
		{
			new TestAttempt
			{
				TestId = TestData.RecertAssistant22.TestId,
				Level= CertificationLevel.Assistant,
				StartedAt = TestCurrentDateTime.AddHours(-5),
				UserId = TestUserId,
			}
		});
		this.SetupReferee(new[]
			{
				new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			},
			Array.Empty<CertificationVersion>(),
			attempts.ToArray());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Assistant18,
			TestData.RecertAssistant22,
		});

		result.Excludes(TestData.Assistant18);
		result.Excludes(TestData.RecertAssistant22);
	}

	[Fact]
	public async Task ReturnsTest_WhenRefereeHasNotYetPassedAttemptThreshold()
	{
		// has 6 attempts for AR test, so with 5 should be withing threshold
		var attempts = Enumerable.Range(1, 5).Select(i => new TestAttempt
		{
			TestId = TestData.Assistant18.TestId,
			Level = CertificationLevel.Assistant,
			StartedAt = TestCurrentDateTime.AddDays(-i - 1),
			UserId = TestUserId,
		});
		this.SetupReferee(
			Array.Empty<Certification>(),
			Array.Empty<CertificationVersion>(),
			attempts.ToArray());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Assistant18,
		});

		result.Includes(TestData.Assistant18);
	}

	[Fact]
	public async Task ReturnsTest_WhenRefereeHasTheMaxAttemptsOverriden()
	{
		this.mockContextualOptions.Value.MaxAttempts = 10;
		// has 8 attempts for AR test and 1 for recert
		var attempts = Enumerable.Range(1, 8).Select(i => new TestAttempt
		{
			TestId = TestData.Assistant18.TestId,
			Level = CertificationLevel.Assistant,
			StartedAt = TestCurrentDateTime.AddDays(-i - 1),
			UserId = TestUserId,
		}).Concat(new[]
		{
			new TestAttempt
			{
				TestId = TestData.RecertAssistant22.TestId,
				Level= CertificationLevel.Assistant,
				StartedAt = TestCurrentDateTime.AddHours(-5),
				UserId = TestUserId,
			}
		});
		this.SetupReferee(new[]
			{
				new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
			},
			Array.Empty<CertificationVersion>(),
			attempts.ToArray());

		var result = await this.ExecuteChecksAsync(new[]
		{
			TestData.Assistant18,
			TestData.RecertAssistant22,
		});

		result.Includes(TestData.Assistant18);
		result.Excludes(TestData.RecertAssistant22);
	}
}

class TestContextualOptions<T>: IContextualOptions<T> where T : class
{
	public T Value { get; set; }
	public TestContextualOptions(T value) => Value = value;

	public ValueTask<T> GetAsync<U>(in U ctx, CancellationToken ct)
		where U : IOptionsContext
	 => ValueTask.FromResult(this.Value);
}
