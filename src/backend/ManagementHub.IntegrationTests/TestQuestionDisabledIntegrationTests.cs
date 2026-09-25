using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.Models.Abstraction.Commands.Import;
using ManagementHub.Service.Areas.Tests;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for disabling/enabling individual test questions from the admin API.
/// </summary>
public class TestQuestionDisabledIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory factory;
	private readonly HttpClient client;

	public TestQuestionDisabledIntegrationTests(TestWebApplicationFactory factory)
	{
		this.factory = factory;
		this.client = this.factory.CreateClient();
	}

	private async Task<(string testId, TestQuestionRecord question)> GetFirstTestWithQuestionsAsync()
	{
		var tests = await this.client.GetFromJsonAsync<JsonElement[]>("/api/admin/Tests");

		foreach (var test in tests ?? [])
		{
			if (!test.TryGetProperty("testId", out var testIdProperty))
			{
				continue;
			}

			var testId = testIdProperty.GetString();
			if (string.IsNullOrWhiteSpace(testId))
			{
				continue;
			}

			var questions = await this.client.GetFromJsonAsync<TestQuestionRecord[]>(
				$"/api/admin/Tests/{testId}/questions");

			if (questions is { Length: > 0 })
			{
				return (testId, questions.First());
			}
		}

		throw new Xunit.Sdk.XunitException("No seeded test with questions was found.");
	}

	[Fact]
	public async Task SetQuestionDisabled_WithIqaAdmin_ShouldPersistAndExposeDisabledState()
	{
		await AuthenticationHelper.AuthenticateAsAsync(
			this.client,
			"iqa_admin@example.com",
			"password");

		var (testId, question) = await this.GetFirstTestWithQuestionsAsync();

		question.SequenceNum.Should().NotBeNull();

		var disableResponse = await this.client.PostAsJsonAsync(
			$"/api/admin/Tests/{testId}/questions/{question.SequenceNum}/disabled",
			true);

		disableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var questionsAfterDisable =
			await this.client.GetFromJsonAsync<TestQuestionRecord[]>(
				$"/api/admin/Tests/{testId}/questions");

		questionsAfterDisable.Should().NotBeNull();

		questionsAfterDisable!
			.Single(q => q.SequenceNum == question.SequenceNum)
			.Disabled
			.Should()
			.BeTrue();

		var enableResponse = await this.client.PostAsJsonAsync(
			$"/api/admin/Tests/{testId}/questions/{question.SequenceNum}/disabled",
			false);

		enableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var questionsAfterEnable =
			await this.client.GetFromJsonAsync<TestQuestionRecord[]>(
				$"/api/admin/Tests/{testId}/questions");

		questionsAfterEnable.Should().NotBeNull();

		questionsAfterEnable!
			.Single(q => q.SequenceNum == question.SequenceNum)
			.Disabled
			.Should()
			.BeFalse();
	}

	[Fact]
	public async Task SetQuestionDisabled_WithNonAdmin_ShouldReturnForbidden()
	{
		await AuthenticationHelper.AuthenticateAsAsync(
			this.client,
			"referee@example.com",
			"password");

		var response = await this.client.PostAsJsonAsync(
			"/api/admin/Tests/T_00000000000000000000000000/questions/1/disabled",
			true);

		response.StatusCode.Should().BeOneOf(
			HttpStatusCode.Forbidden,
			HttpStatusCode.Unauthorized);
	}
}
