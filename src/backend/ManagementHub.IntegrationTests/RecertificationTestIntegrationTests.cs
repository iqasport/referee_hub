using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.Models.Enums;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for recertification tests to verify correct certification awards.
/// Recertification tests should award all previous certifications (e.g., FR recert awards FR + AR, HR recert awards HR + FR + AR).
/// Uses Testcontainers to run against a real PostgreSQL database.
/// </summary>
public class RecertificationTestIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public RecertificationTestIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task FlagRecertificationTest_ShouldAwardFlagAndAssistantCertifications()
	{
		// Arrange: Sign in as a referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Act: Get available tests
		var response = await this._client.GetAsync("/api/v2/referees/me/tests/available");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK, "getting available tests should succeed");

		// Parse JSON manually to avoid serialization issues in tests
		var jsonString = await response.Content.ReadAsStringAsync();
		var json = JsonDocument.Parse(jsonString);
		
		// Find Flag recertification test (if one exists in test data)
		foreach (var testElement in json.RootElement.EnumerateArray())
		{
			var title = testElement.GetProperty("title").GetString();
			if (title != null && title.Contains("Flag", StringComparison.OrdinalIgnoreCase) && 
			    title.Contains("Recert", StringComparison.OrdinalIgnoreCase))
			{
				var certifications = testElement.GetProperty("awardedCertifications").EnumerateArray();
				var levels = certifications.Select(c => c.GetProperty("level").GetString()).ToList();
				
				// Verify that Flag recertification awards both Flag and Assistant certifications
				levels.Should().Contain("snitch", "Flag recertification should award Flag certification");
				levels.Should().Contain("assistant", "Flag recertification should award Assistant certification");
				levels.Count.Should().Be(2, "Flag recertification should award exactly 2 certifications (Flag and Assistant)");
				return; // Test passed
			}
		}
		
		Assert.True(false, "No Flag recertification test found in test data");
	}

	[Fact]
	public async Task HeadRecertificationTest_ShouldAwardHeadFlagAndAssistantCertifications()
	{
		// Arrange: Sign in as a referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Act: Get available tests
		var response = await this._client.GetAsync("/api/v2/referees/me/tests/available");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK, "getting available tests should succeed");

		// Parse JSON manually to avoid serialization issues in tests
		var jsonString = await response.Content.ReadAsStringAsync();
		var json = JsonDocument.Parse(jsonString);
		
		// Find Head recertification test (if one exists in test data)
		foreach (var testElement in json.RootElement.EnumerateArray())
		{
			var title = testElement.GetProperty("title").GetString();
			if (title != null && title.Contains("Head", StringComparison.OrdinalIgnoreCase) && 
			    title.Contains("Recert", StringComparison.OrdinalIgnoreCase))
			{
				var certifications = testElement.GetProperty("awardedCertifications").EnumerateArray();
				var levels = certifications.Select(c => c.GetProperty("level").GetString()).ToList();
				
				// Verify that Head recertification awards Head, Flag, and Assistant certifications
				levels.Should().Contain("head", "Head recertification should award Head certification");
				levels.Should().Contain("snitch", "Head recertification should award Flag certification");
				levels.Should().Contain("assistant", "Head recertification should award Assistant certification");
				levels.Count.Should().Be(3, "Head recertification should award exactly 3 certifications (Head, Flag, and Assistant)");
				return; // Test passed
			}
		}
		
		Assert.True(false, "No Head recertification test found in test data");
	}

	[Fact]
	public async Task AssistantRecertificationTest_ShouldAwardOnlyAssistantCertification()
	{
		// Arrange: Sign in as a referee
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "referee@example.com", "password");

		// Act: Get available tests
		var response = await this._client.GetAsync("/api/v2/referees/me/tests/available");

		// Assert: Response should be successful
		response.StatusCode.Should().Be(HttpStatusCode.OK, "getting available tests should succeed");

		// Parse JSON manually to avoid serialization issues in tests
		var jsonString = await response.Content.ReadAsStringAsync();
		var json = JsonDocument.Parse(jsonString);
		
		// Find Assistant recertification test (if one exists in test data)
		foreach (var testElement in json.RootElement.EnumerateArray())
		{
			var title = testElement.GetProperty("title").GetString();
			if (title != null && title.Contains("Assitant", StringComparison.OrdinalIgnoreCase) && 
			    title.Contains("Recert", StringComparison.OrdinalIgnoreCase))
			{
				var certifications = testElement.GetProperty("awardedCertifications").EnumerateArray();
				var levels = certifications.Select(c => c.GetProperty("level").GetString()).ToList();
				
				// Verify that Assistant recertification awards only Assistant certification
				levels.Should().Contain("assistant", "Assistant recertification should award Assistant certification");
				levels.Count.Should().Be(1, "Assistant recertification should award exactly 1 certification (Assistant only)");
				return; // Test passed
			}
		}
		
		Assert.True(false, "No Assistant recertification test found in test data");
	}
}
