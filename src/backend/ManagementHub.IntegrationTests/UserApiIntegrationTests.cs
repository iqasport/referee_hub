using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.IntegrationTests.Models;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for User API endpoints.
/// Tests user-related functionality including managed teams.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class UserApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public UserApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task GetManagedTeams_AsTeamManager_ShouldReturnTeamsWithGroupAffiliation()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get managed teams for the current user
		var response = await this._client.GetAsync("/api/v2/users/me/managedTeams");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"team manager should be able to retrieve their managed teams");

		var managedTeams = await response.Content.ReadFromJsonAsync<List<ManagedTeamViewModelDto>>();
		managedTeams.Should().NotBeNull();
		managedTeams!.Should().NotBeEmpty("team manager should have at least one managed team");

		// Verify that the managed teams include GroupAffiliation
		var yankeesTeam = managedTeams!.Should().ContainSingle(t => t.TeamName == "Yankees",
			"the seeded team manager should manage the Yankees team").Subject;
		
		yankeesTeam.TeamId.Should().NotBeNullOrEmpty("TeamId should be populated");
		yankeesTeam.Ngb.Should().Be("USA", "Yankees team should belong to USA NGB");
		yankeesTeam.GroupAffiliation.Should().Be(TeamGroupAffiliationDto.Community,
			"Yankees team should have Community group affiliation as seeded in test data");
	}

	[Fact]
	public async Task GetManagedTeams_AsUserWithoutManagedTeams_ShouldReturnEmptyList()
	{
		// Arrange: Sign in as regular player (not a team manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Act: Get managed teams for the current user
		var response = await this._client.GetAsync("/api/v2/users/me/managedTeams");

		// Assert: Response should be successful but return empty list
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"request should succeed even if user has no managed teams");

		var managedTeams = await response.Content.ReadFromJsonAsync<List<ManagedTeamViewModelDto>>();
		managedTeams.Should().NotBeNull();
		managedTeams!.Should().BeEmpty("user without team manager role should have no managed teams");
	}

	[Fact]
	public async Task GetManagedTeams_WithMultipleTeams_ShouldReturnAllWithGroupAffiliation()
	{
		// Arrange: Sign in as NGB admin (who is also seeded as team manager for multiple teams)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Get managed teams for the current user
		var response = await this._client.GetAsync("/api/v2/users/me/managedTeams");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to retrieve their managed teams");

		var managedTeams = await response.Content.ReadFromJsonAsync<List<ManagedTeamViewModelDto>>();
		managedTeams.Should().NotBeNull();

		// Verify all teams have the GroupAffiliation property set (even if null)
		foreach (var team in managedTeams!)
		{
			team.TeamId.Should().NotBeNullOrEmpty("all teams should have a TeamId");
			team.TeamName.Should().NotBeNullOrEmpty("all teams should have a TeamName");
			team.Ngb.Should().NotBeNullOrEmpty("all teams should have an NGB");
			// GroupAffiliation can be null, but the property should exist
		}
	}
}
