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
using ManagementHub.Service.Filtering;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for Team Members API endpoint.
/// Tests listing team members (referees) with proper authorization and filtering.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class TeamMembersApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TeamMembersApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task GetTeamMembers_AsTeamManager_ShouldReturnMembers()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get team members for Yankees team (TM_1)
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"team manager should be able to list members of their team");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		// The seeded data has referee@example.com as a player on Yankees team
		members.Should().NotBeEmpty("there should be at least one member on the team");
		members.Should().Contain(m => m.Name.Contains("Referee"),
			"the referee user should be in the team members list");

		// Verify the returned data structure
		var firstMember = members.First();
		firstMember.UserId.Should().NotBeNullOrEmpty("UserId should be populated");
		firstMember.Name.Should().NotBeNullOrEmpty("Name should be populated");

		// Verify UserId is properly formatted UserIdentifier (should start with U_)
		firstMember.UserId.Should().MatchRegex("^U_",
			"UserId should be a properly formatted UserIdentifier");
	}

	[Fact]
	public async Task GetTeamMembers_AsNgbAdmin_ShouldReturnMembers()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Get team members for Yankees team (TM_1) under USA NGB
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"NGB admin should be able to list members of teams in their NGB");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		members.Should().NotBeEmpty("there should be at least one member on the team");
	}

	[Fact]
	public async Task GetTeamMembers_WithNameFilter_ShouldReturnFilteredResults()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get team members with name filter
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members?filter=Jimmy");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"filtered request should succeed");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		// Should contain Jimmy Referee
		members.Should().Contain(m => m.Name.Contains("Jimmy"),
			"filter should return members matching the name");
	}

	[Fact]
	public async Task GetTeamMembers_WithPagination_ShouldRespectPageSize()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get team members with pagination
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members?page=1&pageSize=1");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"paginated request should succeed");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		// Should respect page size
		members.Should().HaveCountLessOrEqualTo(1,
			"pagination should limit results to requested page size");

		// Metadata should be present
		membersResponse.Metadata.Should().NotBeNull("pagination metadata should be included");
	}

	[Fact]
	public async Task GetTeamMembers_TeamFromDifferentNgb_ShouldReturnEmpty()
	{
		// Arrange: Sign in as USA NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to get team members for a team from different NGB (e.g., ARG team TM_3)
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_3/members");

		// Assert: Response should be successful but return empty
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"request should succeed but return empty for team from different NGB");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		members.Should().BeEmpty("should return empty when team doesn't belong to specified NGB");
	}

	[Fact]
	public async Task GetTeamMembers_WithoutAuthentication_ShouldReturnUnauthorized()
	{
		// Act: Try to get team members without authentication
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members");

		// Assert: Should return unauthorized
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
			"unauthenticated requests should be rejected");
	}

	[Fact]
	public async Task GetTeamMembers_AsRegularReferee_ShouldReturnForbidden()
	{
		// Arrange: Sign in as regular referee (not team manager or NGB admin)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Act: Try to get team members
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members");

		// Assert: Should return forbidden
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
			"regular referees should not be able to list team members");
	}

	[Fact]
	public async Task GetTeamMembers_NonExistentTeam_ShouldReturnEmpty()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to get team members for non-existent team
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_999999/members");

		// Assert: Response should be successful but return empty
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"request should succeed for non-existent team");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		members.Should().BeEmpty("should return empty for non-existent team");
	}

	[Fact]
	public async Task GetTeamMembers_CaseInsensitiveNameFilter_ShouldWork()
	{
		// Arrange: Sign in as team manager
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "team_manager@example.com", "password");

		// Act: Get team members with lowercase filter (should match "Jimmy" case-insensitively)
		var response = await this._client.GetAsync("/api/v2/Ngbs/USA/teams/TM_1/members?filter=jimmy");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK,
			"case-insensitive filter should work");

		var membersResponse = await response.Content.ReadFromJsonAsync<Filtered<TeamMemberViewModelDto>>();
		membersResponse.Should().NotBeNull();
		var members = membersResponse!.Items.ToList();

		// Should find Jimmy even with lowercase filter
		members.Should().Contain(m => m.Name.Contains("Jimmy", StringComparison.OrdinalIgnoreCase),
			"case-insensitive filter should match members");
	}
}
