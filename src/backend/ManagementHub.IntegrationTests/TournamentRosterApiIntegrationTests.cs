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

		// Step 3: Get user IDs for roster
		var refereeUserId = await this.GetUserIdByEmailAsync("referee@example.com");
		var ngbAdminUserId = await this.GetUserIdByEmailAsync("ngb_admin@example.com");
		var teamManagerUserId = await this.GetUserIdByEmailAsync("team_manager@example.com");

		// Step 4: Update roster with players, coaches, and staff
		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel
				{
					UserId = refereeUserId,
					Number = "7",
					Gender = "Male"
				},
				new RosterPlayerModel
				{
					UserId = ngbAdminUserId,
					Number = "42",
					Gender = "Female"
				}
			},
			Coaches = new List<RosterStaffModel>
			{
				new RosterStaffModel { UserId = teamManagerUserId }
			},
			Staff = new List<RosterStaffModel>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"updating roster should succeed");

		// Step 5: Get participants list and verify roster was updated
		var participantsResponse = await this._client.GetAsync($"/api/v2/tournaments/{tournamentId}/participants");
		participantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var participants = await participantsResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
		participants.Should().NotBeNull();
		participants.Should().HaveCount(1);

		var participant = participants![0];
		var players = participant.GetProperty("players").EnumerateArray().ToList();
		var coaches = participant.GetProperty("coaches").EnumerateArray().ToList();

		players.Should().HaveCount(2, "should have 2 players");
		coaches.Should().HaveCount(1, "should have 1 coach");

		// Verify player details
		var player1 = players.FirstOrDefault(p => p.GetProperty("number").GetString() == "7");
		player1.ValueKind.Should().NotBe(JsonValueKind.Undefined);
		player1.GetProperty("userId").GetString().Should().Be(refereeUserId.ToString());
		player1.GetProperty("gender").GetString().Should().Be("Male");

		var player2 = players.FirstOrDefault(p => p.GetProperty("number").GetString() == "42");
		player2.ValueKind.Should().NotBe(JsonValueKind.Undefined);
		player2.GetProperty("userId").GetString().Should().Be(ngbAdminUserId.ToString());
		player2.GetProperty("gender").GetString().Should().Be("Female");
	}

	[Fact]
	public async Task UpdateRoster_WithDuplicateJerseyNumbers_ShouldReturnBadRequest()
	{
		// Step 1: Create tournament and add team
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Get user IDs
		var user1 = await this.GetUserIdByEmailAsync("referee@example.com");
		var user2 = await this.GetUserIdByEmailAsync("ngb_admin@example.com");

		// Step 3: Try to add players with duplicate jersey numbers
		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel { UserId = user1, Number = "10", Gender = "Male" },
				new RosterPlayerModel { UserId = user2, Number = "10", Gender = "Female" }
			},
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"duplicate jersey numbers should return BadRequest");
	}

	[Fact]
	public async Task UpdateRoster_WithNonTeamMember_ShouldReturnBadRequest()
	{
		// Step 1: Create tournament and add team
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Test Tournament", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: ngbAdmin is NOT on the Yankees team (only referee, playerSarah, coachMike are on Yankees)
		// We'll create a user ID manually since ngb_admin won't be in the team members API
		// Using a simple approach: ngbAdmin should have a database ID (legacy ID path)
		var ngbAdminUserId = UserIdentifier.FromLegacyUserId(2); // ngbAdmin is the 2nd user added in seed data

		// Step 3: Try to add non-team-member to roster
		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel { UserId = ngbAdminUserId, Number = "1", Gender = "Male" }
			},
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
		};

		var updateResponse = await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

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
		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>(),
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
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
		// Step 1: Create tournament in the past
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync(
			"Past Tournament",
			TournamentType.Club,
			"USA",
			"NYC",
			startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
			endDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));

		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		// Step 2: Try to update roster for past tournament
		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>(),
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
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

		var refereeUserId = await this.GetUserIdByEmailAsync("referee@example.com");

		// Step 2: Add referee to roster with gender
		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel { UserId = refereeUserId, Number = "10", Gender = "Male" }
			},
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 3: Get gender data
		var response = await this._client.GetAsync("/api/v2/users/me/gender");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var genderData = await response.Content.ReadFromJsonAsync<JsonElement>();
		genderData.GetProperty("gender").GetString().Should().Be("Male");

		var tournaments = genderData.GetProperty("referencedInTournaments").EnumerateArray().ToList();
		tournaments.Should().HaveCount(1, "user is in roster of 1 tournament");
		tournaments[0].GetProperty("id").GetString().Should().Be(tournamentId);
		tournaments[0].GetProperty("name").GetString().Should().Be("Gender Test Tournament");
	}

	[Fact]
	public async Task DeleteMyGender_ShouldSucceed()
	{
		// Step 1: Create tournament and add user to roster with gender
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync("Delete Gender Test", TournamentType.Club, "USA", "NYC");
		var yankeesTeamId = await this.GetYankeesTeamIdAsync();
		var participantId = await this.AddTeamToTournamentAsync(tournamentId, yankeesTeamId);

		var refereeUserId = await this.GetUserIdByEmailAsync("referee@example.com");

		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel { UserId = refereeUserId, Number = "5", Gender = "Male" }
			},
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 2: Verify gender exists
		var getResponse = await this._client.GetAsync("/api/v2/users/me/gender");
		var genderData = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
		genderData.GetProperty("gender").GetString().Should().Be("Male");

		// Step 3: Delete gender
		var deleteResponse = await this._client.DeleteAsync("/api/v2/users/me/gender");
		deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, "deleting gender should succeed");

		// Step 4: Verify gender is now null
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

		var ngbAdminUserId = await this.GetUserIdByEmailAsync("ngb_admin@example.com");

		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel { UserId = ngbAdminUserId, Number = "8", Gender = "Female" }
			},
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 2: As tournament manager (referee), get participants and verify gender is visible
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

		// Step 2: Add ngb_admin to roster
		var ngbAdminUserId = await this.GetUserIdByEmailAsync("ngb_admin@example.com");

		var updateRosterModel = new UpdateRosterModel
		{
			Players = new List<RosterPlayerModel>
			{
				new RosterPlayerModel { UserId = ngbAdminUserId, Number = "99", Gender = "Male" }
			},
			Coaches = new List<RosterStaffModel>(),
			Staff = new List<RosterStaffModel>()
		};

		await this._client.PutAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/participants/{participantId}/roster",
			updateRosterModel);

		// Step 3: Switch to ngb_admin (roster member) and verify they can access the tournament
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

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
		// Get user ID from team members list (Yankees team has all our test users)
		// Switch to a user who can access the team members endpoint
		var currentAuth = this._client.DefaultRequestHeaders.Authorization;
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members");
		response.StatusCode.Should().Be(HttpStatusCode.OK, "should be able to get team members");

		var membersResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
		var members = membersResponse.GetProperty("items").EnumerateArray();

		// Restore original auth
		this._client.DefaultRequestHeaders.Authorization = currentAuth;

		// Find the user by matching their first/last name
		var userMap = new Dictionary<string, (string firstName, string lastName)>
		{
			["referee@example.com"] = ("Jimmy", "Referee"),
			["ngb_admin@example.com"] = ("Jason", "NgbAdmin"),
			["team_manager@example.com"] = ("Tom", "TeamManager")
		};

		if (!userMap.TryGetValue(email, out var names))
		{
			throw new ArgumentException($"Unknown test user: {email}");
		}

		foreach (var member in members)
		{
			var name = member.GetProperty("name").GetString();
			if (name != null && (name.Contains(names.firstName) || name.Contains(names.lastName)))
			{
				var userId = member.GetProperty("userId").GetString();
				return UserIdentifier.Parse(userId!);
			}
		}

		throw new InvalidOperationException($"Could not find user {email} in team members");
	}
}
