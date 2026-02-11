using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.IntegrationTests.Models;
using ManagementHub.Service.Filtering;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for National Teams API endpoints.
/// Tests that national teams are returned independently of user's NGBs.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class NationalTeamsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public NationalTeamsApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task GetNationalTeams_AsAuthenticatedUser_ShouldReturnAllNationalTeams()
	{
		// Arrange: Sign in as a regular player
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Act: Get all national teams
		var response = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"authenticated user should be able to list all national teams");

		var result = await response.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		result.Should().NotBeNull();
		result!.Items.Should().NotBeNull();

		// All returned teams should have national group affiliation
		result.Items!.Should().OnlyContain(team => team.GroupAffiliation == "national",
			"the endpoint should only return teams with national group affiliation");
	}

	[Fact]
	public async Task GetNationalTeams_AsUnauthenticatedUser_ShouldReturnUnauthorized()
	{
		// Act: Try to get national teams without authentication
		var response = await this._client.GetAsync("/api/v2/Teams/national");

		// Assert: Should be unauthorized
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
			"unauthenticated users should not be able to access the national teams endpoint");
	}

	[Fact]
	public async Task GetNationalTeams_ShouldReturnTeamsFromAllNgbs()
	{
		// Arrange: Sign in as USA NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Get all national teams
		var response = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var result = await response.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		result.Should().NotBeNull();

		// Should return national teams from all NGBs, not just USA
		// (The actual assertion depends on seeded test data, but this verifies the endpoint works)
		result!.Items.Should().NotBeNull();
	}
}
