using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using Xunit;

namespace ManagementHub.IntegrationTests;

public class TestsAdminApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient client;

	public TestsAdminApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this.client = factory.CreateClient();
	}

	[Fact]
	public async Task GetAllTests_AsStandaloneTestAdmin_ShouldReturnTests()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "translator@example.com", "password");

		var response = await this.client.GetAsync("/api/admin/Tests");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var tests = await response.Content.ReadFromJsonAsync<List<object>>();
		tests.Should().NotBeNull();
		tests.Should().NotBeEmpty();
	}

	[Fact]
	public async Task GetAllTests_AsRegularReferee_ShouldBeForbidden()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var response = await this.client.GetAsync("/api/admin/Tests");

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task GrantAndRevokeTestAdminRole_AsIqaAdmin_ShouldChangeAccess()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var currentUser = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/users/me");
		var refereeUserId = currentUser.GetProperty("userId").GetString();

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "iqa_admin@example.com", "password");
		var grantResponse = await this.client.PutAsync($"/api/v2/users/{refereeUserId}/roles/testAdmin", null);
		grantResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var testsResponseAfterGrant = await this.client.GetAsync("/api/admin/Tests");
		testsResponseAfterGrant.StatusCode.Should().Be(HttpStatusCode.OK);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "iqa_admin@example.com", "password");
		var revokeResponse = await this.client.DeleteAsync($"/api/v2/users/{refereeUserId}/roles/testAdmin");
		revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var testsResponseAfterRevoke = await this.client.GetAsync("/api/admin/Tests");
		testsResponseAfterRevoke.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}
}