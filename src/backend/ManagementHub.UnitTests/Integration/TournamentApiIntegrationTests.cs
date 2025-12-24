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
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task Tournament_FullWorkflow_ShouldSucceed()
	{
		// Step 1: Sign in to get bearer token
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
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
		this._client.DefaultRequestHeaders.Authorization =
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

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"tournament creation should succeed");

		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		createResult.Should().NotBeNull();
		createResult!.Id.Should().NotBeNull("created tournament should have an ID");

		var tournamentId = createResult.Id;
		tournamentId.Should().StartWith("TR_", "tournament ID should have TR_ prefix");

		// Step 3: Get tournaments and check the tournament is there
		var listResponse = await this._client.GetAsync("/api/v2/tournaments");

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
		tournaments.Should().Contain(t => t.Id == tournamentId,
			"the created tournament with ID {0} should appear in the list", tournamentId);

		var listedTournament = tournaments!.First(t => t.Id == tournamentId);
		listedTournament.Name.Should().Be("Test Tournament");
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

		var updateResponse = await this._client.PutAsJsonAsync($"/api/v2/tournaments/{tournamentId}", updateModel);
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"tournament update should succeed");

		// Step 5: Get tournament by ID and check data was updated
		var getResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}");
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
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
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

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Verify it appears in the list (since creator is a manager)
		var listResponse = await this._client.GetAsync("/api/v2/tournaments");
		var tournamentsResponse = await listResponse.Content.ReadFromJsonAsync<Filtered<TournamentViewModelDto>>();
		var tournaments = tournamentsResponse!.Items.ToList();
		tournaments.Should().Contain(t => t.Id == tournamentId,
			"private tournament should be visible to its manager");

		// Verify it can be accessed by ID (since creator is a manager)
		var getResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"private tournament should be accessible to its manager");

		// Sign in as a different user (ngb_admin)
		var otherLoginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "ngb_admin@example.com",
			password = "password"
		});

		var otherLoginContent = await otherLoginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var otherToken = otherLoginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", otherToken);

		// Verify private tournament is NOT in the list for other user
		var otherListResponse = await this._client.GetAsync("/api/v2/tournaments");
		var otherTournamentsResponse = await otherListResponse.Content.ReadFromJsonAsync<Filtered<TournamentViewModelDto>>();
		var otherTournaments = otherTournamentsResponse!.Items.ToList();
		otherTournaments.Should().NotContain(t => t.Id == tournamentId,
			"private tournament should NOT be visible to non-managers");

		// Verify other user cannot access by ID (should get 404)
		var otherGetResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		otherGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
			"private tournament should NOT be accessible to non-managers");
	}

	[Fact]
	public async Task TournamentManagers_FullWorkflow_ShouldSucceed()
	{
		// Step 1: Sign in as referee (who will create the tournament)
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		// Step 2: Create a tournament
		var createModel = new TournamentModel
		{
			Name = "Manager Test Tournament",
			Description = "Test manager operations",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test Country",
			City = "Test City",
			Place = "Test Place",
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Step 3: List managers - should only show the creator
		var listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		listManagersResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"listing managers should succeed");

		var managers = await listManagersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		managers.Should().HaveCount(1, "should have exactly one manager (the creator)");
		managers![0].GetProperty("email").GetString().Should().Be("referee@example.com",
			"creator should be the first manager");

		// Step 4: Add another manager (ngb_admin)
		var addManagerResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/managers", new
		{
			email = "ngb_admin@example.com"
		});

		addManagerResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"adding a manager should succeed");

		// Step 5: List managers again - should show both
		listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		managers = await listManagersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		managers.Should().HaveCount(2, "should have two managers after adding one");

		var managerEmails = managers!.Select(m => m.GetProperty("email").GetString()).ToList();
		managerEmails.Should().Contain("referee@example.com", "original manager should still be present");
		managerEmails.Should().Contain("ngb_admin@example.com", "new manager should be added");

		// Step 6: Verify new manager can access manager endpoints
		// Sign in as ngb_admin
		var ngbLoginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "ngb_admin@example.com",
			password = "password"
		});

		var ngbLoginContent = await ngbLoginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var ngbToken = ngbLoginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ngbToken);

		// New manager should be able to list managers
		listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		listManagersResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"new manager should be able to list managers");
		managers = await listManagersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();

		// Step 7: Remove a manager (sign back in as original creator)
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		// Get the ngb_admin userId from the managers list
		var ngbAdminManager = managers!.First(m => m.GetProperty("email").GetString() == "ngb_admin@example.com");
		var ngbAdminUserId = ngbAdminManager.GetProperty("id").GetString();

		var removeManagerResponse = await this._client.DeleteAsync($"/api/v2/tournaments/{tournamentId}/managers/{ngbAdminUserId}");
		removeManagerResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"removing a manager should succeed");

		// Step 8: Verify manager was removed
		listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		managers = await listManagersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		managers.Should().HaveCount(1, "should have one manager after removing one");
		managers![0].GetProperty("email").GetString().Should().Be("referee@example.com",
			"only the original manager should remain");

		// Step 9: Verify removed manager cannot access manager endpoints
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ngbToken);

		listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		listManagersResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"removed manager should not be able to access manager endpoints");
	}

	[Fact]
	public async Task AddTournamentManager_WithInvalidEmail_ShouldReturnBadRequest()
	{
		// Sign in and create a tournament
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", new TournamentModel
		{
			Name = "Test Tournament",
			Description = "Test",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test",
			City = "Test",
			Organizer = "Test",
			IsPrivate = false
		});
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Try to add manager with invalid email
		var addResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/managers", new
		{
			email = "not-an-email"
		});

		addResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"adding manager with invalid email should return BadRequest");
	}

	[Fact]
	public async Task AddTournamentManager_WithNonexistentUser_ShouldReturnNotFound()
	{
		// Sign in and create a tournament
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", new TournamentModel
		{
			Name = "Test Tournament",
			Description = "Test",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test",
			City = "Test",
			Organizer = "Test",
			IsPrivate = false
		});
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Try to add manager with nonexistent user email
		var addResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/managers", new
		{
			email = "nonexistent@example.com"
		});

		addResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
			"adding nonexistent user as manager should return NotFound");
	}

	[Fact]
	public async Task RemoveTournamentManager_LastManager_ShouldReturnBadRequest()
	{
		// Sign in and create a tournament
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", new TournamentModel
		{
			Name = "Test Tournament",
			Description = "Test",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test",
			City = "Test",
			Organizer = "Test",
			IsPrivate = false
		});
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Get the current user's userId from the managers list
		var listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		var managers = await listManagersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		var currentUserId = managers![0].GetProperty("id").GetString();

		// Try to remove the only manager
		var removeResponse = await this._client.DeleteAsync($"/api/v2/tournaments/{tournamentId}/managers/{currentUserId}");

		removeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"removing the last manager should return BadRequest");

		var errorContent = await removeResponse.Content.ReadFromJsonAsync<JsonElement>();
		errorContent.GetProperty("error").GetString().Should().Contain("last manager",
			"error message should indicate cannot remove last manager");
	}

	[Fact]
	public async Task AddTournamentManager_Idempotent_ShouldNotError()
	{
		// Sign in and create a tournament
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", new TournamentModel
		{
			Name = "Test Tournament",
			Description = "Test",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test",
			City = "Test",
			Organizer = "Test",
			IsPrivate = false
		});
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Add a manager
		var addResponse1 = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/managers", new
		{
			email = "ngb_admin@example.com"
		});
		addResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

		// Add the same manager again (idempotent)
		var addResponse2 = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/managers", new
		{
			email = "ngb_admin@example.com"
		});
		addResponse2.StatusCode.Should().Be(HttpStatusCode.OK,
			"adding the same manager again should be idempotent and not error");

		// Verify still only 2 managers (not 3)
		var listResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		var managers = await listResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		managers.Should().HaveCount(2, "should still have only 2 managers after duplicate add");
	}

	[Fact]
	public async Task Tournament_InviteAndParticipant_Workflow_ShouldSucceed()
	{
		// Step 1: Sign in as tournament manager
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email = "referee@example.com",
			password = "password"
		});
		loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		// Step 2: Create a tournament
		var createModel = new TournamentModel
		{
			Name = "Invite Test Tournament",
			Description = "Testing invites",
			StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
			Type = TournamentType.Club,
			Country = "Test Country",
			City = "Test City",
			Place = null,
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		var createResult = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = createResult!.Id;

		// Step 3: Get participants (should be empty)
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var participants = await participantsResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		participants.Should().BeEmpty("new tournament should have no participants");

		// Step 4: Get invites - should work since user is a tournament manager
		var invitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");

		if (invitesResponse.StatusCode != HttpStatusCode.OK)
		{
			var errorContent = await invitesResponse.Content.ReadAsStringAsync();
			Console.WriteLine($"Error getting invites: {errorContent}");
		}

		invitesResponse.StatusCode.Should().Be(HttpStatusCode.OK, "tournament manager should be able to get invites");

		var invites = await invitesResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		invites.Should().BeEmpty("new tournament should have no invites");
	}
}
