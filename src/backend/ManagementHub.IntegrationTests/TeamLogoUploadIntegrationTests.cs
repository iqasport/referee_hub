using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.IntegrationTests.Models;
using ManagementHub.Service.Filtering;
using Xunit;

namespace ManagementHub.IntegrationTests;

/// <summary>
/// Integration tests for team logo upload functionality.
/// Tests the complete upload sequence from frontend to backend.
/// </summary>
public class TeamLogoUploadIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public TeamLogoUploadIntegrationTests(TestWebApplicationFactory factory)
	{
		this._factory = factory;
		this._client = this._factory.CreateClient();
	}

	[Fact]
	public async Task UploadTeamLogo_WithValidImage_ShouldSucceed()
	{
		// Arrange: Sign in as NGB admin who can manage teams
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		// Get the Yankees team ID (known test team)
		var teams = await this._client.GetAsync("/api/v2/ngbs/USA/teams?SkipPaging=true");
		teams.StatusCode.Should().Be(HttpStatusCode.OK);
		var teamsResult = await teams.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		var yankeesTeam = teamsResult!.Items!.FirstOrDefault(t => t.Name == "Yankees");
		yankeesTeam.Should().NotBeNull("Yankees team should exist in test data");

		// Create a test image (1x1 PNG)
		var pngBytes = CreateTestPngImage();
		var content = new MultipartFormDataContent();
		var fileContent = new ByteArrayContent(pngBytes);
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
		content.Add(fileContent, "logoBlob", "test-logo.png");

		// Act
		var response = await this._client.PutAsync($"/api/v2/teams/{yankeesTeam!.TeamId}/logo", content);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK, "logo upload should succeed");
		var logoUrl = await response.Content.ReadFromJsonAsync<string>();
		logoUrl.Should().NotBeNullOrEmpty("logo URL should be returned");
		logoUrl.Should().StartWith("http", "logo URL should be a valid HTTP URL");

		// Verify the logo URL is persisted in team data
		var teamDetails = await this._client.GetFromJsonAsync<TeamDetailViewModelDto>($"/api/v2/teams/{yankeesTeam.TeamId}");
		teamDetails.Should().NotBeNull();
		teamDetails!.LogoUrl.Should().NotBeNullOrEmpty("team should have logo URL after upload");
	}

	[Fact]
	public async Task UploadTeamLogo_WithNonImageFile_ShouldReturnBadRequest()
	{
		// Arrange
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var teams = await this._client.GetAsync("/api/v2/ngbs/USA/teams?SkipPaging=true");
		var teamsResult = await teams.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		var yankeesTeam = teamsResult!.Items!.FirstOrDefault(t => t.Name == "Yankees");

		// Create a text file instead of an image
		var textBytes = System.Text.Encoding.UTF8.GetBytes("This is not an image");
		var content = new MultipartFormDataContent();
		var fileContent = new ByteArrayContent(textBytes);
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
		content.Add(fileContent, "logoBlob", "not-an-image.txt");

		// Act
		var response = await this._client.PutAsync($"/api/v2/teams/{yankeesTeam!.TeamId}/logo", content);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "non-image files should be rejected");
	}

	[Fact]
	public async Task UploadTeamLogo_WithOversizedImage_ShouldReturnBadRequest()
	{
		// Arrange
		await AuthenticationHelper.AuthenticateAsAsync(this._client, "ngb_admin@example.com", "password");

		var teams = await this._client.GetAsync("/api/v2/ngbs/USA/teams?SkipPaging=true");
		var teamsResult = await teams.Content.ReadFromJsonAsync<Filtered<NgbTeamViewModelDto>>();
		var yankeesTeam = teamsResult!.Items!.FirstOrDefault(t => t.Name == "Yankees");

		// Create a file larger than 5 MB
		var largeFileBytes = new byte[6 * 1024 * 1024]; // 6 MB
		var content = new MultipartFormDataContent();
		var fileContent = new ByteArrayContent(largeFileBytes);
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
		content.Add(fileContent, "logoBlob", "large-image.png");

		// Act
		var response = await this._client.PutAsync($"/api/v2/teams/{yankeesTeam!.TeamId}/logo", content);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "files larger than 5 MB should be rejected");
	}

	[Fact]
	public async Task UploadTeamLogo_WithoutAuthentication_ShouldReturnUnauthorized()
	{
		// Arrange
		// No authorization header set

		var pngBytes = CreateTestPngImage();
		var content = new MultipartFormDataContent();
		var fileContent = new ByteArrayContent(pngBytes);
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
		content.Add(fileContent, "logoBlob", "test-logo.png");

		// Act
		var response = await this._client.PutAsync("/api/v2/teams/TM_01JHB1FWHG1TYJ7Z6VDG95CJE0/logo", content);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "unauthenticated requests should be rejected");
	}

	/// <summary>
	/// Creates a minimal valid 1x1 PNG image for testing.
	/// </summary>
	private static byte[] CreateTestPngImage()
	{
		// Minimal 1x1 white PNG image
		return new byte[]
		{
			0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
			0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
			0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1 dimensions
			0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
			0x89, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x44, 0x41, // IDAT chunk
			0x54, 0x78, 0x9C, 0x62, 0x00, 0x01, 0x00, 0x00,
			0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
			0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, // IEND chunk
			0x42, 0x60, 0x82
		};
	}
}
