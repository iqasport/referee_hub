using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using Microsoft.Extensions.Logging;

using Service.API.Test.WebsiteClient.HttpMessageMiddleware;

namespace Service.API.Test.WebsiteClient;

public class RequestBuilder
{
	public static readonly string HttpClientName = $"{nameof(RequestBuilder)}_HttpClient";

	private User? user;
	private bool signed_in = false;
	private readonly HttpClient httpClient;
	private readonly ILogger logger;
	private readonly CookieContainer CookieContainer = new();

	public RequestBuilder(IHttpClientFactory httpClientFactory, ILogger logger)
	{
		this.httpClient = httpClientFactory.CreateClient(HttpClientName);
		this.logger = logger;
	}

	public RequestBuilder Anonymous()
	{
		this.user = null;
		return this;
	}

	public RequestBuilder AsIqaAdmin() => AsUser("iqa_admin@example.com");

	public RequestBuilder AsNgbAdmin() => AsUser("ngb_admin@example.com");

	public RequestBuilder AsReferee() => AsUser("referee@example.com");

	public RequestBuilder AsUser(string email)
	{
		this.user = new User { Email = email };
		return this;
	}

	public async Task<TResult?> GetModelAsync<TResult>(string path) where TResult : class
	{
		try
		{
			await this.SignIn();
			var content = await this.GetAsync(path);
			return await content.ReadModelAsync<TResult>();
		}
		catch (Exception ex)
		{
			this.logger.LogError(0xd0ff404, ex, "Error occurred in GET {path}", path);
			throw;
		}
	}

	public async Task<List<TResult>?> GetModelListAsync<TResult>(string path) where TResult : class
	{
		try
		{
			await this.SignIn();
			var content = await this.GetAsync(path);
			return await content.ReadModelListAsync<TResult>();
		}
		catch (Exception ex)
		{
			this.logger.LogError(0xd0ff405, ex, "Error occurred in GET {path}", path);
			throw;
		}
	}

	public async Task<string> GetStringAsync(string path, Dictionary<string, object?> properties = null, ContentType contentType = ContentType.Json)
	{
		try
		{
			await this.SignIn();
			if (properties == null)
				return await this.GetStringInternalAsync(path);
			else
				return await this.GetStringInternalAsync(path, this.CreateContent(properties, contentType));
		}
		catch (Exception ex)
		{
			this.logger.LogError(0xd0ff401, ex, "Error occurred in GET {path}", path);
			throw;
		}
	}

	public async Task<TResult?> PostModelAsync<TResult>(string path, Dictionary<string, object?> properties, ContentType contentType = ContentType.Json) where TResult : class
	{
		try
		{
			await this.SignIn();
			var content = await this.PostAsync(path, CreateContent(properties, contentType));
			return await content.ReadModelAsync<TResult>();
		}
		catch (Exception ex)
		{
			this.logger.LogError(0xd0ff409, ex, "Error occurred in POST {path}", path);
			throw;
		}
	}

	public async Task<TResult?> PatchModelAsync<TResult>(string path, Dictionary<string, object?> properties, ContentType contentType = ContentType.Json) where TResult : class
	{
		try
		{
			await this.SignIn();
			var content = await this.PatchAsync(path, CreateContent(properties, contentType));
			return await content.ReadModelAsync<TResult>();
		}
		catch (Exception ex)
		{
			this.logger.LogError(0xd0ff408, ex, "Error occurred in PATCH {path}", path);
			throw;
		}
	}

	public async Task RegisterUserAsync(string firstName, string lastName, string email)
	{
		string password = "password";

		var signUpPage = await this.GetStringInternalAsync("/sign_up");
		var authenticityTokenMatch = new Regex("<input type=\"hidden\" name=\"authenticity_token\" value=\"(?<token>.*)\"").Match(signUpPage);
		if (!authenticityTokenMatch.Success)
		{
			this.logger.LogError(0xd0ff406, "Unable to parse CSRF token from /sign_in page.");
			throw new Exception("Unable to parse CSRF token");
		}

		var authenticityToken = authenticityTokenMatch.Groups["token"].Captures[0].Value;

		var signUpRequestData = new Dictionary<string, string> {
			{"utf8", "%E2%9C%93"},
			{"authenticity_token", authenticityToken},
			{"user[first_name]", firstName},
			{"user[last_name]", lastName},
			{"user[email]", email},
			{"user[password]", password},
			{"user[password_confirmation]", password},
			{"user[policy_rule_privacy_terms]", "true"},
			{"commit", "Create Account"},
		};

		logger.LogInformation(
			0xd0ff407,
			"Starting /register with content: {content}",
			string.Join("&", signUpRequestData.Select(kvp => $"{kvp.Key}={kvp.Value}")));

		var signUpRequest = new FormUrlEncodedContent(signUpRequestData);
		var postContent = await this.PostAsync("/register", signUpRequest);
		var postString = await postContent.ReadAsStringAsync();
		if (postString.Contains("error-message"))
		{
			var errorMatch = new Regex("<div id='error_explanation' class='error-message'>(?<error>.*?)</div>", RegexOptions.Singleline).Match(postString);
			var error = errorMatch.Groups["error"].Captures[0].Value;

			throw new Exception($"There was an error during register: {error}");
		}

		this.signed_in = true;
	}

	private async Task SignIn()
	{
		if (this.user == null) return;
		if (signed_in) return;

		string email = this.user.Email;
		string password = "password";

		var signInPage = await this.GetStringInternalAsync("/sign_in");
		var authenticityTokenMatch = new Regex("<input type=\"hidden\" name=\"authenticity_token\" value=\"(?<token>.*)\"").Match(signInPage);
		if (!authenticityTokenMatch.Success)
		{
			this.logger.LogError(0xd0ff402, "Unable to parse CSRF token from /sign_in page.");
			throw new Exception("Unable to parse CSRF token");
		}

		var authenticityToken = authenticityTokenMatch.Groups["token"].Captures[0].Value;

		var signInRequestData = new Dictionary<string, string> {
			{"utf8", "%E2%9C%93"},
			{"authenticity_token", authenticityToken},
			{"user[email]", email},
			{"user[password]", password},
			{"user[remember_me]", "0"},
			{"commit", "Log in"},
		};

		logger.LogInformation(
			0xd0ff403,
			"Starting /sign_in with content: {content}",
			string.Join("&", signInRequestData.Select(kvp => $"{kvp.Key}={kvp.Value}")));

		var signInRequest = new FormUrlEncodedContent(signInRequestData);
		await this.PostAsync("/sign_in", signInRequest);

		var user = await this.GetStringInternalAsync("api/v1/users/current_user");
		this.signed_in = true;
	}

	private async Task<HttpContent> PostAsync(string path, HttpContent content)
	{
		var response = await SendAsync(HttpMethod.Post, path, content);
		await AssertSuccessfulResponseAsync(response);
		return response.Content;
	}

	private async Task<string> GetStringInternalAsync(string path, HttpContent? content = null)
	{
		var response = await SendAsync(HttpMethod.Get, path, content);
		await AssertSuccessfulResponseAsync(response);
		return await response.Content.ReadAsStringAsync();
	}

	private async Task<HttpContent> GetAsync(string path)
	{
		var response = await SendAsync(HttpMethod.Get, path);
		await AssertSuccessfulResponseAsync(response);
		return response.Content;
	}

	private async Task<HttpContent> PatchAsync(string path, HttpContent content)
	{
		var response = await SendAsync(HttpMethod.Patch, path, content);
		await AssertSuccessfulResponseAsync(response);
		return response.Content;
	}

	private static async Task AssertSuccessfulResponseAsync(HttpResponseMessage response)
	{
		if (!response.IsSuccessStatusCode) throw new Exception($"Error when getting data: status {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
	}

	private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content = null)
	{
		var request = new HttpRequestMessage(method, path);
		if (content != null)
		{
			request.Content = content;
		}

		request.Options.Set(CookieSessionMessageHandler.CookieContainerOption, this.CookieContainer);

		return this.httpClient.SendAsync(request);
	}

	private HttpContent CreateContent(Dictionary<string, object?> data, ContentType contentType)
	{
		switch (contentType)
		{
			case ContentType.Json:
				return JsonContent.Create(data);
			case ContentType.FormEncoded:
				return new FormUrlEncodedContent(data.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value?.ToString())));
			case ContentType.Multipart:
				var content = new MultipartFormDataContent($"----WebKitFormBoundary");
				content.Add(
					new ByteArrayContent(data.TryGetValue("bytes", out var byteData) ? byteData as byte[] ?? Array.Empty<byte>() : Array.Empty<byte>()),
					data.TryGetValue("name", out var name) ? name?.ToString() ?? "" : "",
					data.TryGetValue("file", out var file) ? file?.ToString() ?? "" : "");
				return content;
			default:
				throw new NotSupportedException();
		}
	}

	public enum ContentType
	{
		Json,
		FormEncoded,
		Multipart,
	}
}
