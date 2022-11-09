using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ManagementHub.Models;
using ManagementHub.Models.Enums;
using Microsoft.Extensions.Logging;
using Service.API.Test.DatabaseClient;
using Service.API.Test.Helpers;
using Service.API.Test.WebsiteClient;
using Xunit;

namespace Service.API.Test.Tests
{
	/// <summary>
	/// /api/v1/referees
	/// </summary>
	public class RefereeTests
	{
		private readonly DatabaseProvider databaseProvider;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<RefereeTests> logger;

		public RefereeTests(DatabaseProvider databaseProvider, IHttpClientFactory httpClientFactory, ILogger<RefereeTests> logger)
		{
			this.databaseProvider = databaseProvider;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[Fact]
		public async Task Referee_RequestsDetailsForTheirId_ReturnsSuccessfully()
		{
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger));

			var user = await userContext.WebClient.GetModelAsync<User>("/api/v1/users/current_user");
			Assert.NotNull(user);

			user = await userContext.WebClient.GetModelAsync<User>($"/api/v1/referees/{user.Id}");
			Assert.NotNull(user);
		}

		[Fact]
		public async Task Referee_RequestsDetailsForOtherId_Returns401()
		{
			await using var userContext1 = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 1);
			await using var userContext2 = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 2);

			var user2 = await userContext2.WebClient.GetModelAsync<User>("/api/v1/users/current_user");
			Assert.NotNull(user2);

			var exception = await Assert.ThrowsAnyAsync<Exception>(() => userContext1.WebClient.GetModelAsync<User>($"/api/v1/referees/{user2.Id}"));
			Assert.Equal("Error when getting data: status Unauthorized", exception.Message);
		}

		[Fact]
		public async Task NgbAdmin_RequestsDetailsForOtherId_ReturnsSuccessfully()
		{
			// get a valid NGB to create its admin
			var ngb = (await new RequestBuilder(httpClientFactory, logger)
				.Anonymous()
				.GetModelListAsync<NationalGoverningBody>("/api/v1/national_governing_bodies"))![0];

			await using var ngbAdminContext = await UserContext.NewNgbAdminAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), ngb.Country!, seed: 1);
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 2);

			var user = await userContext.WebClient.GetModelAsync<User>("/api/v1/users/current_user");
			Assert.NotNull(user);

			user = await ngbAdminContext.WebClient.GetModelAsync<User>($"/api/v1/referees/{user.Id}");
			Assert.NotNull(user);
		}

		[Fact]
		public async Task IqaAdmin_RequestsDetailsForOtherId_ReturnsSuccessfully()
		{
			await using var adminContext = await UserContext.NewIqaAdminAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 1);
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 2);

			var user = await userContext.WebClient.GetModelAsync<User>("/api/v1/users/current_user");
			Assert.NotNull(user);

			user = await adminContext.WebClient.GetModelAsync<User>($"/api/v1/referees/{user.Id}");
			Assert.NotNull(user);
		}
	}
}
