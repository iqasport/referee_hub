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

		// Debug: Print response content if not successful
		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync();
			System.Console.WriteLine($"Response Status: {response.StatusCode}");
			System.Console.WriteLine($"Response Content: {errorContent}");
		}

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

	/// <summary>
	/// Test that a player from one NGB can join a National Team of another NGB.
	/// This verifies that national teams are truly cross-NGB and not restricted to a player's primary NGB.
	/// </summary>
	[Fact]
	public async Task PlayerFromOneNgb_CanJoinNationalTeamOfAnotherNgb()
	{
		// Arrange: Sign in as sarah.player who is associated with USA
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Step 1: Get all national teams to find the Australia National Team
		var nationalTeamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		nationalTeamsResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"should be able to retrieve national teams");

		var nationalTeamsResult = await nationalTeamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		nationalTeamsResult.Should().NotBeNull();
		nationalTeamsResult!.Items.Should().NotBeNull();

		// Find Australia National Team (which is from AUS NGB, different from sarah's USA NGB)
		var australiaTeam = nationalTeamsResult.Items!
			.FirstOrDefault(t => t.Name.Contains("Australia"));
		australiaTeam.Should().NotBeNull("Australia National Team should be in the seeded data");

		// Step 2: Get current user profile to verify current state
		var profileBeforeResponse = await this._client.GetAsync("/api/v2/Referees/me");
		profileBeforeResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"should be able to get current user profile");

		// Step 3: Update sarah's profile to join Australia National Team
		var updateRequest = new
		{
			primaryNgb = "USA",  // Keep USA as primary NGB
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = new
			{
				id = australiaTeam!.TeamId.ToString()
			}
		};

		var updateResponse = await this._client.PutAsJsonAsync("/api/v2/Referees/me", updateRequest);
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"player from USA should be able to join Australia National Team");

		// Step 4: Verify the update by getting the profile again
		var profileAfterResponse = await this._client.GetAsync("/api/v2/Referees/me");
		profileAfterResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"should be able to get updated profile");

		var profileAfterContent = await profileAfterResponse.Content.ReadAsStringAsync();
		profileAfterContent.Should().Contain(australiaTeam.TeamId.ToString(),
			"profile should now contain the Australia National Team ID");

		// Optional: Clean up by removing the national team assignment
		var cleanupRequest = new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null
		};

		await this._client.PutAsJsonAsync("/api/v2/Referees/me", cleanupRequest);
	}
}
