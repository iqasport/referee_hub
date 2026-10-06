using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using Xunit;

namespace ManagementHub.IntegrationTests;

public class FlagRunnerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;

	public FlagRunnerIntegrationTests(TestWebApplicationFactory factory)
	{
		this._client = factory.CreateClient();
	}

	[Fact]
	public async Task AvailableTests_ShouldExposeFlagRunnerWithSameEligibilityAsScorekeeper()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var response = await this._client.GetAsync("/api/v2/referees/me/tests/available");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var testsJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var tests = testsJson.RootElement.EnumerateArray().ToList();

		var scorekeeper = tests.First(t => t.GetProperty("level").GetString() == "scorekeeper");
		var flagrunner = tests.First(t => t.GetProperty("level").GetString() == "flagrunner");

		scorekeeper.GetProperty("isRefereeEligible").GetBoolean()
			.Should().Be(flagrunner.GetProperty("isRefereeEligible").GetBoolean());
		scorekeeper.GetProperty("refereeEligibilityResult").GetString()
			.Should().Be(flagrunner.GetProperty("refereeEligibilityResult").GetString());
	}

	[Fact]
	public async Task AvailableTests_ShouldSortFlagRunnerAfterScorekeeper()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var response = await this._client.GetAsync("/api/v2/referees/me/tests/available");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var testsJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var levels = testsJson.RootElement
			.EnumerateArray()
			.Select(t => t.GetProperty("level").GetString())
			.ToList();

		var scorekeeperIndex = levels.IndexOf("scorekeeper");
		var flagrunnerIndex = levels.IndexOf("flagrunner");

		scorekeeperIndex.Should().BeGreaterThanOrEqualTo(0);
		flagrunnerIndex.Should().BeGreaterThan(scorekeeperIndex);
	}

	[Fact]
	public async Task AvailableTests_ShouldIncludeDiverseDataForFilters()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var response = await this._client.GetAsync("/api/v2/referees/me/tests/available");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var testsJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var tests = testsJson.RootElement.EnumerateArray().ToList();

		var versions = tests
			.SelectMany(t => t.GetProperty("awardedCertifications").EnumerateArray())
			.Select(c => c.GetProperty("version").GetString())
			.Where(v => !string.IsNullOrWhiteSpace(v))
			.Distinct()
			.ToList();

		var languages = tests
			.Select(t => t.GetProperty("language").GetString())
			.Where(l => !string.IsNullOrWhiteSpace(l))
			.Distinct()
			.ToList();

		var levels = tests
			.Select(t => t.GetProperty("level").GetString())
			.Where(l => !string.IsNullOrWhiteSpace(l))
			.Distinct()
			.ToList();

		versions.Should().Contain("twentyfour", "newest rulebook should be present for the default filter");
		versions.Count.Should().BeGreaterThanOrEqualTo(3, "rulebook filter should provide multiple options");

		languages.Count.Should().BeGreaterThanOrEqualTo(4, "language filter should provide diverse options");
		levels.Should().Contain(new[] { "assistant", "scorekeeper", "snitch", "head" }, "type filter should include common exam categories");
	}
}
