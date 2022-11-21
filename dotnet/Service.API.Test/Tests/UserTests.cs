using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ManagementHub.Models;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Web;
using Microsoft.Extensions.Logging;
using Service.API.Test.DatabaseClient;
using Service.API.Test.Helpers;
using Service.API.Test.WebsiteClient;
using Xunit;

namespace Service.API.Test.Tests
{
	/// <summary>
	/// /api/v1/users
	/// </summary>
	public class UserTests
	{
		private readonly DatabaseProvider databaseProvider;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<UserTests> logger;

		public UserTests(DatabaseProvider databaseProvider, IHttpClientFactory httpClientFactory, ILogger<UserTests> logger)
		{
			this.databaseProvider = databaseProvider;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[Fact]
		public async Task CurrentUser_WhenUnauthenticated_ReturnsError()
		{
			var requestBuilder = new RequestBuilder(httpClientFactory, logger)
				.Anonymous();

			var exception = await Assert.ThrowsAnyAsync<Exception>(() => requestBuilder.GetModelAsync<WebUser>("/api/v1/users/current_user"));
			Assert.Equal("Error when getting data: status UnprocessableEntity", exception.Message);
		}

		[Fact]
		public async Task UserCanUpdateTheirLanguage()
		{
			var requestBuilder = new RequestBuilder(httpClientFactory, logger);

			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, requestBuilder);

			var user = await requestBuilder.GetModelAsync<WebUser>("/api/v1/users/current_user");
			Assert.NotNull(user);
			Assert.Null(user.Language);

			var languages = await requestBuilder.GetModelListAsync<Language>("/api/v1/languages");
			Assert.NotEmpty(languages!);

			user = await requestBuilder.PatchModelAsync<WebUser>($"/api/v1/users/{user.Id}", new()
			{
				["language_id"] = languages!.First().Id,
			});
			Assert.NotNull(user);

			user = await requestBuilder.GetModelAsync<WebUser>("/api/v1/users/current_user");
			Assert.NotNull(user);
			Assert.NotNull(user.Language);
		}

		[Fact]
		public async Task AcceptPolicies_SetsPoliciesAsAccepted()
		{
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger));

			var user = await userContext.WebClient.GetModelAsync<WebUser>($"/api/v1/users/current_user");
			Assert.NotNull(user);
			await userContext.WebClient.PostModelAsync<WebUser>($"/api/v1/users/{user.Id}/accept_policies", new());

			var policies = await userContext.Database.GetPoliciesAsync(userContext.Email);

			Assert.Collection(policies, t => Assert.Equal("accepted", t.State));
		}

		[Fact(Skip = "Privacy policy acceptance is enforced (required on UX during sign up and set to accepted by default on the server).")]
		public async Task RejectPolicies_SetsPoliciesAsRejected()
		{
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger));

			var user = await userContext.WebClient.GetModelAsync<WebUser>($"/api/v1/users/current_user");
			Assert.NotNull(user);
			await userContext.WebClient.PostModelAsync<WebUser>($"/api/v1/users/{user.Id}/reject_policies", new());

			var policies = await userContext.Database.GetPoliciesAsync(userContext.Email);

			Assert.Collection(policies, t => Assert.Equal("rejected", t.State));
		}

		[Fact]
		public async Task UpdateAvatar_StoresImage()
		{
			const string fileName = "myAvatar.jpg";

			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger));

			var user = await userContext.WebClient.GetModelAsync<WebUser>($"/api/v1/users/current_user");
			Assert.NotNull(user);

			user = await userContext.WebClient.PostModelAsync<WebUser>($"/api/v1/users/{user.Id}/update_avatar", new()
			{
				["bytes"] = Array.Empty<byte>(),
				["name"] = "avatar",
				["file"] = fileName,
			}, RequestBuilder.ContentType.Multipart);

			Assert.NotNull(user);
			Assert.Contains(fileName, user.AvatarUrl);
		}
	}
}
