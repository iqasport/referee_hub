using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for usernames: viewing, updating, availability and lookup.
/// </summary>
public class UsernameApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient client;

	public UsernameApiIntegrationTests(TestWebApplicationFactory factory)
	{
		this.client = factory.CreateClient();
	}

	[Fact]
	public async Task GetCurrentUserData_ShouldIncludeUsername()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var data = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/me/info");

		data.GetProperty("username").GetString().Should().Be("jimmy.referee");
	}

	[Fact]
	public async Task GetUserByUsername_ShouldResolveCaseInsensitively()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var me = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/me");

		var response = await this.client.GetAsync("/api/v2/Users/by-username/Jimmy.Referee");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var lookup = await response.Content.ReadFromJsonAsync<JsonElement>();
		lookup.GetProperty("userId").GetString().Should().Be(me.GetProperty("userId").GetString());
		lookup.GetProperty("username").GetString().Should().Be("jimmy.referee");
	}

	[Fact]
	public async Task GetUserByUsername_Unknown_ShouldReturnNotFound()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var response = await this.client.GetAsync("/api/v2/Users/by-username/no.such.username");

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task UpdateUsername_WithValidUnusedValue_ShouldPersistNormalized()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "mike.coach@example.com", "password");

		var response = await this.client.PatchAsJsonAsync("/api/v2/Users/me/info", new { username = "  Coach.Mike_1 " });
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var data = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/me/info");
		data.GetProperty("username").GetString().Should().Be("coach.mike_1");
		data.GetProperty("firstName").GetString().Should().Be("Mike", "other fields should not be affected");

		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");
		var availability = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/username-availability?username=coach.mike_1");
		availability.GetProperty("isValid").GetBoolean().Should().BeTrue();
		availability.GetProperty("isAvailable").GetBoolean().Should().BeFalse("the username is now taken by another user");
	}

	[Fact]
	public async Task UpdateUsername_WhenTaken_ShouldReturnConflict()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "sarah.player@example.com", "password");

		var response = await this.client.PatchAsJsonAsync("/api/v2/Users/me/info", new { username = "jimmy.referee" });

		response.StatusCode.Should().Be(HttpStatusCode.Conflict);

		var data = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/me/info");
		data.GetProperty("username").GetString().Should().Be("sarah.player");
	}

	[Theory]
	[InlineData("ab")]
	[InlineData("has space")]
	[InlineData("someone@example.com")]
	[InlineData(".leadingdot")]
	public async Task UpdateUsername_WithInvalidFormat_ShouldReturnBadRequest(string username)
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "sarah.player@example.com", "password");

		var response = await this.client.PatchAsJsonAsync("/api/v2/Users/me/info", new { username });

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CheckUsernameAvailability_OwnUsername_ShouldBeAvailable()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var availability = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/username-availability?username=Jimmy.Referee");

		availability.GetProperty("username").GetString().Should().Be("jimmy.referee");
		availability.GetProperty("isAvailable").GetBoolean().Should().BeTrue();
	}

	[Fact]
	public async Task CheckUsernameAvailability_InvalidFormat_ShouldReportInvalid()
	{
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		var availability = await this.client.GetFromJsonAsync<JsonElement>("/api/v2/Users/username-availability?username=a!");

		availability.GetProperty("isValid").GetBoolean().Should().BeFalse();
		availability.GetProperty("isAvailable").GetBoolean().Should().BeFalse();
	}
}
