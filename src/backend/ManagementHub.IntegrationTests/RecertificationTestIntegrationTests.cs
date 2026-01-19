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
	public async Task FlagRecertification_ShouldAwardBothFlagAndAssistantCertifications()
	{
		// Arrange: Sign in as the recert test referee (has Assistant certification already)
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "recert.test@example.com", "password");

		// Verify initial state: should have only Flag certification (previous version)
		var initialProfileResponse = await this._client.GetAsync("/api/v2/referees/me");
		initialProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK, "getting initial referee profile should succeed");
		var initialProfile = await initialProfileResponse.Content.ReadAsStringAsync();
		var initialProfileJson = JsonDocument.Parse(initialProfile);
		var initialCertifications = initialProfileJson.RootElement.GetProperty("acquiredCertifications").EnumerateArray().ToList();

		initialCertifications.Should().NotBeEmpty("referee should have at least one certification");
		var hasFlagCert = initialCertifications.Any(c => c.GetProperty("level").GetString() == "snitch");
		hasFlagCert.Should().BeTrue("referee should initially have Flag certification from previous version");

		// Get available tests and find Flag recertification test
		var testsResponse = await this._client.GetAsync("/api/v2/referees/me/tests/available");
		testsResponse.StatusCode.Should().Be(HttpStatusCode.OK, "getting available tests should succeed");
		var testsJson = JsonDocument.Parse(await testsResponse.Content.ReadAsStringAsync());

		string? flagRecertTestId = null;
		foreach (var testElement in testsJson.RootElement.EnumerateArray())
		{
			var title = testElement.GetProperty("title").GetString();
			if (title != null && title.Contains("Flag", StringComparison.OrdinalIgnoreCase) &&
				title.Contains("Recert", StringComparison.OrdinalIgnoreCase))
			{
				flagRecertTestId = testElement.GetProperty("testId").GetString();
				break;
			}
		}

		flagRecertTestId.Should().NotBeNull("Flag recertification test should be available");

		// Start the test
		var startResponse = await this._client.PostAsync($"/api/v2/referees/me/tests/{flagRecertTestId}/start", null);
		startResponse.StatusCode.Should().Be(HttpStatusCode.OK, "starting test should succeed");
		var testStartJson = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());

		// Build correct answers by finding the correct answer for each question
		var questions = testStartJson.RootElement.GetProperty("questions").EnumerateArray().ToList();
		var answers = new System.Collections.Generic.List<object>();

		// In the seeded data, correct answers have "Correct" in their description
		foreach (var question in questions)
		{
			var questionId = question.GetProperty("questionId").GetInt64();
			var questionAnswers = question.GetProperty("answers").EnumerateArray().ToList();

			// Find the answer with "Correct" in the description
			var correctAnswer = questionAnswers.FirstOrDefault(a =>
				a.GetProperty("htmlText").GetString()?.Contains("Correct", StringComparison.OrdinalIgnoreCase) == true);

			if (correctAnswer.ValueKind == JsonValueKind.Undefined)
			{
				// Fallback: just pick the first answer if we can't find the correct one
				correctAnswer = questionAnswers[0];
			}

			var answerId = correctAnswer.GetProperty("answerId").GetInt64();
			answers.Add(new { questionId, answerId });
		}

		// Submit the test
		var submitModel = new
		{
			startedAt = DateTime.UtcNow.AddMinutes(-5),
			answers
		};

		var submitResponse = await this._client.PostAsJsonAsync($"/api/v2/referees/me/tests/{flagRecertTestId}/submit", submitModel);
		submitResponse.StatusCode.Should().Be(HttpStatusCode.OK, "submitting test should succeed");

		var submitResult = await submitResponse.Content.ReadAsStringAsync();
		var submitJson = JsonDocument.Parse(submitResult);
		var passed = submitJson.RootElement.GetProperty("passed").GetBoolean();
		passed.Should().BeTrue("test should be passed with correct answers");

		// Verify final state: should now have both Assistant and Flag certifications for the new version
		var finalProfileResponse = await this._client.GetAsync("/api/v2/referees/me");
		finalProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK, "getting final referee profile should succeed");
		var finalProfile = await finalProfileResponse.Content.ReadAsStringAsync();
		var finalProfileJson = JsonDocument.Parse(finalProfile);
		var finalCertifications = finalProfileJson.RootElement.GetProperty("acquiredCertifications").EnumerateArray().ToList();

		// Assert: Should have Flag certifications for both versions (old and new)
		// Plus Assistant certification for the new version
		var flagCerts = finalCertifications.Where(c => c.GetProperty("level").GetString() == "snitch").ToList();
		var assistantCerts = finalCertifications.Where(c => c.GetProperty("level").GetString() == "assistant").ToList();

		flagCerts.Should().HaveCountGreaterOrEqualTo(2, "referee should have Flag certification for both old and new versions");
		assistantCerts.Should().HaveCount(1, "referee should have Assistant certification for the new version");

		// Verify the new version certifications were awarded
		var hasNewVersionFlag = finalCertifications.Any(c =>
			c.GetProperty("level").GetString() == "snitch" &&
			c.GetProperty("version").GetString() == "twentyfour");
		var hasNewVersionAssistant = finalCertifications.Any(c =>
			c.GetProperty("level").GetString() == "assistant" &&
			c.GetProperty("version").GetString() == "twentyfour");

		hasNewVersionFlag.Should().BeTrue("referee should have Flag certification for the new version (twentyfour)");
		hasNewVersionAssistant.Should().BeTrue("referee should have Assistant certification for the new version (twentyfour)");
	}
}
