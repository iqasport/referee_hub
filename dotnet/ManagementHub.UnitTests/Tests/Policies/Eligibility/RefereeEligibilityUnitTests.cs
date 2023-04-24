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

public class RefereeEligibilityUnitTests
{
	private readonly Mock<IUserContextProvider> userContextProvider = new();
	private readonly Mock<IRefereeContextProvider> refereeContextProvider = new();
	private readonly Mock<ISystemClock> clock = new();

	private static readonly UserIdentifier TestUserId = UserIdentifier.NewUserId();

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

	private async Task<IDictionary<Test, bool>> ExecuteChecksAsync(IEnumerable<Test> tests)
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

		lang ??= LanguageIdentifier.Default;
		var userContext = new Mock<IUserContext>();
		userContext.Setup(u => u.UserId).Returns(TestUserId);
		userContext.Setup(u => u.UserData).Returns(new UserData(new Email("test@test.com"), "John", "Smith")
		{
			UserLang = lang.Value,
		});

		this.userContextProvider.Setup(p => p.GetUserContextAsync(TestUserId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(userContext.Object);
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

		Assert.True(result[TestData.Assistant18], "Should return AR18");
		Assert.True(result[TestData.Assistant20], "Should return AR20");
		Assert.True(result[TestData.Assistant22], "Should return AR22");
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

		Assert.False(result[TestData.Flag18], "Should not return FR18");
		Assert.False(result[TestData.Flag20], "Should not return FR20");
		Assert.False(result[TestData.Flag22], "Should not return FR22");
		Assert.False(result[TestData.Head18], "Should not return HR18");
		Assert.False(result[TestData.Head20], "Should not return HR20");
		Assert.False(result[TestData.Head22], "Should not return HR22");
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

		Assert.False(result[TestData.Assistant18], "Should not return AR18");
		Assert.False(result[TestData.Assistant20], "Should not return AR20");
		Assert.True(result[TestData.Assistant22], "Should return AR22");
		Assert.True(result[TestData.Flag18], "Should return FR18");
		Assert.True(result[TestData.Flag20], "Should return FR20");
		Assert.False(result[TestData.Flag22], "Should not return FR22");
	}
}
