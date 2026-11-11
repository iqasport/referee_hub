using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
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

		var createInviteModel = new
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

	[Theory]
	[InlineData(true, "RequestAccepted")]
	[InlineData(false, "RequestRejected")]
	public async Task IndividualRegistration_ShouldNotifyManagersAndApplicantOnDecision(
		bool approved,
		string expectedResponseType)
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var tournamentName = $"Individual Registration Notification {Guid.NewGuid():N}";
		var createResponse = await this.client.PostAsJsonAsync("/api/v2/tournaments", new TournamentModel
		{
			Name = tournamentName,
			Description = "Individual registration notification lifecycle",
			StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(32)),
			Type = TournamentType.Fantasy,
			Country = "USA",
			City = "Portland",
			Organizer = "Test Organizer",
			IsPrivate = false,
			AllowsTeamRegistration = true,
			AllowsIndividualRegistration = true,
		});
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		var tournamentId = createBody.GetProperty("id").GetString();
		tournamentId.Should().NotBeNullOrWhiteSpace();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");
		var currentUserResponse = await this.client.GetAsync("/api/v2/users/me");
		currentUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var currentUser = await currentUserResponse.Content.ReadFromJsonAsync<JsonElement>();
		var playerId = currentUser.GetProperty("userId").GetString();
		playerId.Should().NotBeNullOrWhiteSpace();

		var applicationResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites",
			new CreateInviteModel
			{
				ParticipantType = ParticipantType.Player,
				ParticipantId = TournamentParticipantIdentifier.ForPlayer(UserIdentifier.Parse(playerId!)),
			});
		applicationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var managerNotificationsResponse = await this.client.GetAsync("/api/v2/notifications");
		managerNotificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var managerNotificationsJson = await managerNotificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var applicationNotification = managerNotificationsJson.GetProperty("notifications").EnumerateArray().FirstOrDefault(notification =>
			notification.GetProperty("title").GetString() == "New individual application" &&
			notification.GetProperty("relatedEntityId").GetString() == tournamentId);
		applicationNotification.ValueKind.Should().NotBe(JsonValueKind.Undefined);

		var decisionResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/tournaments/{tournamentId}/invites/{playerId}?participantType=player",
			new InviteResponseModel { Approved = approved });
		decisionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");
		var applicantNotificationsResponse = await this.client.GetAsync("/api/v2/notifications");
		applicantNotificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var applicantNotificationsJson = await applicantNotificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
		var decisionNotification = applicantNotificationsJson.GetProperty("notifications").EnumerateArray().FirstOrDefault(notification =>
			notification.GetProperty("type").GetString() == expectedResponseType &&
			notification.GetProperty("relatedEntityId").GetString() == tournamentId);
		decisionNotification.ValueKind.Should().NotBe(JsonValueKind.Undefined);
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
		var createInviteModel = new
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