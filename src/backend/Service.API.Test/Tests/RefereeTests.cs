using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Web;
using Microsoft.Extensions.Logging;
using Service.API.Test.DatabaseClient;
using Service.API.Test.EmailClient;
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
		private readonly EmailProvider emailProvider;
		private readonly ILogger<RefereeTests> logger;

		public RefereeTests(DatabaseProvider databaseProvider, IHttpClientFactory httpClientFactory, EmailProvider emailProvider, ILogger<RefereeTests> logger)
		{
			this.databaseProvider = databaseProvider;
			this.httpClientFactory = httpClientFactory;
			this.emailProvider = emailProvider;
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

		[Fact]
		public async Task RefereeCanUpdateTheirProfile()
		{
			const string firstName = "Alfred";
			const string lastName = "Smith";
			const string pronouns = "he, him";

			var requestBuilder = new RequestBuilder(httpClientFactory, logger);

			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, requestBuilder);

			var user = await requestBuilder.GetModelAsync<WebUser>("/api/v1/users/current_user");
			Assert.NotNull(user);
			Assert.Null(user.FirstName);
			Assert.Null(user.LastName);
			var role = Assert.Single(user.Roles);
			Assert.Equal(UserAccessType.Referee, role.AccessType);

			user = await requestBuilder.PatchModelAsync<WebUser>($"/api/v1/referees/{user.Id}", new()
			{
				["bio"] = null,
				["export_name"] = true,
				["first_name"] = firstName,
				["last_name"] = lastName,
				["pronouns"] = pronouns,
				["show_pronouns"] = true,
				["submitted_payment_at"] = null,
				["ngb_data"] = new Dictionary<int, string> { [3] = "primary" },
				["teams_data"] = new { },
			});

			Assert.NotNull(user);
			Assert.Equal(firstName, user.FirstName);
			Assert.Equal(lastName, user.LastName);
			Assert.Equal(true, user.ExportName);
			var location = Assert.Single(user.RefereeLocations);
			Assert.Equal(3, location.NationalGoverningBodyId);
			Assert.Equal(RefereeNgbAssociationType.Primary, location.AssociationType);
			Assert.Equal(pronouns, user.Pronouns);
			Assert.Equal(true, user.ShowPronouns);
			Assert.Null(user.SubmittedPaymentAt);
			Assert.Empty(user.RefereeTeams);

			user = await requestBuilder.GetModelAsync<WebUser>("/api/v1/users/current_user");
			Assert.NotNull(user);
			Assert.Equal(firstName, user.FirstName);
			Assert.Equal(lastName, user.LastName);
		}

		[Fact]
		public async Task IqaAdmin_ExportRefereeDetails_FromNgb_ReceivesEmail()
		{
			await using var adminContext = await UserContext.NewIqaAdminAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 1);
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger), seed: 2);

			var user = await userContext.WebClient.GetModelAsync<User>("/api/v1/users/current_user");
			Assert.NotNull(user);

			// Set NGB on user
			user = await userContext.WebClient.PatchModelAsync<WebUser>($"/api/v1/referees/{user.Id}", new()
			{
				["ngb_data"] = new Dictionary<int, string> { [3] = "primary" },
			});

			var exportJobString = await adminContext.WebClient.GetStringAsync($"/api/v1/referees_export/", new()
			{
				["national_governing_bodies"] = new[] { 3 },
			});

			var message = await emailProvider.PollAsync(
				message => message.ToAddresses.Any(addr => addr.Address == adminContext.Email) &&
							message.Subject == "Your Referee Export is ready");
			var body = Assert.Single(message.MessageParts).BodyData;
			Assert.Contains("We just finished exporting your CSV.", body);
		}
	}
}
