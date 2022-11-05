using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.API.Test.WebsiteClient.HttpMessageMiddleware;

namespace Service.API.Test.WebsiteClient;

public class RequestBuilder
{
    public static readonly string HttpClientName = $"{nameof(RequestBuilder)}_HttpClient";
    public enum User
    {
        Anonymous,
        Referee,
        NgbAdmin,
        IqaAdmin
    }

    private User user = User.Anonymous;
    private bool signed_in = false;
    private string path = "";
    private readonly HttpClient httpClient;
    private readonly ILogger logger;
    private readonly Guid CookieContainerId = Guid.NewGuid();

    public RequestBuilder(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        this.httpClient = httpClientFactory.CreateClient(HttpClientName);
        this.logger = logger;
    }

    public RequestBuilder Anonymous()
    {
        this.user = User.Anonymous;
        return this;
    }

    public RequestBuilder AsIqaAdmin()
    {
        this.user = User.IqaAdmin;
        return this;
    }

    public RequestBuilder AsNgbAdmin()
    {
        this.user = User.NgbAdmin;
        return this;
    }

    public RequestBuilder AsReferee()
    {
        this.user = User.Referee;
        return this;
    }

    public RequestBuilder WithEndpoint(string path)
    {
        this.path = path;
        return this;
    }

    public async Task<TResult?> GetFromJsonAsync<TResult>()
    {
        try
        {
            await this.SignIn();
            return await this.httpClient.GetFromJsonAsync<TResult>(this.path);
        }
        catch (Exception ex)
        {
            this.logger.LogError(0xd0ff400, ex, "Error occurred in GET {path}", this.path);
            throw;
        }
    }

    public async Task<string> GetStringAsync()
    {
        try
        {
            await this.SignIn();
            return await this.GetStringAsync(this.path);
        }
        catch (Exception ex)
        {
            this.logger.LogError(0xd0ff401, ex, "Error occurred in GET {path}", this.path);
            throw;
        }
    }

    private async Task SignIn()
    {
        if (signed_in) return;

        string email;
        string password = "password";
        switch (this.user)
        {
            case User.Anonymous: return;
            case User.Referee:
                email = "referee@example.com";
                break;
            case User.NgbAdmin:
                email = "ngb_admin@example.com";
                break;
            case User.IqaAdmin:
                email = "iqa_admin@example.com";
                break;
            default: throw new NotSupportedException($"Invalid value of {this.user}");
        }

        var signInPage = await this.GetStringAsync("/sign_in");
        var csrfTokenMatch = new Regex("<input type=\"hidden\" name=\"authenticity_token\" value=\"(?<token>.*)\"").Match(signInPage);
        if (!csrfTokenMatch.Success)
        {
            this.logger.LogError(0xd0ff402, "Unable to parse CSRF token from /sign_in page.");
            throw new Exception("Unable to parse CSRF token");
        }

        var csrfToken = csrfTokenMatch.Groups["token"].Captures[0].Value;

        var signInRequestData = new Dictionary<string, string> {
            {"utf8", "%E2%9C%93"},
            {"authenticity_token", csrfToken},
            {"user[email]", email},
            {"user[password]", password},
            {"user[remember_me]", "0"},
            {"commit", "Log in"},
        };
        var signInRequest = new FormUrlEncodedContent(signInRequestData);
        logger.LogInformation(
            0xd0ff403,
            "Starting /sign_in with content: {content}",
            string.Join("&", signInRequestData.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        await this.PostAsync("/sign_in", signInRequest);

        var user = await this.GetStringAsync("api/v1/users/current_user");
        this.signed_in = true;
    }

    private Task PostAsync(string path, HttpContent content) => SendAsync(HttpMethod.Post, path, content);
    private async Task<string> GetStringAsync(string path)
    {
        var response = await SendAsync(HttpMethod.Get, path);
        return await response.Content.ReadAsStringAsync();
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (content != null)
        {
            request.Content = content;
        }

        request.Options.Set(CookieSessionMessageHandler.CookieContainerIdOption, this.CookieContainerId);

        return this.httpClient.SendAsync(request);
    }
}