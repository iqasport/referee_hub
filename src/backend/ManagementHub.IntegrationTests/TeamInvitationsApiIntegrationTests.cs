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
using ManagementHub.Models.Data;
using ManagementHub.Service.Areas.Ngbs;
using ManagementHub.Service.Areas.Teams;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ManagementHub.IntegrationTests;

public class TeamInvitationsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory factory;
	private readonly HttpClient client;

	public TeamInvitationsApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this.factory = factory;
		this.client = factory.CreateClient();
	}

	private async Task SetTeamAutoApprovePlayerRequestsAsync(string teamId, bool isEnabled)
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var toggleResponse = await this.client.PutAsJsonAsync(
			$"/api/v2/Teams/{teamId}/autoApprovePlayerRequests",
			new { isEnabled });

		toggleResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	private async Task SetNgbAutoApproveInternalTransfersAsync(bool isEnabled)
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var response = await this.client.PutAsJsonAsync(
			"/api/v2/Ngbs/USA/settings/transfers",
			new { autoApproveInternalTransfers = isEnabled });

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	private async Task RevokePendingTeamInvitesAsync(string teamId, string email)
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var managementResponse = await this.client.GetAsync($"/api/v2/Teams/{teamId}/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		foreach (var invitation in team!.PendingInvites.Where(invitation => invitation.Email == email))
		{
			var revokeResponse = await this.client.DeleteAsync($"/api/v2/Teams/{teamId}/invites/{invitation.InvitationId}");
			revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
		}
	}

	[Fact]
	public async Task InvitePlayer_AsTeamManager_ShouldCreatePendingInvite()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var response = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "invitee@example.com"
		});

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var invite = await response.Content.ReadFromJsonAsync<TeamInvitationViewModelDto>();
		invite.Should().NotBeNull();
		invite!.Email.Should().Be("invitee@example.com");

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_1/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().Contain(i => i.Email == "invitee@example.com");
	}

	[Fact]
	public async Task InvitePlayer_DuplicatePendingInvite_ShouldReturnBadRequest()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "duplicate@example.com"
		});

		var response = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "duplicate@example.com"
		});

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task InvitePlayer_ForExistingMember_ShouldReturnBadRequest()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var response = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "sarah.player@example.com"
		});

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task RevokeInvite_ShouldRemoveInviteFromManagementView()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var createResponse = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "revoke.me@example.com"
		});

		var invite = await createResponse.Content.ReadFromJsonAsync<TeamInvitationViewModelDto>();
		invite.Should().NotBeNull();

		var revokeResponse = await this.client.DeleteAsync($"/api/v2/Teams/TM_1/invites/{invite!.InvitationId}");
		revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_1/management");
		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team!.PendingInvites.Should().NotContain(i => i.Email == "revoke.me@example.com");
	}

	[Fact]
	public async Task InvitePlayer_AsRegularUser_ShouldReturnForbidden()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "sarah.player@example.com", "password");

		var response = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "blocked@example.com"
		});

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task GetNgbTransfers_WithPagination_ShouldReturnDistinctPagesAndMetadata()
	{
		using (var scope = this.factory.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
			var destinationTeam = dbContext.Teams.Single(team => team.Name == "Yankees");
			var originTeam = dbContext.Teams.Single(team => team.Name == "BA Jacks");
			var ngb = dbContext.NationalGoverningBodies.Single(item => item.CountryCode == "USA");
			var initiator = dbContext.Users.Single(user => user.Email == "team_manager@example.com");

			foreach (var index in Enumerable.Range(1, 2))
			{
				var createdAt = DateTime.UtcNow.AddMinutes(-index);
				dbContext.NgbTransferApprovals.Add(new NgbTransferApproval
				{
					TeamInvitation = new TeamInvitation
					{
						Team = destinationTeam,
						OriginTeam = originTeam,
						Email = $"pagination.transfer.{index}@example.test",
						Initiator = initiator,
						CreatedAt = createdAt,
						IsInternalTransfer = false,
					},
					Ngb = ngb,
					IsOriginNgb = false,
					CreatedAt = createdAt,
				});
			}

			dbContext.SaveChanges();
		}

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var firstResponse = await this.client.GetAsync("/api/v2/Ngbs/USA/transfers?filter=pagination.transfer&page=1&pageSize=1");
		var secondResponse = await this.client.GetAsync("/api/v2/Ngbs/USA/transfers?filter=pagination.transfer&page=2&pageSize=1");

		firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var firstPage = await firstResponse.Content.ReadFromJsonAsync<Filtered<NgbTransferViewModel>>();
		var secondPage = await secondResponse.Content.ReadFromJsonAsync<Filtered<NgbTransferViewModel>>();

		firstPage!.Metadata!.TotalCount.Should().Be(2);
		firstPage.Items.Should().ContainSingle();
		firstPage.Items.Single().Status.Should().Be(TransferApprovalStatus.PendingNgbApproval);
		firstPage.Items.Single().OriginNgbCode.Should().Be("ARG");
		firstPage.Items.Single().DestinationNgbCode.Should().Be("USA");
		secondPage!.Items.Should().ContainSingle();
		secondPage.Items.Single().Status.Should().Be(TransferApprovalStatus.PendingNgbApproval);
		secondPage.Items.Single().InvitationId.Should().NotBe(firstPage.Items.Single().InvitationId);
	}

	[Fact]
	public async Task RespondToInvite_Accept_ShouldCreateMembershipAndHistory()
	{
		this.factory.EmailSender.Clear();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var createResponse = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "ngb_admin@example.com"
		});

		createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
		var invite = await createResponse.Content.ReadFromJsonAsync<TeamInvitationViewModelDto>();
		invite.Should().NotBeNull();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().Contain(i =>
			NormalizeInvitationId(i.InvitationId) == NormalizeInvitationId(invite!.InvitationId)
			&& i.TeamId == "TM_1");

		var respondResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/users/me/teamInvites/{invite!.InvitationId}",
			new { Approved = true });

		respondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesAfterResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesAfterResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var pendingAfterResponse = await myInvitesAfterResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		pendingAfterResponse.Should().NotBeNull();
		pendingAfterResponse!.Should().NotContain(i => i.InvitationId == invite.InvitationId);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_1/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().NotContain(i => i.InvitationId == invite.InvitationId);
		team.Members.Should().Contain(m => m.Email == "ngb_admin@example.com");
		team.PlayerHistory.Should().Contain(a => a.ActivityType == "inviteAccepted" && a.Email == "ngb_admin@example.com");

		var sentEmails = this.factory.EmailSender.GetSentEmails();
		sentEmails.Should().Contain(e => e.Subject.Contains("Team Invitation accepted") && e.To.Contains("team_manager@example.com"));
	}

	private static string NormalizeInvitationId(string invitationId)
	{
		const string idPrefix = "TI_";
		return invitationId.StartsWith(idPrefix, System.StringComparison.OrdinalIgnoreCase)
			? invitationId.Substring(idPrefix.Length)
			: invitationId;
	}

	[Fact]
	public async Task RespondToInvite_Decline_ShouldCloseInviteAndRecordHistory()
	{
		this.factory.EmailSender.Clear();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var createResponse = await this.client.PostAsJsonAsync("/api/v2/Teams/TM_1/invites", new
		{
			Email = "team_manager@example.com"
		});

		createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
		var invite = await createResponse.Content.ReadFromJsonAsync<TeamInvitationViewModelDto>();
		invite.Should().NotBeNull();

		var respondResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/users/me/teamInvites/{invite!.InvitationId}",
			new { Approved = false });

		respondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().NotContain(i => i.InvitationId == invite.InvitationId);

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_1/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().NotContain(i => i.InvitationId == invite.InvitationId);
		team.PlayerHistory.Should().Contain(a => a.ActivityType == "inviteDeclined" && a.Email == "team_manager@example.com");

		var sentEmails = this.factory.EmailSender.GetSentEmails();
		sentEmails.Should().Contain(e => e.Subject.Contains("Team Invitation declined") && e.To.Contains("team_manager@example.com"));
	}

	[Fact]
	public async Task UpdateReferee_WithPlayingTeamRequest_ShouldCreatePendingManagerApprovalRequest()
	{
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var clearExistingTeamResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		clearExistingTeamResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var updateResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_2" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().ContainSingle(i => i.TeamId == "TM_2" && i.CanRespond == false);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_2/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().ContainSingle(i => i.Email == "referee@example.com" && i.RequiresManagerDecision);
	}

	[Fact]
	public async Task RespondToPendingInvite_Approve_ShouldAddMembershipAndClearPendingRequest()
	{
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var updateResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_2" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		var pendingRequest = myInvites!.Should().ContainSingle(i => i.TeamId == "TM_2").Subject;

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var approveResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/Teams/TM_2/invites/{pendingRequest.InvitationId}/response",
			new { Approved = true });

		approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_2/management");
		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().NotContain(i => i.InvitationId == pendingRequest.InvitationId);
		team.Members.Should().Contain(m => m.Email == "referee@example.com");
		team.PlayerHistory.Should().Contain(a => a.ActivityType == "inviteAccepted" && a.Email == "referee@example.com");

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var myInvitesAfterResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		var pendingAfterResponse = await myInvitesAfterResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		pendingAfterResponse.Should().NotContain(i => i.InvitationId == pendingRequest.InvitationId);

		var refereeProfileResponse = await this.client.GetAsync("/api/v2/Referees/me");
		refereeProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var refereeProfile = await refereeProfileResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
		refereeProfile.TryGetProperty("playingTeam", out var playingTeamProperty).Should().BeTrue();
		playingTeamProperty.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
		playingTeamProperty.GetProperty("id").GetString().Should().Be("TM_2");
	}

	[Fact]
	public async Task RespondToPendingInvite_Reject_ShouldClearPendingRequestWithoutMembership()
	{
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var updateResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_2" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		var pendingRequest = myInvites!.Should().ContainSingle(i => i.TeamId == "TM_2").Subject;

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var rejectResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/Teams/TM_2/invites/{pendingRequest.InvitationId}/response",
			new { Approved = false });

		rejectResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_2/management");
		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().NotContain(i => i.InvitationId == pendingRequest.InvitationId);
		team.Members.Should().NotContain(m => m.Email == "referee@example.com");
		team.PlayerHistory.Should().Contain(a => a.ActivityType == "inviteDeclined" && a.Email == "referee@example.com");

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var myInvitesAfterResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		var pendingAfterResponse = await myInvitesAfterResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		pendingAfterResponse.Should().NotContain(i => i.InvitationId == pendingRequest.InvitationId);

		var refereeProfileResponse = await this.client.GetAsync("/api/v2/Referees/me");
		refereeProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var refereeProfile = await refereeProfileResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
		refereeProfile.TryGetProperty("playingTeam", out var playingTeamProperty).Should().BeTrue();
		playingTeamProperty.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Null);
	}

	[Fact]
	public async Task UpdateReferee_WithPlayingAndCoachingRequests_ShouldKeepBothPendingInvites()
	{
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", false);
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var clearExistingTeamResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		clearExistingTeamResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var updateResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = new { id = "TM_2" },
			nationalTeam = (object?)null,
		});

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().Contain(i => i.TeamId == "TM_1" && i.Email == "referee@example.com");
		myInvites.Should().Contain(i => i.TeamId == "TM_2" && i.Email == "referee@example.com");
	}

	[Fact]
	public async Task GetMyTeamHistory_AfterApprovedTransfer_ShouldIncludeJoinAndLeaveActivities()
	{
		await this.RevokePendingTeamInvitesAsync("TM_1", "referee@example.com");
		await this.RevokePendingTeamInvitesAsync("TM_2", "referee@example.com");

		// Establish a known prior membership on TM_1 so the transfer to TM_2 always generates a playerRemoved entry.
		// Enabling auto-approve for TM_1 also bulk-approves any leftover pending TM_1 requests from prior tests.
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", true);
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		// Clear any existing playing-team membership so that RefereeTeams record is fully deleted.
		var clearResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});
		clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		// Join TM_1 via auto-approve — creates a fresh RefereeTeams player record for TM_1.
		var joinTm1Response = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});
		joinTm1Response.StatusCode.Should().Be(HttpStatusCode.OK);

		// Disable auto-approve on TM_1 so the TM_2 request below goes through the pending-approval flow.
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", false);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		const string targetTeamId = "TM_2";

		var requestResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = targetTeamId },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		var pendingRequest = myInvites!.Should().ContainSingle(i => i.TeamId == targetTeamId).Subject;

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");
		var notificationsResponse = await this.client.GetAsync("/api/v2/notifications");
		var notificationsJson = await notificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
		notificationsJson.GetProperty("notifications").EnumerateArray().Should().Contain(notification =>
			notification.GetProperty("type").GetString() == "NgbApprovalNeeded"
			&& notification.GetProperty("secondaryEntityId").GetString() == pendingRequest.InvitationId);

		var ngbApproveResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/Ngbs/USA/transfers/{pendingRequest.InvitationId}/approve",
			new { });

		ngbApproveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var approveResponse = await this.client.PostAsJsonAsync(
			$"/api/v2/Teams/{targetTeamId}/invites/{pendingRequest.InvitationId}/response",
			new { Approved = true });

		approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var historyResponse = await this.client.GetAsync("/api/v2/users/me/teamHistory");
		historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var history = await historyResponse.Content.ReadFromJsonAsync<List<TeamTransferHistoryItemViewModelDto>>();
		history.Should().NotBeNull();
		history!.Should().Contain(a => a.ActivityType == "inviteAccepted" && a.TeamId == targetTeamId);
		history.Should().Contain(a => a.ActivityType == "playerRemoved");
	}

	[Fact]
	public async Task UpdateReferee_WithAutoApprovedInternalTransfer_ShouldNotNotifyNgbAdmins()
	{
		await this.RevokePendingTeamInvitesAsync("TM_1", "referee@example.com");
		await this.RevokePendingTeamInvitesAsync("TM_2", "referee@example.com");
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", true);
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await this.SetNgbAutoApproveInternalTransfersAsync(true);

		try
		{
			await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
			var clearResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
			{
				primaryNgb = "USA",
				secondaryNgb = (string?)null,
				playingTeam = (object?)null,
				coachingTeam = (object?)null,
				nationalTeam = (object?)null,
			});
			clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

			var joinResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
			{
				primaryNgb = "USA",
				secondaryNgb = (string?)null,
				playingTeam = new { id = "TM_1" },
				coachingTeam = (object?)null,
				nationalTeam = (object?)null,
			});
			joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

			await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", false);
			await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
			var transferResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
			{
				primaryNgb = "USA",
				secondaryNgb = (string?)null,
				playingTeam = new { id = "TM_2" },
				coachingTeam = (object?)null,
				nationalTeam = (object?)null,
			});
			transferResponse.StatusCode.Should().Be(HttpStatusCode.OK);

			var invitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
			var invites = await invitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
			var transfer = invites!.Should().ContainSingle(invite => invite.TeamId == "TM_2").Subject;

			await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");
			var notificationsResponse = await this.client.GetAsync("/api/v2/notifications");
			var notificationsJson = await notificationsResponse.Content.ReadFromJsonAsync<JsonElement>();
			notificationsJson.GetProperty("notifications").EnumerateArray().Should().NotContain(notification =>
				notification.GetProperty("type").GetString() == "NgbApprovalNeeded"
				&& notification.GetProperty("secondaryEntityId").GetString() == transfer.InvitationId);
		}
		finally
		{
			await this.SetNgbAutoApproveInternalTransfersAsync(false);
		}
	}

	[Fact]
	public async Task UpdateReferee_WithExistingYankeesMembershipAndNoInviteHistory_ShouldCreatePendingManagerApprovalRequest()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var requestResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().Contain(i => i.TeamId == "TM_1" && i.Email == "referee@example.com" && i.CanRespond == false);
	}

	[Fact]
	public async Task UpdateReferee_WithYankeesRequest_ShouldAppearInYankeesTeamManagement()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var clearResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var requestResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().Contain(i => i.TeamId == "TM_1" && i.Email == "referee@example.com" && i.CanRespond == false);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "team_manager@example.com", "password");

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_1/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.PendingInvites.Should().Contain(i => i.Email == "referee@example.com" && i.RequiresManagerDecision);
	}

	[Fact]
	public async Task UpdateReferee_ClearPlayingTeamThenRejoinSameTeam_ShouldCreatePendingRequest()
	{
		// Ensure any prior TM_1 pending requests are resolved before the rejoin assertion.
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", true);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var initialJoinResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});
		initialJoinResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

		var clearResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});
		clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_1", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var rejoinResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});
		rejoinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var rejoinStatus = await rejoinResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
		rejoinStatus.GetProperty("playingTeam").GetProperty("teamId").GetString().Should().Be("TM_1");
		rejoinStatus.GetProperty("playingTeam").GetProperty("status").GetString().Should().Be("pending");
		rejoinStatus.GetProperty("playingTeam").GetProperty("requestCreated").GetBoolean().Should().BeTrue();

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().Contain(i => i.TeamId == "TM_1" && i.Email == "referee@example.com" && i.CanRespond == false);
	}

	[Fact]
	public async Task SetAutoApprovePlayerRequests_Enable_ShouldBulkApprovePendingRequests()
	{
		await this.SetTeamAutoApprovePlayerRequestsAsync("TM_2", false);
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var requestResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_2" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var toggleResponse = await this.client.PutAsJsonAsync(
			"/api/v2/Teams/TM_2/autoApprovePlayerRequests",
			new { isEnabled = true });

		toggleResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var managementResponse = await this.client.GetAsync("/api/v2/Teams/TM_2/management");
		managementResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var team = await managementResponse.Content.ReadFromJsonAsync<TeamManagementViewModelDto>();
		team.Should().NotBeNull();
		team!.AutoApprovePlayerRequests.Should().BeTrue();
		team.PendingInvites.Should().NotContain(i => i.Email == "referee@example.com");
		team.Members.Should().Contain(m => m.Email == "referee@example.com");
	}

	[Fact]
	public async Task UpdateReferee_WithAutoApproveEnabled_ShouldNotCreatePendingRequest()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "ngb_admin@example.com", "password");

		var toggleResponse = await this.client.PutAsJsonAsync(
			"/api/v2/Teams/TM_2/autoApprovePlayerRequests",
			new { isEnabled = true });

		toggleResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		// Clear any existing team assignment from prior tests to ensure a clean state
		var clearResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = (object?)null,
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});
		clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var updateResponse = await this.client.PutAsJsonAsync("/api/v2/Referees/me", new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_2" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null,
		});

		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var myInvitesResponse = await this.client.GetAsync("/api/v2/users/me/teamInvites");
		myInvitesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var myInvites = await myInvitesResponse.Content.ReadFromJsonAsync<List<CurrentUserTeamInviteViewModelDto>>();
		myInvites.Should().NotBeNull();
		myInvites!.Should().NotContain(i => i.TeamId == "TM_2" && i.Email == "referee@example.com");
	}
}