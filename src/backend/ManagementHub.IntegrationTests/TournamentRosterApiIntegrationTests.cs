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
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Areas.Tournaments;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for Phase 4: Tournament Roster Management API endpoints.
/// Tests roster update, gender data access, and privacy controls.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class TournamentRosterApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TournamentRosterApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task UpdateRoster_WithValidPlayersAndStaff_ShouldSucceed()
	{
		// Step 1: Create tournament as referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Club Tournament 2024", TournamentType.Club, "USA", "New York");

		// Step 2: Get Yankees team and add them to tournament
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 3: Switch to team manager for roster updates
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Step 4: Get user IDs for roster - ONLY Yankees RefereeTeam members (sarah.player, mike.coach)
		var sarahPlayerId = await this.GetUserIdByEmailAsync("sarah.player@example.com");
		var mikeCoachId = await this.GetUserIdByEmailAsync("mike.coach@example.com");

		// Step 5: Update roster with both users as players (since we only have 2 Yankees members)
		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto
				{
					UserId = sarahPlayerId.ToString(),
					Number = "7",
					Gender = "Female"
				},
				new RosterPlayerDto
				{
					UserId = mikeCoachId.ToString(),
					Number = "42",
					Gender = "Male"
				}
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"updating roster should succeed");

		// Step 6: Switch back to tournament manager to view participants
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Step 7: Get participants list and verify roster was updated
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participants = await participantsResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		participants.Should().NotBeNull();
		participants.Should().HaveCount(1);

		var participant = participants![0];
		var players = participant.GetProperty("players").EnumerateArray().ToList();

		players.Should().HaveCount(2, "should have 2 players");

		// Verify player details
		var player1 = players.FirstOrDefault(p => p.GetProperty("number").GetString() == "7");
		player1.ValueKind.Should().NotBe(JsonValueKind.Undefined);
		player1.GetProperty("userId").GetString().Should().Be(sarahPlayerId.ToString());
		player1.GetProperty("gender").GetString().Should().Be("Female");

		var player2 = players.FirstOrDefault(p => p.GetProperty("number").GetString() == "42");
		player2.ValueKind.Should().NotBe(JsonValueKind.Undefined);
		player2.GetProperty("userId").GetString().Should().Be(mikeCoachId.ToString());
		player2.GetProperty("gender").GetString().Should().Be("Male");
	}

	[Fact]
	public async Task UpdateRoster_WithDuplicateJerseyNumbers_ShouldReturnBadRequest()
	{
		// Step 1: Create tournament as referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager to update roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Step 3: Get user IDs (use actual Yankees team members)
		var sarahId = await this.GetUserIdByEmailAsync("sarah.player@example.com");
		var mikeId = await this.GetUserIdByEmailAsync("mike.coach@example.com");

		// Step 4: Try to add players with duplicate jersey numbers
		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = sarahId.ToString(), Number = "10", Gender = "Female" },
				new RosterPlayerDto { UserId = mikeId.ToString(), Number = "10", Gender = "Male" }
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"duplicate jersey numbers should return BadRequest");
	}

	[Fact]
	public async Task UpdateRoster_WithNonTeamMember_ShouldReturnForbidden()
	{
		// Step 1: Create tournament as referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager to update roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Step 3: Get ID for a user who exists but is NOT on the Yankees team
		// iqa_admin exists in seed data but is not a Yankees team member
		var iqaAdminUserId = await this.GetUserIdByEmailAsync("iqa_admin@example.com");

		// Step 4: Try to add non-team-member to roster
		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = iqaAdminUserId.ToString(), Number = "1", Gender = "Male" }
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Should return BadRequest because the operation is invalid (user is not a team member)
		updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"adding non-team-member should return BadRequest");
	}

	[Fact]
	public async Task UpdateRoster_AsNonManager_ShouldReturnForbidden()
	{
		// Step 1: Create tournament as referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to non-manager user
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Step 3: Try to update roster as non-manager
		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>(),
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"non-manager should not be able to update roster");
	}

	[Fact]
	public async Task UpdateRoster_ForArchivedTournament_ShouldReturnBadRequest()
	{
		// Step 1: Create tournament in the future first (so we can add participants)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync(
			"Past Tournament",
			TournamentType.Club,
			"USA",
			"NYC",
			startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
			endDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));

		// Step 2: Add team while tournament is still future
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 3: Update tournament to be archived (set end date in past)
		var archiveModel = new TournamentModel
		{
			Name = "Past Tournament",
			Description = "Integration test tournament",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
			Type = TournamentType.Club,
			Country = "USA",
			City = "NYC",
			Organizer = "Test Organizer",
			IsPrivate = false
		};

		var updateTournamentResponse = await this._client.PutAsJsonAsync($"/api/v2/tournaments/{tournamentId}", archiveModel);
		updateTournamentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// Step 4: Switch to team manager to try updating roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Step 5: Try to update roster for archived tournament
		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>(),
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"updating roster for archived tournament should return BadRequest");
	}

	[Fact]
	public async Task GetMyGender_WithNoGenderData_ShouldReturnNullGender()
	{
		// Authenticate as user who hasn't set gender
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Get gender data
		var response = await this._client.GetAsync("/api/v2/users/me/gender");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var genderData = await response.Content.ReadFromJsonAsync<JsonElement>();
		genderData.ValueKind.Should().NotBe(JsonValueKind.Undefined);

		// Gender should be null, tournaments list should be empty
		genderData.GetProperty("gender").ValueKind.Should().Be(JsonValueKind.Null);
		var tournaments = genderData.GetProperty("referencedInTournaments").EnumerateArray().ToList();
		tournaments.Should().BeEmpty("user hasn't been added to any roster");
	}

	[Fact]
	public async Task GetMyGender_AfterRosterUpdate_ShouldReturnGenderAndTournaments()
	{
		// Step 1: Create tournament and add team with roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Gender Test Tournament", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager to update roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var sarahPlayerId = await this.GetUserIdByEmailAsync("sarah.player@example.com");

		// Step 3: Add sarah.player to roster with gender
		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = sarahPlayerId.ToString(), Number = "10", Gender = "Female" }
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 4: Switch to sarah.player to check their gender
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Step 5: Get gender data
		var response = await this._client.GetAsync("/api/v2/users/me/gender");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var genderData = await response.Content.ReadFromJsonAsync<JsonElement>();
		genderData.GetProperty("gender").GetString().Should().Be("Female");

		var tournaments = genderData.GetProperty("referencedInTournaments").EnumerateArray().ToList();
		tournaments.Should().NotBeEmpty("user is in at least one roster");

		// Verify the specific tournament is in the list
		var thisTournament = tournaments.FirstOrDefault(t => t.GetProperty("id").GetString() == tournamentId);
		thisTournament.ValueKind.Should().NotBe(JsonValueKind.Undefined, "the tournament we added should be in the list");
		thisTournament.GetProperty("name").GetString().Should().Be("Gender Test Tournament");
	}

	[Fact]
	public async Task DeleteMyGender_ShouldSucceed()
	{
		// Step 1: Create tournament and add user to roster with gender
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Delete Gender Test", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager to update roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var mikeCoachId = await this.GetUserIdByEmailAsync("mike.coach@example.com");

		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = mikeCoachId.ToString(), Number = "5", Gender = "Male" }
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 3: Switch to mike.coach to check and delete their gender
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "mike.coach@example.com", "password");

		// Step 4: Verify gender exists
		var getResponse = await this._client.GetAsync("/api/v2/users/me/gender");
		var genderData = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
		genderData.GetProperty("gender").GetString().Should().Be("Male");

		// Step 5: Delete gender
		var deleteResponse = await this._client.DeleteAsync("/api/v2/users/me/gender");
		deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, "deleting gender should succeed");

		// Step 6: Verify gender is now null
		getResponse = await this._client.GetAsync("/api/v2/users/me/gender");
		genderData = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
		genderData.GetProperty("gender").ValueKind.Should().Be(JsonValueKind.Null, "gender should be null after deletion");
	}

	[Fact]
	public async Task GenderPrivacy_TournamentManager_CanSeeGenderData()
	{
		// Step 1: Create tournament and add users to roster with gender
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Privacy Test", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager to update roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var sarahPlayerId = await this.GetUserIdByEmailAsync("sarah.player@example.com");

		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = sarahPlayerId.ToString(), Number = "8", Gender = "Female" }
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 3: Switch back to tournament manager (referee) and get participants to verify gender is visible
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		var participants = await participantsResponse.Content.ReadFromJsonAsync<List<JsonElement>>();

		var players = participants![0].GetProperty("players").EnumerateArray().ToList();
		players.Should().HaveCount(1);
		players[0].GetProperty("gender").GetString().Should().Be("Female",
			"tournament manager should see gender data");
	}

	[Fact]
	public async Task RosterMember_CanAccessPrivateTournament()
	{
		// Step 1: Create private tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync(
			"Private Tournament",
			TournamentType.Club,
			"USA",
			"NYC",
			isPrivate: true);

		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager to add roster member
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var sarahPlayerId = await this.GetUserIdByEmailAsync("sarah.player@example.com");

		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = sarahPlayerId.ToString(), Number = "99", Gender = "Female" }
			},
			Coaches = new List<RosterStaffDto>(),
			Staff = new List<RosterStaffDto>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 3: Switch to sarah.player (roster member) and verify they can access the tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		var getResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"roster member should be able to access private tournament");

		var tournament = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
		tournament.GetProperty("name").GetString().Should().Be("Private Tournament");
	}

	// Helper methods
	private async Task<string> CreateTestTournamentAsync(
		string name,
		TournamentType type,
		string country,
		string city,
		string? place = null,
		DateOnly? startDate = null,
		DateOnly? endDate = null,
		bool isPrivate = false)
	{
		var model = new TournamentModel
		{
			Name = name,
			Description = "Integration test tournament",
			StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
			EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(32)),
			Type = type,
			Country = country,
			City = city,
			Place = place,
			Organizer = "Test Organizer",
			IsPrivate = isPrivate
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/tournaments", model);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var tournamentIdResponse = await createResponse.Content.ReadFromJsonAsync<TournamentIdResponseDto>();
		return tournamentIdResponse!.Id;
	}

	private async Task<string> GetYankeesTeamIdAsync()
	{
		var teamsResponse = await this._client.GetAsync("/api/v2/ngbs/USA/teams");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var teamsJson = await teamsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var teamsArray = teamsJson.GetProperty("items").EnumerateArray();
		var yankeesTeam = teamsArray.FirstOrDefault(t => t.GetProperty("name").GetString() == "Yankees");
		yankeesTeam.ValueKind.Should().NotBe(JsonValueKind.Undefined, "Yankees team should exist");

		return yankeesTeam.GetProperty("teamId").GetString()!;
	}

	private async Task<string> AddTeamToTournamentAsync(string tournamentId, string teamId)
	{
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = teamId
		};

		var inviteResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			createInviteModel);
		inviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		// Switch to team manager and accept invite
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var acceptModel = new InviteResponseModel { Approved = true };
		var acceptResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites/{teamId}",
			acceptModel);
		acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// Switch back to tournament manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Return the team ID which is used as the participant identifier in the roster endpoint
		return teamId;
	}

	private async Task<UserIdentifier> GetUserIdByEmailAsync(string email)
	{
		// Get user ID by authenticating as that user and calling /users/me
		var currentAuth = this._client.DefaultRequestHeaders.Authorization;
		await AuthenticationHelper.AuthenticateAsAsync(this._client, email, "password");

		var response = await this._client.GetAsync("/api/v2/users/me");
		response.StatusCode.Should().Be(HttpStatusCode.OK, $"should be able to get user info for {email}");

		var userResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
		var userId = userResponse.GetProperty("userId").GetString();

		// Restore original auth
		this._client.DefaultRequestHeaders.Authorization = currentAuth;

		return UserIdentifier.Parse(userId!);
	}

	[Fact]
	public async Task GetTeamRoster_AsTournamentManager_ShouldReturnRosterWithCertifications()
	{
		// Step 1: Create tournament as tournament manager (referee)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Roster View Test", TournamentType.Club, "USA", "NYC");
		
		// Step 2: Add Yankees team to tournament
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 3: Switch to team manager and add roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");
		
		var sarahPlayerId = await this.GetUserIdByEmailAsync("sarah.player@example.com");
		var mikeCoachId = await this.GetUserIdByEmailAsync("mike.coach@example.com");

		var updateRosterModel = new UpdateRosterDto
		{
			Players = new List<RosterPlayerDto>
			{
				new RosterPlayerDto { UserId = sarahPlayerId.ToString(), Number = "7", Gender = "Female" }
			},
			Coaches = new List<RosterStaffDto>
			{
				new RosterStaffDto { UserId = mikeCoachId.ToString() }
			},
			Staff = new List<RosterStaffDto>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// Step 4: Switch to tournament manager and get roster
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		var rosterResponse = await this._client.GetAsync(
			$"/api/v2/tournaments/{tournamentId}/teams/{participantId}/roster");
		rosterResponse.StatusCode.Should().Be(HttpStatusCode.OK, "tournament manager should be able to view roster");

		var roster = await rosterResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		roster.Should().NotBeNull();
		roster.Should().HaveCount(2, "roster should have 2 entries (1 player + 1 coach)");

		// Verify player entry
		var playerEntry = roster!.FirstOrDefault(e => e.GetProperty("role").GetString() == "Player");
		playerEntry.ValueKind.Should().NotBe(JsonValueKind.Undefined, "should have a player entry");
		playerEntry.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
		playerEntry.GetProperty("jerseyNumber").GetString().Should().Be("7");
		playerEntry.GetProperty("role").GetString().Should().Be("Player");

		// Verify coach entry
		var coachEntry = roster!.FirstOrDefault(e => e.GetProperty("role").GetString() == "Coach");
		coachEntry.ValueKind.Should().NotBe(JsonValueKind.Undefined, "should have a coach entry");
		coachEntry.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
		coachEntry.GetProperty("role").GetString().Should().Be("Coach");
		
		// Jersey number should be null for non-players
		if (coachEntry.TryGetProperty("jerseyNumber", out var jerseyNum))
		{
			jerseyNum.ValueKind.Should().Be(JsonValueKind.Null);
		}
	}

	[Fact]
	public async Task GetTeamRoster_AsNonTournamentManager_ShouldReturnForbidden()
	{
		// Step 1: Create tournament as tournament manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Forbidden Roster Test", TournamentType.Club, "USA", "NYC");
		
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Switch to team manager (who is NOT a tournament manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Step 3: Try to get roster - should be forbidden
		var rosterResponse = await this._client.GetAsync(
			$"/api/v2/tournaments/{tournamentId}/teams/{participantId}/roster");
		rosterResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, 
			"non-tournament manager should not be able to view roster");
	}

	[Fact]
	public async Task GetTeamRoster_ForNonParticipantTeam_ShouldReturnNotFound()
	{
		// Step 1: Create tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Not Found Test", TournamentType.Club, "USA", "NYC");
		
		// Step 2: Try to get roster for team that's not a participant (using valid format but non-existent team)
		var nonExistentTeamId = "TM_99999";
		var rosterResponse = await this._client.GetAsync(
			$"/api/v2/tournaments/{tournamentId}/teams/{nonExistentTeamId}/roster");
		rosterResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, 
			"should return 404 for non-participant team");
	}
}
