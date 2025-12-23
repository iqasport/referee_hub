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
using ManagementHub.Service.Filtering;
using ManagementHub.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ManagementHub.UnitTests.Integration;

/// <summary>
/// Test DTO for deserializing tournament ID response with string ID.
/// </summary>
public class TournamentIdResponseDto
{
	public required string Id { get; set; }
}

/// <summary>
/// Test DTO for deserializing tournament view models with string ID (since JSON serializer converts TournamentIdentifier to string).
/// </summary>
public class TournamentViewModelDto
{
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required DateOnly StartDate { get; set; }
	public required DateOnly EndDate { get; set; }
	public required TournamentType Type { get; set; }
	public required string Country { get; set; }
	public required string City { get; set; }
	public string? Place { get; set; }
	public required string Organizer { get; set; }
	public required bool IsPrivate { get; set; }
	public string? BannerImageUrl { get; set; }
	public bool IsCurrentUserInvolved { get; set; }
}

/// <summary>
/// Custom WebApplicationFactory that explicitly calls Program's CreateWebHostBuilder.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override IWebHostBuilder CreateWebHostBuilder()
	{
		// Explicitly call your Program's CreateWebHostBuilder with empty args
		return Program.CreateWebHostBuilder(Array.Empty<string>());
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Development");
		
		// Add any test-specific configuration overrides here
		builder.ConfigureServices(services =>
		{
			// Override services for testing if needed
		});
	}
	
}

/// <summary>
/// Integration tests for Tournament API endpoints to verify EF Core dynamic logic.
/// Tests the full flow: sign in, create tournament, list tournaments, update tournament, get by ID.
/// </summary>
public class TournamentApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly CustomWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TournamentApiIntegrationTests(CustomWebApplicationFactory factory)
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
		var token = loginContent.GetProperty("accessToken").GetString();
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
		
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		createResult.Should().NotBeNull();
		createResult!.Id.Should().NotBeNull("created tournament should have an ID");
		
		var tournamentId = createResult.Id;
		tournamentId.Should().StartWith("TR_", "tournament ID should have TR_ prefix");

		// Step 3: Get tournaments and check the tournament is there
		var listResponse = await _client.GetAsync("/api/v2/tournaments");
		
		if (listResponse.StatusCode != HttpStatusCode.OK)
		{
			var errorContent = await listResponse.Content.ReadAsStringAsync();
			throw new Exception($"List tournaments returned {listResponse.StatusCode}: {errorContent}");
		}
		
		listResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"listing tournaments should succeed");
		
		var tournamentsResponse = await listResponse.Content.ReadFromJsonAsync<Filtered<TournamentViewModelDto>>();
		tournamentsResponse.Should().NotBeNull();
		var tournaments = tournamentsResponse!.Items.ToList();
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
		
		var updatedTournament = await getResponse.Content.ReadFromJsonAsync<TournamentViewModelDto>();
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
		// Sign in as referee (who will create the tournament)
		var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});
		
		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		_client.DefaultRequestHeaders.Authorization = 
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		// Create a private tournament as referee user
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
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Verify it appears in the list (since creator is a manager)
		var listResponse = await _client.GetAsync("/api/v2/tournaments");
		var tournamentsResponse = await listResponse.Content.ReadFromJsonAsync<Filtered<TournamentViewModelDto>>();
		var tournaments = tournamentsResponse!.Items.ToList();
		tournaments.Should().Contain(t => t.Id == tournamentId,
			"private tournament should be visible to its manager");

		// Verify it can be accessed by ID (since creator is a manager)
		var getResponse = await _client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"private tournament should be accessible to its manager");
		
		// Sign in as a different user (ngb_admin)
		var otherLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "ngb_admin@example.com",
			password = "password"
		});
		
		var otherLoginContent = await otherLoginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var otherToken = otherLoginContent.GetProperty("accessToken").GetString();
		_client.DefaultRequestHeaders.Authorization = 
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherToken);
		
		// Verify private tournament is NOT in the list for other user
		var otherListResponse = await _client.GetAsync("/api/v2/tournaments");
		var otherTournamentsResponse = await otherListResponse.Content.ReadFromJsonAsync<Filtered<TournamentViewModelDto>>();
		var otherTournaments = otherTournamentsResponse!.Items.ToList();
		otherTournaments.Should().NotContain(t => t.Id == tournamentId,
			"private tournament should NOT be visible to non-managers");
		
		// Verify other user cannot access by ID (should get 404)
		var otherGetResponse = await _client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		otherGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
			"private tournament should NOT be accessible to non-managers");
	}
}
