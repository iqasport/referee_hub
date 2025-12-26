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
	public async Task Tournament_TournamentManagerInvitesTeam_TeamManagerAccepts_ShouldCreateParticipant()
	{
		// This test covers the scenario where a tournament manager invites a team,
		// then the team manager accepts the invite, resulting in the team becoming a participant.
		// According to phase-3-implementation.md, there is no separate endpoint for join vs invite - 
		// the same POST /invites endpoint is used by both tournament managers and team managers.

		// Step 1: Sign in as tournament manager (referee@example.com)
		await this.AuthenticateAsAsync("referee@example.com", "password");

		// Step 2: Create a tournament
		var createModel = new TournamentModel
		{
			Name = "Club Tournament 2024",
			Description = "Test tournament for invite workflow",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)),
			Type = TournamentType.Club,  // Club tournaments accept University/Community teams
			Country = "USA",
			City = "New York",
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK, "tournament creation should succeed");

		var tournamentIdResponse = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		tournamentIdResponse.Should().NotBeNull();
		var tournamentId = tournamentIdResponse!.Id;

		// Step 3: Get the Yankees team ID from the USA NGB teams list
		var teamsResponse = await this._client.GetAsync("/api/v2/ngbs/USA/teams");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should be able to list teams");
		
		var teamsJson = await teamsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var teamsArray = teamsJson.GetProperty("items").EnumerateArray();
		var yankeesTeam = teamsArray.FirstOrDefault(t => t.GetProperty("name").GetString() == "Yankees");
		yankeesTeam.ValueKind.Should().NotBe(JsonValueKind.Undefined, "Yankees team should exist in seed data");
		
		var yankeesTeamId = yankeesTeam.GetProperty("id").GetString();
		yankeesTeamId.Should().NotBeNullOrEmpty();

		// Step 4: As tournament manager, create invite for Yankees team
		// (Yankees is a Community team seeded in the database, managed by team_manager@example.com)
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId!  // TeamIdentifier for Yankees team
		};

		var createInviteResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites", 
			createInviteModel);
		
		if (createInviteResponse.StatusCode != HttpStatusCode.Created)
		{
			var errorContent = await createInviteResponse.Content.ReadAsStringAsync();
			createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created, 
				$"tournament manager should be able to create invite. Error: {errorContent}");
		}

		var createdInvite = await createInviteResponse.Content.ReadFromJsonAsync<TournamentInviteViewModel>();
		createdInvite.Should().NotBeNull();
		createdInvite!.Status.Should().Be(InviteStatus.Pending, 
			"invite should be pending because team manager hasn't approved yet");
		createdInvite.TournamentManagerApproval.Status.Should().Be(ApprovalStatus.Approved, 
			"tournament manager auto-approves when they create the invite");
		createdInvite.ParticipantApproval.Status.Should().Be(ApprovalStatus.Pending, 
			"participant approval should be pending");

		// Step 4: Verify invites list shows the pending invite
		var invitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		invitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var invites = await invitesResponse.Content.ReadFromJsonAsync<List<TournamentInviteViewModel>>();
		invites.Should().NotBeNull();
		invites!.Should().HaveCount(1, "there should be one pending invite");
		invites[0].ParticipantId.Should().Be("TM_1");

		// Step 5: Switch to team manager user and accept the invite
		await this.AuthenticateAsAsync("team_manager@example.com", "password");

		var acceptModel = new InviteResponseModel { Approved = true };
		var acceptResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites/TM_1",
			acceptModel);
		acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"team manager should be able to accept invite");

		// Step 6: Verify the team is now a participant
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participants = await participantsResponse.Content.ReadFromJsonAsync<List<TournamentParticipantViewModel>>();
		participants.Should().NotBeNull();
		participants!.Should().HaveCount(1, "Yankees team should now be a participant");
		participants[0].TeamId.ToString().Should().Be("TM_1");
		participants[0].TeamName.Should().Be("Yankees");

		// Step 7: Verify the invite status is now approved
		var updatedInvitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		var updatedInvites = await updatedInvitesResponse.Content.ReadFromJsonAsync<List<TournamentInviteViewModel>>();
		updatedInvites.Should().NotBeNull();
		updatedInvites!.Should().HaveCount(1);
		updatedInvites[0].Status.Should().Be(InviteStatus.Approved, "both approvals are done");
		updatedInvites[0].TournamentManagerApproval.Status.Should().Be(ApprovalStatus.Approved);
		updatedInvites[0].ParticipantApproval.Status.Should().Be(ApprovalStatus.Approved);
	}

	[Fact]
	public async Task Tournament_TeamManagerRequestsToJoin_TournamentManagerAccepts_ShouldCreateParticipant()
	{
		// This test covers the scenario where a team manager requests to join a tournament,
		// then the tournament manager accepts the request, resulting in the team becoming a participant.
		// According to phase-3-implementation.md, there is no separate endpoint for join request - 
		// the same POST /invites endpoint is used, but when initiated by a team manager it's a "join request".

		// Step 1: Create tournament as referee (who becomes tournament manager)
		await this.AuthenticateAsAsync("referee@example.com", "password");

		var createModel = new TournamentModel
		{
			Name = "Club Tournament for Join Request",
			Description = "Test tournament for join request workflow",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(17)),
			Type = TournamentType.Club,
			Country = "USA",
			City = "Boston",
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var tournamentIdResponse = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		var tournamentId = tournamentIdResponse!.Id;

		// Step 2: Switch to team manager and request to join (create invite from team side)
		await this.AuthenticateAsAsync("team_manager@example.com", "password");

		var joinRequestModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = "TM_1"  // Team manager manages Yankees (ID 1)
		};

		var joinRequestResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			joinRequestModel);
		joinRequestResponse.StatusCode.Should().Be(HttpStatusCode.Created, 
			"team manager should be able to request to join");

		var createdInvite = await joinRequestResponse.Content.ReadFromJsonAsync<TournamentInviteViewModel>();
		createdInvite.Should().NotBeNull();
		createdInvite!.Status.Should().Be(InviteStatus.Pending, 
			"invite should be pending because tournament manager hasn't approved yet");
		createdInvite.TournamentManagerApproval.Status.Should().Be(ApprovalStatus.Pending, 
			"tournament manager approval should be pending");
		createdInvite.ParticipantApproval.Status.Should().Be(ApprovalStatus.Approved, 
			"team manager auto-approves when they create the join request");

		// Step 3: Team manager can see their own pending invite
		var teamManagerInvitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		teamManagerInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var teamManagerInvites = await teamManagerInvitesResponse.Content.ReadFromJsonAsync<List<TournamentInviteViewModel>>();
		teamManagerInvites.Should().NotBeNull();
		teamManagerInvites!.Should().HaveCount(1, "team manager should see their own invite");

		// Step 4: Switch to tournament manager and accept the request
		await this.AuthenticateAsAsync("referee@example.com", "password");

		var approveModel = new InviteResponseModel { Approved = true };
		var approveResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites/TM_1",
			approveModel);
		approveResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
			"tournament manager should be able to approve join request");

		// Step 5: Verify the team is now a participant
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participants = await participantsResponse.Content.ReadFromJsonAsync<List<TournamentParticipantViewModel>>();
		participants.Should().NotBeNull();
		participants!.Should().HaveCount(1, "Yankees team should now be a participant");
		participants[0].TeamId.ToString().Should().Be("TM_1");

		// Step 6: Verify the invite status is now approved
		var updatedInvitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		var updatedInvites = await updatedInvitesResponse.Content.ReadFromJsonAsync<List<TournamentInviteViewModel>>();
		updatedInvites.Should().NotBeNull();
		updatedInvites!.Should().HaveCount(1);
		updatedInvites[0].Status.Should().Be(InviteStatus.Approved, "both approvals are complete");
		updatedInvites[0].TournamentManagerApproval.Status.Should().Be(ApprovalStatus.Approved);
		updatedInvites[0].ParticipantApproval.Status.Should().Be(ApprovalStatus.Approved);
	}

	private async Task AuthenticateAsAsync(string email, string password)
	{
		var loginResponse = await this._client.PostAsJsonAsync("/api/auth/login", new
		{
			email,
			password
		});
		loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"login for {email} should succeed");

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		this._client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
	}
}
