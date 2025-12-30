using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace ManagementHub.IntegrationTests.Helpers;

/// <summary>
/// Helper class for authentication operations in integration tests.
/// </summary>
public static class AuthenticationHelper
{
	/// <summary>
	/// Authenticates a user and sets the Bearer token on the HttpClient.
	/// </summary>
	/// <param name="client">The HTTP client to authenticate.</param>
	/// <param name="email">User email address.</param>
	/// <param name="password">User password.</param>
	public static async Task AuthenticateAsAsync(HttpClient client, string email, string password)
	{
		var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
		{
			email,
			password
		});
		loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"login for {email} should succeed");

		var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
		var token = loginContent.GetProperty("accessToken").GetString();
		client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
	}
}
