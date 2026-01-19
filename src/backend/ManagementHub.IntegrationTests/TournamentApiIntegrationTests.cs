using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.IntegrationTests.Models;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Tournaments;
using ManagementHub.Service.Filtering;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for Tournament API endpoints to verify EF Core dynamic logic.
/// Tests the full flow: sign in, create tournament, list tournaments, update tournament, get by ID.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class TournamentApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TournamentApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task Tournament_FullWorkflow_ShouldSucceed()
	{
		// Step 1: Sign in to get bearer token
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Step 2: Create a tournament with mock data
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test Country", "Test City", place: "Test Place");
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
		listedTournament.Type.Should().Be(TournamentType.Club);
		listedTournament.City.Should().Be("Test City");

		// Step 4: Edit the tournament
		var updateModel = new TournamentModel
		{
			Name = "Updated Test Tournament",
			Description = "Updated description",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(32)),
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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Create a private tournament
		var tournamentId = await this.CreateTestTournamentAsync("Private Tournament", TournamentType.Club, "Private Country", "Private City", isPrivate: true, place: "Private Place");

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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Step 2: Create a tournament
		var tournamentId = await this.CreateTestTournamentAsync("Manager Test Tournament", TournamentType.Club, "Test Country", "Test City", place: "Test Place");

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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// New manager should be able to list managers
		listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		listManagersResponse.StatusCode.Should().Be(HttpStatusCode.OK,
		"new manager should be able to list managers");
		managers = await listManagersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();

		// Step 7: Remove a manager (sign back in as original creator)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		listManagersResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/managers");
		listManagersResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden,
		"removed manager should not be able to access manager endpoints");
	}

	[Fact]
	public async Task AddTournamentManager_WithInvalidEmail_ShouldReturnBadRequest()
	{
		// Sign in and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test", "Test");

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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test", "Test");

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
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test", "Test");

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
	public async Task ContactTournamentOrganizers_PublicTournament_ShouldSucceed()
	{
		// Sign in as referee and create a public tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Public Contact Test", TournamentType.Club, "Test", "Test", isPrivate: false);

		// Sign in as a different user (ngb_admin) who is not a manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Send a contact message
		var contactResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/contact", new
		{
			message = "Hello, I have a question about this tournament."
		});

		contactResponse.StatusCode.Should().Be(HttpStatusCode.OK,
		"any authenticated user should be able to contact organizers of a public tournament");
	}

	[Fact]
	public async Task ContactTournamentOrganizers_PrivateTournament_OnlyInvolvedUsers_ShouldSucceed()
	{
		// Sign in as referee and create a private tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Private Contact Test", TournamentType.Club, "Test", "Test", isPrivate: true);

		// Add ngb_admin as a manager
		await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/managers", new
		{
			email = "ngb_admin@example.com"
		});

		// Sign in as ngb_admin (who is now a manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Send a contact message as manager (involved user)
		var contactResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/contact", new
		{
			message = "I'm a manager contacting other managers."
		});

		contactResponse.StatusCode.Should().Be(HttpStatusCode.OK,
		"involved users should be able to contact organizers of a private tournament");

		// Sign in as empty@example.com who is NOT involved
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "empty@example.com", "password");

		// Try to send a contact message (should fail)
		var unauthorizedContactResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/contact", new
		{
			message = "I'm not involved in this tournament."
		});

		unauthorizedContactResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
		"non-involved users should not be able to contact organizers of a private tournament");
	}

	[Fact]
	public async Task ContactTournamentOrganizers_WithEmptyMessage_ShouldReturnBadRequest()
	{
		// Sign in and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test", "Test");

		// Try to send an empty message
		var contactResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/contact", new
		{
			message = ""
		});

		contactResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
		"sending an empty message should return BadRequest");
	}

	[Fact]
	public async Task ContactTournamentOrganizers_WithTooLongMessage_ShouldReturnBadRequest()
	{
		// Sign in and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test", "Test");

		// Try to send a message that's too long (> 1000 characters)
		var longMessage = new string('a', 1001);
		var contactResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/contact", new
		{
			message = longMessage
		});

		contactResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
		"sending a message longer than 1000 characters should return BadRequest");
	}

	[Fact]
	public async Task AddTournamentManager_Idempotent_ShouldNotError()
	{
		// Sign in and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "Test", "Test");

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

	// Helper method to create a test tournament
	private async Task<string> CreateTestTournamentAsync(string name, TournamentType type, string country, string city, bool isPrivate = false, string? place = null)
	{
		var createModel = new TournamentModel
		{
			Name = name,
			Description = $"Test tournament for {name}",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(32)),
			Type = type,
			Country = country,
			City = city,
			Place = place,
			Organizer = "Test Organizer",
			IsPrivate = isPrivate
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var tournamentIdResponse = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		return tournamentIdResponse!.Id;
	}

	// Helper method to get Yankees team ID
	private async Task<string> GetYankeesTeamIdAsync()
	{
		var teamsResponse = await this._client.GetAsync("/api/v2/ngbs/USA/teams");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should be able to list teams");

		var teamsJson = await teamsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var teamsArray = teamsJson.GetProperty("items").EnumerateArray();
		var yankeesTeam = teamsArray.FirstOrDefault(t => t.GetProperty("name").GetString() == "Yankees");
		yankeesTeam.ValueKind.Should().NotBe(JsonValueKind.Undefined, "Yankees team should exist in seed data");

		var yankeesTeamId = yankeesTeam.GetProperty("teamId").GetString();
		yankeesTeamId.Should().NotBeNullOrEmpty();
		return yankeesTeamId!;
	}

	[Fact]
	public async Task Tournament_TournamentManagerInvitesTeam_TeamManagerAccepts_ShouldCreateParticipant()
	{
		// This test covers the scenario where a tournament manager invites a team,
		// then the team manager accepts the invite, resulting in the team becoming a participant.
		// According to phase-3-implementation.md, there is no separate endpoint for join vs invite - 
		// the same POST /invites endpoint is used by both tournament managers and team managers.

		// Step 1: Create tournament as referee (who becomes tournament manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Club Tournament 2024", TournamentType.Club, "USA", "New York");

		// Step 2: Get the Yankees team ID from seed data
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 3: As tournament manager, create invite for Yankees team
		// (Yankees is a Community team seeded in the database, managed by team_manager@example.com)
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId!  // Team ID obtained from NGB teams endpoint
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

		var createdInvite = await createInviteResponse.Content.ReadFromJsonAsync<JsonElement>();
		createdInvite.ValueKind.Should().NotBe(JsonValueKind.Undefined);
		createdInvite.GetProperty("status").GetString().Should().Be("pending",
		"invite should be pending because team manager hasn't approved yet");
		createdInvite.GetProperty("tournamentManagerApproval").GetProperty("status").GetString().Should().Be("approved",
		"tournament manager auto-approves when they create the invite");
		createdInvite.GetProperty("participantApproval").GetProperty("status").GetString().Should().Be("pending",
		"participant approval should be pending");

		// Step 4: Verify invites list shows the pending invite
		var invitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		invitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var invitesJson = await invitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		invitesJson.ValueKind.Should().Be(JsonValueKind.Array);
		var invitesList = invitesJson.EnumerateArray().ToList();
		invitesList.Should().HaveCount(1, "there should be one pending invite");
		invitesList[0].GetProperty("participantId").GetString().Should().Be(yankeesTeamId);

		// Step 5: Switch to team manager user and accept the invite
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var acceptModel = new InviteResponseModel { Approved = true };
		var acceptResponse = await this._client.PostAsJsonAsync(
		$"/api/v2/tournaments/{tournamentId}/invites/{yankeesTeamId}",
		acceptModel);
		acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK,
		"team manager should be able to accept invite");

		// Step 6: Verify the team is now a participant
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participantsJson = await participantsResponse.Content.ReadFromJsonAsync<JsonElement>();
		participantsJson.ValueKind.Should().Be(JsonValueKind.Array);
		var participantsList = participantsJson.EnumerateArray().ToList();
		participantsList.Should().HaveCount(1, "Yankees team should now be a participant");
		participantsList[0].GetProperty("teamId").GetString().Should().Be(yankeesTeamId);
		participantsList[0].GetProperty("teamName").GetString().Should().Be("Yankees");

		// Step 7: Verify the invite status is now approved
		var updatedInvitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		var updatedInvitesJson = await updatedInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		var updatedInvitesList = updatedInvitesJson.EnumerateArray().ToList();
		updatedInvitesList.Should().HaveCount(1);
		updatedInvitesList[0].GetProperty("status").GetString().Should().Be("approved", "both approvals are done");
		updatedInvitesList[0].GetProperty("tournamentManagerApproval").GetProperty("status").GetString().Should().Be("approved");
		updatedInvitesList[0].GetProperty("participantApproval").GetProperty("status").GetString().Should().Be("approved");
	}

	[Fact]
	public async Task Tournament_TeamManagerRequestsToJoin_TournamentManagerAccepts_ShouldCreateParticipant()
	{
		// This test covers the scenario where a team manager requests to join a tournament,
		// then the tournament manager accepts the request, resulting in the team becoming a participant.
		// According to phase-3-implementation.md, there is no separate endpoint for join request - 
		// the same POST /invites endpoint is used, but when initiated by a team manager it's a "join request".

		// Step 1: Create tournament as referee (who becomes tournament manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Club Tournament for Join Request", TournamentType.Club, "USA", "Boston");

		// Step 2: Switch to team manager and get their managed teams
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var managedTeamsResponse = await this._client.GetAsync("/api/v2/users/me/managedTeams");
		managedTeamsResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should be able to get managed teams");

		var managedTeams = await managedTeamsResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		managedTeams.Should().NotBeEmpty("team manager should have at least one managed team");

		var yankeesTeam = managedTeams!.FirstOrDefault(t => t.GetProperty("teamName").GetString() == "Yankees");
		yankeesTeam.ValueKind.Should().NotBe(JsonValueKind.Undefined, "team manager should manage Yankees team");

		var yankeesTeamId = yankeesTeam.GetProperty("teamId").GetString();
		yankeesTeamId.Should().NotBeNullOrEmpty("Yankees team should have an ID");

		var yankeesNgb = yankeesTeam.GetProperty("ngb").GetString();
		yankeesNgb.Should().Be("USA", "Yankees team should be part of USA NGB");

		// Step 3: Team manager requests to join (create invite from team side)
		var joinRequestModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId!  // Team ID obtained from managed teams endpoint
		};

		var joinRequestResponse = await this._client.PostAsJsonAsync(
		$"/api/v2/tournaments/{tournamentId}/invites",
		joinRequestModel);
		joinRequestResponse.StatusCode.Should().Be(HttpStatusCode.Created,
		"team manager should be able to request to join");

		var createdInvite = await joinRequestResponse.Content.ReadFromJsonAsync<JsonElement>();
		createdInvite.ValueKind.Should().NotBe(JsonValueKind.Undefined);
		createdInvite.GetProperty("status").GetString().Should().Be("pending",
		"invite should be pending because tournament manager hasn't approved yet");
		createdInvite.GetProperty("tournamentManagerApproval").GetProperty("status").GetString().Should().Be("pending",
		"tournament manager approval should be pending");
		createdInvite.GetProperty("participantApproval").GetProperty("status").GetString().Should().Be("approved",
		"team manager auto-approves when they create the join request");

		// Step 3: Team manager can see their own pending invite
		var teamManagerInvitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		if (teamManagerInvitesResponse.StatusCode != HttpStatusCode.OK)
		{
			var errorContent = await teamManagerInvitesResponse.Content.ReadAsStringAsync();
			Assert.Fail($"Team manager failed to get invites. Status: {teamManagerInvitesResponse.StatusCode}, Content: {errorContent}");
		}
		teamManagerInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var teamManagerInvitesJson = await teamManagerInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		var teamManagerInvitesList = teamManagerInvitesJson.EnumerateArray().ToList();
		teamManagerInvitesList.Should().HaveCount(1, "team manager should see their own invite");

		// Step 4: Switch to tournament manager and accept the request
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var approveModel = new InviteResponseModel { Approved = true };
		var approveResponse = await this._client.PostAsJsonAsync(
		$"/api/v2/tournaments/{tournamentId}/invites/{yankeesTeamId}",
		approveModel);
		approveResponse.StatusCode.Should().Be(HttpStatusCode.OK,
		"tournament manager should be able to approve join request");

		// Step 5: Verify the team is now a participant
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participantsJson = await participantsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var participantsList = participantsJson.EnumerateArray().ToList();
		participantsList.Should().HaveCount(1, "Yankees team should now be a participant");
		participantsList[0].GetProperty("teamId").GetString().Should().Be(yankeesTeamId);

		// Step 6: Verify the invite status is now approved
		var updatedInvitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");
		var updatedInvitesJson = await updatedInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		var updatedInvitesList = updatedInvitesJson.EnumerateArray().ToList();
		updatedInvitesList.Should().HaveCount(1);
		updatedInvitesList[0].GetProperty("status").GetString().Should().Be("approved", "both approvals are complete");
		updatedInvitesList[0].GetProperty("tournamentManagerApproval").GetProperty("status").GetString().Should().Be("approved");
		updatedInvitesList[0].GetProperty("participantApproval").GetProperty("status").GetString().Should().Be("approved");
	}

	[Fact]
	public async Task CreateTournament_ThenGetCurrentUser_ShouldSucceed()
	{
		// This test reproduces the issue where creating a tournament adds a TournamentManagerRole
		// to the user, but then fetching the user fails because TournamentManagerRole cannot be serialized

		// Step 1: Sign in to get bearer token
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Step 2: Verify we can get current user before creating a tournament
		var userResponseBefore = await this._client.GetAsync("/api/v2/users/me");
		userResponseBefore.StatusCode.Should().Be(HttpStatusCode.OK,
		"getting current user should succeed before creating tournament");

		var userBefore = await userResponseBefore.Content.ReadFromJsonAsync<JsonElement>();
		userBefore.GetProperty("userId").GetString().Should().NotBeNullOrEmpty();

		// Step 3: Create a tournament (this adds TournamentManagerRole to the user)
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

		// Step 4: Try to get current user again after creating tournament
		// This should succeed, but before the fix it fails because TournamentManagerRole cannot be serialized
		var userResponseAfter = await this._client.GetAsync("/api/v2/users/me");

		userResponseAfter.StatusCode.Should().Be(HttpStatusCode.OK,
		"getting current user should succeed after creating tournament (this is the bug we're fixing)");

		var userAfter = await userResponseAfter.Content.ReadFromJsonAsync<JsonElement>();
		userAfter.GetProperty("userId").GetString().Should().NotBeNullOrEmpty();

		// Verify the user now has roles including TournamentManager
		var roles = userAfter.GetProperty("roles").EnumerateArray().ToList();
		roles.Should().NotBeEmpty("user should have roles");

		// At least one role should be TournamentManager
		var hasTournamentManagerRole = roles.Any(r =>
		r.GetProperty("roleType").GetString() == "TournamentManager");
		hasTournamentManagerRole.Should().BeTrue(
		"user should have TournamentManager role after creating a tournament");
	}

	[Fact]
	public async Task Tournament_UnauthorizedUser_CannotSeeInvites()
	{
		// This test verifies that a user who is neither a tournament manager nor a team manager
		// cannot see tournament invites.

		// Step 1: Create tournament as referee (who becomes tournament manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament for Auth", TournamentType.Club, "USA", "Boston");

		// Step 2: Get Yankees team ID and create an invite
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId
		};

		var createInviteResponse = await this._client.PostAsJsonAsync(
		$"/api/v2/tournaments/{tournamentId}/invites",
		createInviteModel);
		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		// Step 3: Switch to ngb_admin (who is neither tournament manager nor team manager for Yankees)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Step 4: Try to get invites - should return forbidden or empty list
		var invitesResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/invites");

		// The endpoint should either return 403 Forbidden or an empty list (depends on authorization implementation)
		// Based on the code, it returns empty list for users who aren't authorized
		invitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var invitesJson = await invitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		var invitesList = invitesJson.EnumerateArray().ToList();
		invitesList.Should().BeEmpty("user who is not tournament manager or team manager should not see invites");
	}

	[Fact]
	public async Task Tournament_DeleteParticipant_ShouldSucceed()
	{
		// This test verifies that a tournament manager can remove a participant from a tournament.

		// Step 1: Create tournament and setup a participant (go through full invite/approve workflow)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Tournament for Participant Deletion", TournamentType.Club, "USA", "Seattle");

		// Step 2: Get Yankees team ID
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 3: Create invite as tournament manager
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId
		};

		var createInviteResponse = await this._client.PostAsJsonAsync(
		$"/api/v2/tournaments/{tournamentId}/invites",
		createInviteModel);
		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		// Step 4: Switch to team manager and approve the invite
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var approveModel = new InviteResponseModel { Approved = true };
		var approveResponse = await this._client.PostAsJsonAsync(
		$"/api/v2/tournaments/{tournamentId}/invites/{yankeesTeamId}",
		approveModel);
		approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// Step 5: Verify the team is now a participant
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participantsJson = await participantsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var participantsList = participantsJson.EnumerateArray().ToList();
		participantsList.Should().HaveCount(1, "Yankees should be a participant");

		// Step 6: Delete the participant
		var deleteResponse = await this._client.DeleteAsync($"/api/v2/tournaments/{tournamentId}/participants/{yankeesTeamId}");
		deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, "tournament manager should be able to remove participant");

		// Step 7: Verify the participant was removed
		var updatedParticipantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		updatedParticipantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var updatedParticipantsJson = await updatedParticipantsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var updatedParticipantsList = updatedParticipantsJson.EnumerateArray().ToList();
		updatedParticipantsList.Should().BeEmpty("participant should have been removed");
	}

	[Fact]
	public async Task ArchivedTournament_CannotAcceptInvite_ShouldReturnBadRequest()
	{
		// Step 1: Authenticate as referee and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var tournamentId = await this.CreateTestTournamentAsync("Tournament to be Archived", TournamentType.Club, "USA", "Chicago");

		// Step 2: Get Yankees team ID
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 3: Create an invite for the Yankees team
		var createInviteResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/invites", new
		{
			participantId = yankeesTeamId,
			participantType = "team"
		});
		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created, "invite creation should succeed");

		// Step 4: Archive the tournament by setting end date to the past
		var archiveModel = new TournamentModel
		{
			Name = "Tournament to be Archived",
			Description = "Test tournament for archived tournament",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-8)),
			Type = TournamentType.Club,
			Country = "USA",
			City = "Chicago",
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var archiveResponse = await this._client.PutAsJsonAsync($"/api/v2/tournaments/{tournamentId}", archiveModel);
		archiveResponse.StatusCode.Should().Be(HttpStatusCode.OK, "archiving tournament should succeed");

		// Step 5: Switch to team manager and try to approve the invite
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var approveResponse = await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournamentId}/invites/{yankeesTeamId}", new { });
		approveResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
		"cannot approve invite for archived tournament");

		var errorContent = await approveResponse.Content.ReadAsStringAsync();
		errorContent.Should().Contain("archived", "error message should indicate tournament is archived");
	}
}
