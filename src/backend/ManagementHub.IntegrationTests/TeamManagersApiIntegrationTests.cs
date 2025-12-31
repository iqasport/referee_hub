using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.IntegrationTests.Models;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for Team Managers API endpoints (Phase 5).
/// Tests NGB Admin ability to bootstrap team managers and manage team manager access.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class TeamManagersApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TeamManagersApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task GetTeamManagers_AsTeamManager_ShouldReturnManagers()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get team managers for Yankees team (TM_1)
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/managers");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"team manager should be able to list managers of their team");

		var managers = await response.Content.ReadFromJsonAsync<List<TeamManagerViewModelDto>>();
		managers.Should().NotBeNull();
		managers!.Should().NotBeEmpty("there should be at least one manager on the team");

		// The seeded data has team_manager@example.com as a manager on Yankees team
		managers.Should().Contain(m => m.Email == "team_manager@example.com",
			"the seeded team manager should be in the list");
	}

	[Fact]
	public async Task GetTeamManagers_AsNgbAdmin_ShouldReturnManagers()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Get team managers for Yankees team (TM_1)
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/managers");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to list managers of teams in their NGB");

		var managers = await response.Content.ReadFromJsonAsync<List<TeamManagerViewModelDto>>();
		managers.Should().NotBeNull();
		managers!.Should().NotBeEmpty("there should be at least one manager on the team");
	}

	[Fact]
	public async Task GetTeamManagers_AsUnauthorizedUser_ShouldReturnUnauthorized()
	{
		// Arrange: Sign in as regular referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Act: Try to get team managers
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/managers");

		// Assert: Should be forbidden
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"regular users without team manager or NGB admin role should not access this endpoint");
	}

	[Fact]
	public async Task GetTeamManagers_TeamFromDifferentNgb_ShouldReturnEmpty()
	{
		// Arrange: Sign in as USA NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to get team managers for a team from different NGB (e.g., ARG team TM_3)
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_3/managers");

		// Assert: Response should be successful but return empty
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"request should succeed but return empty for team from different NGB");

		var managers = await response.Content.ReadFromJsonAsync<List<TeamManagerViewModelDto>>();
		managers.Should().NotBeNull();
		managers!.Should().BeEmpty("should return empty when team doesn't belong to specified NGB");
	}

	[Fact]
	public async Task AddTeamManager_AsNgbAdmin_WithExistingUser_ShouldAddManager()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "referee@example.com", // Existing user
			CreateAccountIfNotExists = false
		};

		// Act: Add manager to Yankees team (TM_1)
		var response = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_1/managers", newManager);

		// Assert: Should succeed
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to add managers to teams in their NGB");

		var status = await response.Content.ReadFromJsonAsync<TeamManagerCreationStatusDto>();
		status.Should().Be(TeamManagerCreationStatusDto.ManagerRoleAdded,
			"existing user should have manager role added");

		// Verify the manager was added by listing managers
		var getResponse = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/managers");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var managers = await getResponse.Content.ReadFromJsonAsync<List<TeamManagerViewModelDto>>();
		managers.Should().Contain(m => m.Email == "referee@example.com",
			"newly added manager should appear in the list");
	}

	[Fact]
	public async Task AddTeamManager_AsNgbAdmin_WithNewUser_ShouldCreateUserAndAddManager()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "new_manager@example.com", // New user
			CreateAccountIfNotExists = true
		};

		// Act: Add manager to Yankees team (TM_1)
		var response = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_1/managers", newManager);

		// Assert: Should succeed
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to create user and add as manager");

		var status = await response.Content.ReadFromJsonAsync<TeamManagerCreationStatusDto>();
		status.Should().Be(TeamManagerCreationStatusDto.ManagerUserCreated,
			"new user should be created with manager role");

		// Verify the manager was added
		var getResponse = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/managers");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var managers = await getResponse.Content.ReadFromJsonAsync<List<TeamManagerViewModelDto>>();
		managers.Should().Contain(m => m.Email == "new_manager@example.com",
			"newly created manager should appear in the list");
	}

	[Fact]
	public async Task AddTeamManager_AsNgbAdmin_NonExistentUserWithoutCreate_ShouldReturnUserDoesNotExist()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "nonexistent@example.com",
			CreateAccountIfNotExists = false // Don't create new users
		};

		// Act: Try to add manager
		var response = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_1/managers", newManager);

		// Assert: Should return UserDoesNotExist status
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"request should succeed but return UserDoesNotExist status");

		var status = await response.Content.ReadFromJsonAsync<TeamManagerCreationStatusDto>();
		status.Should().Be(TeamManagerCreationStatusDto.UserDoesNotExist,
			"should indicate user doesn't exist when CreateAccountIfNotExists is false");
	}

	[Fact]
	public async Task AddTeamManager_AsNgbAdmin_InvalidEmail_ShouldReturnBadRequest()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "invalid-email", // Invalid email format
			CreateAccountIfNotExists = false
		};

		// Act: Try to add manager with invalid email
		var response = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_1/managers", newManager);

		// Assert: Should return bad request
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"invalid email should return bad request");
	}

	[Fact]
	public async Task AddTeamManager_AsNgbAdmin_TeamFromDifferentNgb_ShouldReturnNotFound()
	{
		// Arrange: Sign in as USA NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "referee@example.com",
			CreateAccountIfNotExists = false
		};

		// Act: Try to add manager to team from different NGB (ARG team TM_3)
		var response = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_3/managers", newManager);

		// Assert: Should return not found
		response.StatusCode.Should().Be(HttpStatusCode.NotFound,
			"should return not found when team doesn't belong to NGB admin's jurisdiction");
	}

	[Fact]
	public async Task AddTeamManager_AsTeamManager_ShouldReturnForbidden()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "referee@example.com",
			CreateAccountIfNotExists = false
		};

		// Act: Try to add manager (only NGB admins can do this)
		var response = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_1/managers", newManager);

		// Assert: Should be forbidden
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"team managers should not be able to add other managers via this endpoint");
	}

	[Fact]
	public async Task DeleteTeamManager_AsNgbAdmin_ShouldRemoveManager()
	{
		// Arrange: Sign in as NGB admin and add a manager first
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var newManager = new TeamManagerCreationModelDto
		{
			Email = "referee@example.com",
			CreateAccountIfNotExists = false
		};
		await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams/TM_1/managers", newManager);

		// Act: Delete the manager
		var response = await this._client.DeleteAsync("/api/v2/Ngbs/USA/teams/TM_1/managers?email=referee@example.com");

		// Assert: Should succeed
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to remove managers from teams in their NGB");

		// Verify the manager was removed
		var getResponse = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/managers");
		var managers = await getResponse.Content.ReadFromJsonAsync<List<TeamManagerViewModelDto>>();
		managers.Should().NotContain(m => m.Email == "referee@example.com",
			"deleted manager should not appear in the list");
	}

	[Fact]
	public async Task DeleteTeamManager_AsNgbAdmin_NonExistentManager_ShouldReturnNotFound()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to delete a manager that doesn't exist
		var response = await this._client.DeleteAsync("/api/v2/Ngbs/USA/teams/TM_1/managers?email=nonexistent@example.com");

		// Assert: Should return not found
		response.StatusCode.Should().Be(HttpStatusCode.NotFound,
			"should return not found when trying to delete non-existent manager");
	}

	[Fact]
	public async Task DeleteTeamManager_AsNgbAdmin_InvalidEmail_ShouldReturnBadRequest()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to delete with invalid email
		var response = await this._client.DeleteAsync("/api/v2/Ngbs/USA/teams/TM_1/managers?email=invalid-email");

		// Assert: Should return bad request
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"invalid email should return bad request");
	}

	[Fact]
	public async Task DeleteTeamManager_AsTeamManager_ShouldReturnForbidden()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Try to delete a manager (only NGB admins can do this)
		var response = await this._client.DeleteAsync("/api/v2/Ngbs/USA/teams/TM_1/managers?email=referee@example.com");

		// Assert: Should be forbidden
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"team managers should not be able to remove managers via this endpoint");
	}

	[Fact]
	public async Task DeleteTeamManager_AsNgbAdmin_TeamFromDifferentNgb_ShouldReturnNotFound()
	{
		// Arrange: Sign in as USA NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to delete manager from team in different NGB
		var response = await this._client.DeleteAsync("/api/v2/Ngbs/USA/teams/TM_3/managers?email=referee@example.com");

		// Assert: Should return not found
		response.StatusCode.Should().Be(HttpStatusCode.NotFound,
			"should return not found when team doesn't belong to NGB admin's jurisdiction");
	}
}
