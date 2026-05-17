using System;
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

public class NotificationsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient client;

	public NotificationsApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this.client = factory.CreateClient();
	}

	[Fact]
	public async Task CreateInvite_AsTeamManager_ShouldNotifyTournamentManagersOfJoinRequest()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync(
			"Case 7 Notification Tournament",
			TournamentType.Club,
			"USA",
			"Austin");

		var teamId = await this.GetYankeesTeamIdAsync();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = teamId,
		};

		var createInviteResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			createInviteModel);

		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created,
			"team manager should be able to create a join request for their team");

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var notificationsResponse = await this.client.GetAsync("/api/v2/notifications");
		notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var notificationsJson = await notificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var notifications = notificationsJson.GetProperty("notifications").EnumerateArray().ToList();

		var joinRequestNotification = notifications.FirstOrDefault(n =>
			n.TryGetProperty("type", out var type) &&
			type.GetString() == "TeamTournamentJoinRequest" &&
			n.TryGetProperty("relatedEntityId", out var relatedEntityId) &&
			relatedEntityId.GetString() == tournamentId &&
			n.TryGetProperty("secondaryEntityId", out var secondaryEntityId) &&
			secondaryEntityId.GetString() == teamId);

		joinRequestNotification.ValueKind.Should().NotBe(JsonValueKind.Undefined,
			"tournament managers should receive a join request notification for this tournament/team");
	}

	[Fact]
	public async Task NotificationsApi_MarkAsRead_ShouldUpdateNotificationState()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var tournamentId = await this.CreateTestTournamentAsync(
			"Mark Read Notification Tournament",
			TournamentType.Club,
			"USA",
			"Denver");

		var teamId = await this.GetYankeesTeamIdAsync();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");
		var createInviteModel = new CreateInviteModel
		{
			ParticipantType = ParticipantType.Team,
			ParticipantId = teamId,
		};
		var createInviteResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			createInviteModel);
		createInviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var notificationsResponse = await this.client.GetAsync("/api/v2/notifications");
		notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var notificationsJson = await notificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var notifications = notificationsJson.GetProperty("notifications").EnumerateArray().ToList();

		var targetNotification = notifications.FirstOrDefault(n =>
			n.TryGetProperty("type", out var type) &&
			type.GetString() == "TeamTournamentJoinRequest" &&
			n.TryGetProperty("relatedEntityId", out var relatedEntityId) &&
			relatedEntityId.GetString() == tournamentId &&
			n.TryGetProperty("secondaryEntityId", out var secondaryEntityId) &&
			secondaryEntityId.GetString() == teamId);

		targetNotification.ValueKind.Should().NotBe(JsonValueKind.Undefined);

		var notificationId = targetNotification.GetProperty("id").GetString();
		notificationId.Should().NotBeNullOrWhiteSpace();

		var markReadResponse = await this.client.PatchAsync($"/api/v2/notifications/{notificationId}/read", null);
		markReadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var markedNotification = await markReadResponse.Content.ReadFromJsonAsync<JsonElement>();
		markedNotification.GetProperty("id").GetString().Should().Be(notificationId);
		markedNotification.GetProperty("isRead").GetBoolean().Should().BeTrue();
	}

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
			IsPrivate = false,
		};

		var createResponse = await this.client.PostAsJsonAsync("/api/v2/tournaments", createModel);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var tournamentIdResponse = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		return tournamentIdResponse.GetProperty("id").GetString()!;
	}

	private async Task<string> GetYankeesTeamIdAsync()
	{
		var teamsResponse = await this.client.GetAsync("/api/v2/ngbs/USA/teams");
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