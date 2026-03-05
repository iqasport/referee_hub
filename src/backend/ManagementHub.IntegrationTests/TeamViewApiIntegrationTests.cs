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
using ManagementHub.Service.Filtering;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for Team View API endpoints.
/// Tests team detail view, logo upload, and team management features.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class TeamViewApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TeamViewApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task GetTeamDetails_AsNgbAdmin_ShouldReturnTeamDetails()
	{
		// Arrange: Sign in as NGB admin for USA
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get teams belonging to the USA NGB (ngb_admin@example.com is a USA NGB admin)
		var teamsResponse = await this._client.GetAsync("/api/v2/Ngbs/USA/teams?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		teamsResult.Should().NotBeNull();
		teamsResult!.Items.Should().NotBeEmpty();

		var firstTeam = teamsResult.Items!.First();

		// Act: Get team details for a USA team
		var response = await this._client.GetAsync($"/api/v2/Teams/{firstTeam.TeamId}");

		// Assert: Response should be successful — NGB admin can view teams in their NGB
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admins should be able to view team details for teams in their NGB");

		var teamDetails = await response.Content.ReadFromJsonAsync<TeamDetailViewModelDto>();
		teamDetails.Should().NotBeNull();
		teamDetails!.TeamId.Should().Be(firstTeam.TeamId);
		teamDetails.Name.Should().Be(firstTeam.Name);
		teamDetails.Managers.Should().NotBeNull();
		teamDetails.Members.Should().NotBeNull();
		teamDetails.SocialAccounts.Should().NotBeNull();
	}

	[Fact]
	public async Task GetTeamDetails_AsNgbAdminOfDifferentNgb_ShouldReturnForbidden()
	{
		// Arrange: Sign in as USA NGB admin and try to view a team from a different NGB
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get national teams — there are teams from AUS and DEU that the USA NGB admin should not access
		var teamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		teamsResult.Should().NotBeNull();
		// Pick a national team that does NOT belong to USA
		var otherNgbTeam = teamsResult!.Items!.FirstOrDefault(t => t.Country != "USA");
		otherNgbTeam.Should().NotBeNull("there should be at least one national team from a non-USA NGB in test data");

		// Act: Try to get team details for a team from another NGB
		var response = await this._client.GetAsync($"/api/v2/Teams/{otherNgbTeam!.TeamId}");

		// Assert: Should be forbidden — NGB admins can only view teams within their own NGB
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"NGB admins should not be able to view team details for teams outside their NGB");
	}

	[Fact]
	public async Task GetTeamDetails_AsRegularUserWithoutAdminRole_ShouldReturnForbidden()
	{
		// Arrange: Sign in as a regular player with no admin or manager role
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Act: Try to get national team details
		var teamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		var firstNationalTeam = teamsResult!.Items!.First();

		var response = await this._client.GetAsync($"/api/v2/Teams/{firstNationalTeam.TeamId}");

		// Assert: Should be forbidden — regular authenticated users without an admin or team manager role
		// cannot view team member details
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"users without admin or team manager role should not be able to view team details");
	}

	[Fact]
	public async Task GetTeamDetails_AsNationalTeamMember_ShouldReturnForbidden()
	{
		// Arrange: Sign in as a regular player and assign them to a national team
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Get all national teams to find the USA national team to join
		var teamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		var nationalTeam = teamsResult!.Items!.FirstOrDefault(t => t.GroupAffiliation == "national" && t.Country == "USA");
		nationalTeam.Should().NotBeNull("there should be a USA national team in test data");

		// Add sarah to the national team
		var joinRequest = new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = new { id = nationalTeam!.TeamId }
		};
		var joinResponse = await this._client.PutAsJsonAsync("/api/v2/Referees/me", joinRequest);
		joinResponse.StatusCode.Should().Be(HttpStatusCode.OK, "sarah should be able to join a national team");

		// Act: Try to get team details as a national team member
		var response = await this._client.GetAsync($"/api/v2/Teams/{nationalTeam.TeamId}");

		// Assert: Should be forbidden — national team membership alone does not grant access to team details
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"national team members without an admin or team manager role should not be able to view team details");

		// Clean up: remove sarah's national team assignment
		var cleanupRequest = new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" },
			coachingTeam = (object?)null,
			nationalTeam = (object?)null
		};
		await this._client.PutAsJsonAsync("/api/v2/Referees/me", cleanupRequest);
	}

	[Fact]
	public async Task GetTeamDetails_AsUnauthenticatedUser_ShouldReturnUnauthorized()
	{
		// Act: Try to get team details without authentication
		var response = await this._client.GetAsync("/api/v2/Teams/TM_1");

		// Assert: Should be unauthorized
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
			"unauthenticated users should not be able to access team details");
	}

	[Fact]
	public async Task GetTeamDetails_AsTeamManager_ShouldHaveIsCurrentUserManagerTrue()
	{
		// Arrange: Sign in as the seeded team manager (who is a manager of Yankees/TM_1 in the seed data)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get team details for Yankees (TM_1) - seeded as the team managed by team_manager@example.com
		var response = await this._client.GetAsync("/api/v2/Teams/TM_1");

		// Assert: Response should indicate user is a manager of this team
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var teamDetails = await response.Content.ReadFromJsonAsync<TeamDetailViewModelDto>();
		teamDetails.Should().NotBeNull();
		teamDetails!.IsCurrentUserManager.Should().BeTrue(
			"team_manager@example.com is seeded as a manager of the Yankees team (TM_1)");
	}

	[Fact]
	public async Task GetTeamDetails_AsNonManager_ShouldHaveIsCurrentUserManagerFalse()
	{
		// Arrange: Sign in as NGB admin (authorized for USA teams but not a team manager)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get a USA team (ngb_admin is not a manager of any USA team by default)
		var teamsResponse = await this._client.GetAsync("/api/v2/Ngbs/USA/teams?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		teamsResult.Should().NotBeNull();
		teamsResult!.Items.Should().NotBeEmpty();

		var firstTeam = teamsResult.Items!.First();

		// Act: Get team details
		var response = await this._client.GetAsync($"/api/v2/Teams/{firstTeam.TeamId}");

		// Assert: Response should be successful but indicate user is not a manager
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var teamDetails = await response.Content.ReadFromJsonAsync<TeamDetailViewModelDto>();
		teamDetails.Should().NotBeNull();
		teamDetails!.IsCurrentUserManager.Should().BeFalse(
			"NGB admin is not a team manager of the Yankees team");
	}

	[Fact]
	public async Task GetTeamDetails_ForNationalTeam_ShouldIncludePrimaryTeamForMembers()
	{
		// Arrange: Sign in as sarah to add her to a national team
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Get Sarah's user ID from her profile to use for member lookup
		var profileResponse = await this._client.GetAsync("/api/v2/Referees/me");
		profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var profileContent = await profileResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
		var sarahUserId = profileContent.GetProperty("userId").GetString();
		sarahUserId.Should().NotBeNullOrEmpty("Sarah's user ID should be retrievable from the profile");

		// Get all national teams and pick the USA one (ngb_admin@example.com is a USA NGB admin)
		var teamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		teamsResult.Should().NotBeNull();
		var nationalTeam = teamsResult!.Items!.FirstOrDefault(t => t.GroupAffiliation == "national" && t.Country == "USA");
		nationalTeam.Should().NotBeNull("there should be a USA national team in test data");

		// Add sarah to the national team while preserving her playing team (Yankees/TM_1)
		var joinRequest = new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" }, // Preserve Sarah's playing team (Yankees/TM_1)
			coachingTeam = (object?)null,
			nationalTeam = new { id = nationalTeam!.TeamId }
		};
		var joinResponse = await this._client.PutAsJsonAsync("/api/v2/Referees/me", joinRequest);
		joinResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"sarah should be able to join a national team");

		// Act: Sign in as NGB admin to view the national team details (national team members are not authorized)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");
		var response = await this._client.GetAsync($"/api/v2/Teams/{nationalTeam.TeamId}");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var teamDetails = await response.Content.ReadFromJsonAsync<TeamDetailViewModelDto>();
		teamDetails.Should().NotBeNull();
		teamDetails!.GroupAffiliation.Should().Be("national");
		teamDetails.Members.Should().NotBeNull();

		// Sarah should appear as a member with her primary (playing) team populated
		var sarahMember = teamDetails.Members!.FirstOrDefault(m => m.UserId == sarahUserId);
		sarahMember.Should().NotBeNull("sarah should appear as a member of the national team after joining");
		sarahMember!.UserId.Should().NotBeNullOrEmpty();
		sarahMember.PrimaryTeamId.Should().NotBeNullOrEmpty(
			"sarah's primary team (Yankees) should be included in the national team member view");
		sarahMember.PrimaryTeamName.Should().NotBeNullOrEmpty(
			"sarah's primary team name should be included in the national team member view");

		// Clean up: sign back in as sarah to remove her national team assignment
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");
		var cleanupRequest = new
		{
			primaryNgb = "USA",
			secondaryNgb = (string?)null,
			playingTeam = new { id = "TM_1" }, // Preserve Sarah's playing team
			coachingTeam = (object?)null,
			nationalTeam = (object?)null
		};
		await this._client.PutAsJsonAsync("/api/v2/Referees/me", cleanupRequest);
	}

	[Fact]
	public async Task UpdateTeam_AsNgbAdmin_ShouldUpdateTeamSuccessfully()
	{
		// Arrange: Sign in as NGB admin (who can also manage teams)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get the Yankees team (a regular team, not a national team)
		var teamsResponse = await this._client.GetAsync("/api/v2/Ngbs/USA/teams?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		teamsResult.Should().NotBeNull();
		var yankeesTeam = teamsResult!.Items!.First(t => t.Name == "Yankees");
		var teamId = yankeesTeam.TeamId;

		// First, make the NGB admin a manager of this team
		var addManagerRequest = new
		{
			Email = "ngb_admin@example.com",
			CreateAccountIfNotExists = false
		};
		var ngb = "USA";
		var addManagerResponse = await this._client.PostAsJsonAsync(
			$"/api/v2/Ngbs/{ngb}/teams/{teamId}/managers",
			addManagerRequest);

		if (addManagerResponse.StatusCode != HttpStatusCode.OK)
		{
			// Manager might already exist, continue anyway
		}

		// Prepare updated team data
		var updatedTeam = new NgbTeamViewModelDto
		{
			TeamId = teamId,
			Name = "Updated Team Name",
			City = "Updated City",
			State = yankeesTeam.State,
			Country = yankeesTeam.Country,
			Status = yankeesTeam.Status,
			GroupAffiliation = yankeesTeam.GroupAffiliation,
			JoinedAt = yankeesTeam.JoinedAt,
			SocialAccounts = yankeesTeam.SocialAccounts,
			LogoUrl = yankeesTeam.LogoUrl,
			Description = "Updated description for the team",
			ContactEmail = "updated.team@example.com"
		};

		// Act: Update the team
		var updateResponse = await this._client.PutAsJsonAsync($"/api/v2/Teams/{teamId}", updatedTeam);

		// Assert: Update should succeed
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"team manager should be able to update their own team");

		var updatedTeamResult = await updateResponse.Content.ReadFromJsonAsync<NgbTeamViewModelDto>();
		updatedTeamResult.Should().NotBeNull();
		updatedTeamResult!.TeamId.Should().Be(teamId);
		updatedTeamResult.Name.Should().Be("Updated Team Name");
		updatedTeamResult.Description.Should().Be("Updated description for the team");
		updatedTeamResult.ContactEmail.Should().Be("updated.team@example.com");

		// Verify the changes persisted
		var verifyResponse = await this._client.GetAsync($"/api/v2/Teams/{teamId}");
		verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var verifiedTeam = await verifyResponse.Content.ReadFromJsonAsync<TeamDetailViewModelDto>();
		verifiedTeam.Should().NotBeNull();
		verifiedTeam!.Name.Should().Be("Updated Team Name");
		verifiedTeam.Description.Should().Be("Updated description for the team");
		verifiedTeam.ContactEmail.Should().Be("updated.team@example.com");
	}

	[Fact]
	public async Task UpdateTeam_AsNonTeamManager_ShouldReturnForbidden()
	{
		// Arrange: Sign in as a regular player who is NOT a manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "sarah.player@example.com", "password");

		// Get any team
		var teamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		teamsResult.Should().NotBeNull();
		var firstTeam = teamsResult!.Items!.First();

		// Prepare update data
		var updatedTeam = new NgbTeamViewModelDto
		{
			TeamId = firstTeam.TeamId,
			Name = "Unauthorized Update",
			City = firstTeam.City,
			State = firstTeam.State,
			Country = firstTeam.Country,
			Status = firstTeam.Status,
			GroupAffiliation = firstTeam.GroupAffiliation,
			JoinedAt = firstTeam.JoinedAt,
			SocialAccounts = firstTeam.SocialAccounts,
			LogoUrl = firstTeam.LogoUrl,
			Description = firstTeam.Description,
			ContactEmail = firstTeam.ContactEmail
		};

		// Act: Try to update the team
		var updateResponse = await this._client.PutAsJsonAsync($"/api/v2/Teams/{firstTeam.TeamId}", updatedTeam);

		// Assert: Should be forbidden (authenticated but not a team manager)
		updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"non-team-managers should not be able to update teams they don't manage");
	}

	[Fact]
	public async Task UpdateTeam_AsUnauthenticated_ShouldReturnUnauthorized()
	{
		// Arrange: Create update data without authentication
		var updatedTeam = new NgbTeamViewModelDto
		{
			TeamId = "TM_1",
			Name = "Unauthorized Update",
			City = "New York",
			State = "NY",
			Country = "USA",
			Status = "competitive",
			GroupAffiliation = "community",
			JoinedAt = "2020-01-01",
			SocialAccounts = System.Array.Empty<SocialAccountDto>(),
			LogoUrl = null,
			Description = "Test",
			ContactEmail = "test@example.com"
		};

		// Act: Try to update without authentication
		var updateResponse = await this._client.PutAsJsonAsync("/api/v2/Teams/TM_1", updatedTeam);

		// Assert: Should be unauthorized
		updateResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
			"unauthenticated users should not be able to update teams");
	}

	[Fact]
	public async Task UpdateTeam_WithMismatchedTeamId_ShouldReturnBadRequest()
	{
		// Arrange: Sign in as NGB admin and make them a manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get a team
		var teamsResponse = await this._client.GetAsync("/api/v2/Teams/national?SkipPaging=true");
		teamsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teamsResponse.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		var firstTeam = teamsResult!.Items!.First();

		// Make the user a manager of the team
		var addManagerRequest = new { Email = "ngb_admin@example.com", CreateAccountIfNotExists = false };
		await this._client.PostAsJsonAsync($"/api/v2/Ngbs/USA/teams/{firstTeam.TeamId}/managers", addManagerRequest);

		// Prepare update data with mismatched team ID
		var updatedTeam = new NgbTeamViewModelDto
		{
			TeamId = "TM_999", // Different from URL
			Name = "Test Team",
			City = "New York",
			State = "NY",
			Country = "USA",
			Status = "competitive",
			GroupAffiliation = "community",
			JoinedAt = "2020-01-01",
			SocialAccounts = System.Array.Empty<SocialAccountDto>(),
			LogoUrl = null,
			Description = "Test",
			ContactEmail = "test@example.com"
		};

		// Act: Try to update with mismatched ID
		var updateResponse = await this._client.PutAsJsonAsync($"/api/v2/Teams/{firstTeam.TeamId}", updatedTeam);

		// Assert: Should return bad request
		updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"team ID in URL should match team ID in request body");
	}
}
