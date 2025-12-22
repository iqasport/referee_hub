using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Enums;
using ManagementHub.Service;
using ManagementHub.Service.Areas.Tournaments;
using ManagementHub.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ManagementHub.UnitTests.Integration;

/// <summary>
/// Integration tests for Tournament API endpoints to verify EF Core dynamic logic.
/// Tests the full flow: sign in, create tournament, list tournaments, update tournament, get by ID.
/// </summary>
public class TournamentApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;

	public TournamentApiIntegrationTests(WebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = _factory.CreateClient();
	}

	[Fact]
	public async Task Tournament_FullWorkflow_ShouldSucceed()
	{
		// Step 1: Sign in to get bearer token
		var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});
		
		loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"authentication should succeed with valid credentials");
		
		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("token").GetString();
		token.Should().NotBeNullOrEmpty("login should return a bearer token");
		
		// Add bearer token to client for subsequent requests
		_client.DefaultRequestHeaders.Authorization = 
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		// Step 2: Create a tournament with mock data
		var createModel = new TournamentModel
		{
			Name = "Test Tournament",
			Description = "Integration test tournament",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test Country",
			City = "Test City",
			Place = "Test Place",
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var createResponse = await _client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"tournament creation should succeed");
		
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponse>();
		createResult.Should().NotBeNull();
		createResult!.Id.Should().NotBeNull("created tournament should have an ID");
		
		var tournamentId = createResult.Id.ToString();
		tournamentId.Should().StartWith("TR_", "tournament ID should have TR_ prefix");

		// Step 3: Get tournaments and check the tournament is there
		var listResponse = await _client.GetAsync("/api/v2/tournaments");
		listResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"listing tournaments should succeed");
		
		var tournaments = await listResponse.Content.ReadFromJsonAsync<List<TournamentViewModel>>();
		tournaments.Should().NotBeNull();
		tournaments.Should().ContainSingle(t => t.Name == "Test Tournament",
			"the created tournament should appear in the list");
		
		var listedTournament = tournaments!.First(t => t.Name == "Test Tournament");
		listedTournament.Description.Should().Be("Integration test tournament");
		listedTournament.Type.Should().Be(TournamentType.Club);
		listedTournament.City.Should().Be("Test City");

		// Step 4: Edit the tournament
		var updateModel = new TournamentModel
		{
			Name = "Updated Test Tournament",
			Description = "Updated description",
			StartDate = createModel.StartDate,
			EndDate = createModel.EndDate,
			Type = TournamentType.National,
			Country = "Updated Country",
			City = "Updated City",
			Place = "Updated Place",
			Organizer = "Updated Organizer",
			IsPrivate = false
		};

		var updateResponse = await _client.PutAsJsonAsync($"/api/v2/tournaments/{tournamentId}", updateModel);
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"tournament update should succeed");

		// Step 5: Get tournament by ID and check data was updated
		var getResponse = await _client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"getting tournament by ID should succeed");
		
		var updatedTournament = await getResponse.Content.ReadFromJsonAsync<TournamentViewModel>();
		updatedTournament.Should().NotBeNull();
		updatedTournament!.Name.Should().Be("Updated Test Tournament", 
			"tournament name should be updated");
		updatedTournament.Description.Should().Be("Updated description", 
			"tournament description should be updated");
		updatedTournament.Type.Should().Be(TournamentType.National, 
			"tournament type should be updated");
		updatedTournament.City.Should().Be("Updated City", 
			"tournament city should be updated");
		updatedTournament.Organizer.Should().Be("Updated Organizer", 
			"tournament organizer should be updated");
		
		// Verify IsCurrentUserInvolved is computed correctly (user is tournament creator/manager)
		updatedTournament.IsCurrentUserInvolved.Should().BeTrue(
			"creator should be marked as involved in the tournament");
	}

	[Fact]
	public async Task PrivateTournament_ShouldOnlyBeVisibleToManager()
	{
		// Sign in
		var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});
		
		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("token").GetString();
		_client.DefaultRequestHeaders.Authorization = 
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		// Create a private tournament
		var createModel = new TournamentModel
		{
			Name = "Private Tournament",
			Description = "Should only be visible to manager",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Private Country",
			City = "Private City",
			Place = "Private Place",
			Organizer = "Private Organizer",
			IsPrivate = true
		};

		var createResponse = await _client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponse>();
		var tournamentId = createResult!.Id.ToString();

		// Verify it appears in the list (since creator is a manager)
		var listResponse = await _client.GetAsync("/api/v2/tournaments");
		var tournaments = await listResponse.Content.ReadFromJsonAsync<List<TournamentViewModel>>();
		tournaments.Should().Contain(t => t.Id.ToString() == tournamentId,
			"private tournament should be visible to its manager");

		// Verify it can be accessed by ID (since creator is a manager)
		var getResponse = await _client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"private tournament should be accessible to its manager");
	}
}
