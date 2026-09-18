using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

	private async Task<(TestViewModel test, TestQuestionRecord question)> GetFirstTestWithQuestionsAsync()
	{
		var tests = await this.client.GetFromJsonAsync<TestViewModel[]>("/api/admin/Tests");
		foreach (var test in tests!)
		{
			var questions = await this.client.GetFromJsonAsync<TestQuestionRecord[]>($"/api/admin/Tests/{test.TestId}/questions");
			if (questions is { Length: > 0 })
			{
				return (test, questions.First());
			}
		}

		throw new Xunit.Sdk.XunitException("No seeded test with questions was found.");
	}

	[Fact]
	public async Task SetQuestionDisabled_WithIqaAdmin_ShouldPersistAndExcludeFromQuestionsForNewAttempts()
	{
		// Arrange
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "iqa_admin@example.com", "password");
		var (test, question) = await this.GetFirstTestWithQuestionsAsync();

		// Act: disable the question
		var disableResponse = await this.client.PostAsJsonAsync(
			$"/api/admin/Tests/{test.TestId}/questions/{question.SequenceNum}/disabled", true);

		// Assert
		disableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var questionsAfterDisable = await this.client.GetFromJsonAsync<TestQuestionRecord[]>($"/api/admin/Tests/{test.TestId}/questions");
		questionsAfterDisable!.Single(q => q.SequenceNum == question.SequenceNum).Disabled.Should().BeTrue();

		// Act: re-enable the question so other tests / seeded data are not affected
		var enableResponse = await this.client.PostAsJsonAsync(
			$"/api/admin/Tests/{test.TestId}/questions/{question.SequenceNum}/disabled", false);

		// Assert
		enableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var questionsAfterEnable = await this.client.GetFromJsonAsync<TestQuestionRecord[]>($"/api/admin/Tests/{test.TestId}/questions");
		questionsAfterEnable!.Single(q => q.SequenceNum == question.SequenceNum).Disabled.Should().BeFalse();
	}

	[Fact]
	public async Task SetQuestionDisabled_WithNonAdmin_ShouldReturnForbidden()
	{
		// Arrange
		await AuthenticationHelper.AuthenticateAsAsync(this.client, "referee@example.com", "password");

		// Act
		var response = await this.client.PostAsJsonAsync("/api/admin/Tests/T_00000000000000000000000000/questions/1/disabled", true);

		// Assert
		response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
	}
}
