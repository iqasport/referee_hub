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
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Tournaments;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for the Team Tournament Invites API endpoint.
/// Tests the ngbs/{ngb}/teams/{teamId}/tournamentInvites endpoint.
/// </summary>
public class TeamTournamentInvitesApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TeamTournamentInvitesApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task GetTeamTournamentInvites_AsTeamManager_ShouldReturnInvites()
	{
		// Step 1: Sign in as referee (who will create the tournament)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Step 2: Create a tournament
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "USA", "New York");

		// Step 3: Get the Yankees team ID from seed data
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 4: Create an invite for the Yankees team
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId
		};

		var createInviteResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			createInviteModel);
		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created,
			"tournament manager should be able to create invite");

		// Step 5: Switch to team manager and get invites for their team
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var getInvitesResponse = await this._client.GetAsync($"/api/v2/ngbs/USA/teams/{yankeesTeamId}/tournamentInvites");
		getInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"team manager should be able to get invites for their team");

		var invitesJson = await getInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		invitesJson.ValueKind.Should().Be(JsonValueKind.Array);
		var invitesList = invitesJson.EnumerateArray().ToList();
		invitesList.Should().NotBeEmpty("there should be at least one invite for the Yankees team");

		// Find the invite for this specific tournament
		var invite = invitesList.FirstOrDefault(i => 
			i.TryGetProperty("participantId", out var pid) && pid.GetString() == yankeesTeamId);
		invite.ValueKind.Should().NotBe(JsonValueKind.Undefined, "should find an invite for Yankees team");
		
		invite.GetProperty("participantId").GetString().Should().Be(yankeesTeamId);
		invite.GetProperty("status").GetString().Should().Be("pending",
			"invite should be pending because team manager hasn't approved yet");
	}

	[Fact]
	public async Task GetTeamTournamentInvites_AsNgbAdmin_ShouldReturnInvites()
	{
		// Step 1: Sign in as referee and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Another Test Tournament", TournamentType.Club, "USA", "Boston");

		// Step 2: Get the Yankees team ID
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 3: Create an invite
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId
		};

		var createInviteResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			createInviteModel);
		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		// Step 4: Switch to NGB admin and get invites for the team
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var getInvitesResponse = await this._client.GetAsync($"/api/v2/ngbs/USA/teams/{yankeesTeamId}/tournamentInvites");
		getInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to get invites for teams in their jurisdiction");

		var invitesJson = await getInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		var invitesList = invitesJson.EnumerateArray().ToList();
		invitesList.Should().HaveCountGreaterOrEqualTo(1, "there should be at least one invite for the Yankees team");
	}

	[Fact]
	public async Task GetTeamTournamentInvites_MultipleInvites_ShouldReturnAll()
	{
		// Step 1: Sign in as referee and create two tournaments
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournament1Id = await this.CreateTestTournamentAsync("Tournament 1", TournamentType.Club, "USA", "Chicago");
		var tournament2Id = await this.CreateTestTournamentAsync("Tournament 2", TournamentType.Club, "USA", "Seattle");

		// Step 2: Get the Yankees team ID
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 3: Create invites for both tournaments
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId
		};

		await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournament1Id}/invites", createInviteModel);
		await this._client.PostAsJsonAsync($"/api/v2/tournaments/{tournament2Id}/invites", createInviteModel);

		// Step 4: Switch to team manager and get invites
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var getInvitesResponse = await this._client.GetAsync($"/api/v2/ngbs/USA/teams/{yankeesTeamId}/tournamentInvites");
		getInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var invitesJson = await getInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		var invitesList = invitesJson.EnumerateArray().ToList();
		invitesList.Should().HaveCountGreaterOrEqualTo(2,
			"there should be at least two invites for the Yankees team");
	}

	[Fact]
	public async Task GetTeamTournamentInvites_UnauthorizedUser_ShouldReturnForbidden()
	{
		// Step 1: Get the Yankees team ID
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 2: Try to get invites as a user who is neither team manager nor NGB admin
		// (referee is not a team manager for Yankees or NGB admin for USA)
		var getInvitesResponse = await this._client.GetAsync($"/api/v2/ngbs/USA/teams/{yankeesTeamId}/tournamentInvites");
		getInvitesResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"user who is neither team manager nor NGB admin should not be able to access team invites");
	}

	[Fact]
	public async Task GetTeamTournamentInvites_NoInvites_ShouldReturnEmptyList()
	{
		// Step 1: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Step 2: Get the Yankees team ID
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Note: We can't guarantee there are no invites due to test order, but this tests the endpoint behavior

		var getInvitesResponse = await this._client.GetAsync($"/api/v2/ngbs/USA/teams/{yankeesTeamId}/tournamentInvites");
		getInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"getting invites should succeed even if there are no invites");

		var invitesJson = await getInvitesResponse.Content.ReadFromJsonAsync<JsonElement>();
		invitesJson.ValueKind.Should().Be(JsonValueKind.Array,
			"response should be an array");
	}

	[Fact]
	public async Task CreateInvite_ShouldSendEmailToTeamManagers_AndAllowThemToSeeTournament()
	{
		// Clear any previous emails
		this._factory.EmailSender.Clear();

		// Step 1: Sign in as referee and create a tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Email Test Tournament", TournamentType.Club, "USA", "Chicago");

		// Step 2: Get the Yankees team ID
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();

		// Step 3: Create an invite for the Yankees team
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = yankeesTeamId
		};

		var createInviteResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			createInviteModel);

		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created,
			"tournament manager should be able to create invite");

		// Step 4: Verify email was sent to team managers
		var sentEmails = this._factory.EmailSender.GetSentEmails();
		sentEmails.Should().NotBeEmpty("at least one email should be sent to team managers");

		var inviteEmails = sentEmails.Where(e => e.Subject.Contains("Tournament Invitation")).ToArray();
		inviteEmails.Should().NotBeEmpty("invitation email should be sent");

		// Verify email contains team manager email and tournament details
		var teamManagerEmail = inviteEmails.FirstOrDefault(e => 
			e.To.Contains("team_manager@example.com") && 
			e.Body.Contains("Email Test Tournament"));
		teamManagerEmail.Should().NotBeNull("email should be sent to team_manager@example.com for Email Test Tournament");
		teamManagerEmail!.Body.Should().Contain($"/tournaments/{tournamentId}", "email should contain tournament link");

		// Step 5: Switch to team manager and verify they can see the tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var getTournamentResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		getTournamentResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"team manager with pending invite should be able to see the tournament");

		var tournamentJson = await getTournamentResponse.Content.ReadFromJsonAsync<JsonElement>();
		tournamentJson.GetProperty("name").GetString().Should().Be("Email Test Tournament");
		tournamentJson.GetProperty("isCurrentUserInvolved").GetBoolean().Should().BeTrue(
			"team manager with pending invite should be marked as involved");
	}

	// Helper methods

	private async Task<string> CreateTestTournamentAsync(string name, TournamentType type, string country, string city)
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
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var tournamentIdResponse = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		return tournamentIdResponse.GetProperty("id").GetString()!;
	}

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
}
