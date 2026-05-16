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
}
