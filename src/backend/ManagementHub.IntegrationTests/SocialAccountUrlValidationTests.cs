using System;
using System.Collections.Generic;
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
/// Integration tests for Social Account URL validation.
/// Tests that invalid URLs are rejected and valid URLs (with/without https://) are accepted.
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class SocialAccountUrlValidationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public SocialAccountUrlValidationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task UpdateNgbSocialAccounts_WithInvalidUrl_ShouldReturnBadRequest()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get the current NGB info
		var getNgbResponse = await this._client.GetAsync("/api/v2/Ngbs/USA");
		getNgbResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var ngbInfo = await getNgbResponse.Content.ReadFromJsonAsync<NgbInfoDto>();
		ngbInfo.Should().NotBeNull();

		// Act: Try to update with an invalid URL (no protocol)
		var updateModel = new NgbUpdateModelDto
		{
			Name = ngbInfo!.Name,
			Country = ngbInfo.Country,
			Acronym = ngbInfo.Acronym,
			Website = ngbInfo.Website,
			PlayerCount = ngbInfo.PlayerCount,
			SocialAccounts = new[]
			{
				new SocialAccountDto { Url = "not a valid url", Type = "other" }
			}
		};

		var updateResponse = await this._client.PutAsJsonAsync("/api/v2/Ngbs/USA", updateModel);

		// Assert: Should be bad request
		updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"invalid URLs should be rejected");
	}

	[Fact]
	public async Task UpdateNgbSocialAccounts_WithUrlMissingProtocol_ShouldAddHttpsAndSucceed()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get the current NGB info
		var getNgbResponse = await this._client.GetAsync("/api/v2/Ngbs/USA");
		getNgbResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var ngbInfo = await getNgbResponse.Content.ReadFromJsonAsync<NgbInfoDto>();
		ngbInfo.Should().NotBeNull();

		// Act: Update with a URL missing https://
		var updateModel = new NgbUpdateModelDto
		{
			Name = ngbInfo!.Name,
			Country = ngbInfo.Country,
			Acronym = ngbInfo.Acronym,
			Website = ngbInfo.Website,
			PlayerCount = ngbInfo.PlayerCount,
			SocialAccounts = new[]
			{
				new SocialAccountDto { Url = "facebook.com/testpage", Type = "facebook" }
			}
		};

		var updateResponse = await this._client.PutAsJsonAsync("/api/v2/Ngbs/USA", updateModel);

		// Assert: Should succeed
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"URLs missing protocol should have https:// added automatically");

		// Verify the URL was saved correctly with https://
		var verifyResponse = await this._client.GetAsync("/api/v2/Ngbs/USA");
		verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var updatedNgbInfo = await verifyResponse.Content.ReadFromJsonAsync<NgbInfoDto>();
		updatedNgbInfo.Should().NotBeNull();
		updatedNgbInfo!.SocialAccounts.Should().ContainSingle();
		updatedNgbInfo.SocialAccounts[0].Url.Should().StartWith("https://",
			"URL should have https:// prefix added");
	}

	[Fact]
	public async Task UpdateNgbSocialAccounts_WithValidHttpsUrl_ShouldSucceed()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get the current NGB info
		var getNgbResponse = await this._client.GetAsync("/api/v2/Ngbs/USA");
		getNgbResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var ngbInfo = await getNgbResponse.Content.ReadFromJsonAsync<NgbInfoDto>();
		ngbInfo.Should().NotBeNull();

		// Act: Update with a valid URL with https://
		var updateModel = new NgbUpdateModelDto
		{
			Name = ngbInfo!.Name,
			Country = ngbInfo.Country,
			Acronym = ngbInfo.Acronym,
			Website = ngbInfo.Website,
			PlayerCount = ngbInfo.PlayerCount,
			SocialAccounts = new[]
			{
				new SocialAccountDto { Url = "https://twitter.com/testaccount", Type = "twitter" }
			}
		};

		var updateResponse = await this._client.PutAsJsonAsync("/api/v2/Ngbs/USA", updateModel);

		// Assert: Should succeed
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"valid URLs with https:// should be accepted");

		// Verify the URL was saved correctly
		var verifyResponse = await this._client.GetAsync("/api/v2/Ngbs/USA");
		verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var updatedNgbInfo = await verifyResponse.Content.ReadFromJsonAsync<NgbInfoDto>();
		updatedNgbInfo.Should().NotBeNull();
		updatedNgbInfo!.SocialAccounts.Should().ContainSingle();
		updatedNgbInfo.SocialAccounts[0].Url.Should().Be("https://twitter.com/testaccount");
	}

	[Fact]
	public async Task CreateTeam_WithInvalidSocialAccountUrl_ShouldReturnBadRequest()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Try to create a team with an invalid social account URL
		var createTeamModel = new
		{
			name = "Test Team",
			city = "Test City",
			country = "USA",
			state = "NY",
			status = "competitive",
			groupAffiliation = "community",
			joinedAt = "2024-01-01",
			socialAccounts = new[] { new { url = "invalid url here", type = "other" } }
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams", createTeamModel);

		// Assert: Should be bad request
		createResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
			"invalid URLs should be rejected when creating teams");
	}

	[Fact]
	public async Task CreateTeam_WithUrlMissingProtocol_ShouldAddHttpsAndSucceed()
	{
		// Arrange: Sign in as NGB admin
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Act: Create a team with a URL missing https://
		var createTeamModel = new
		{
			name = "Test Team 2",
			city = "Test City",
			country = "USA",
			state = "NY",
			status = "competitive",
			groupAffiliation = "community",
			joinedAt = "2024-01-01",
			socialAccounts = new[] { new { url = "instagram.com/testteam", type = "instagram" } }
		};

		var createResponse = await this._client.PostAsJsonAsync("/api/v2/Ngbs/USA/teams", createTeamModel);

		// Assert: Should succeed
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK,
			"URLs missing protocol should have https:// added automatically");

		var createdTeam = await createResponse.Content.ReadFromJsonAsync<NgbTeamViewModelDto>();
		createdTeam.Should().NotBeNull();
		createdTeam!.SocialAccounts.Should().ContainSingle();
		createdTeam.SocialAccounts[0].Url.Should().StartWith("https://",
			"URL should have https:// prefix added");
	}
}
