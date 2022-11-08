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
	public class UserTests
	{
		private readonly DatabaseProvider databaseProvider;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<UnitTest1> logger;

		public UserTests(DatabaseProvider databaseProvider, IHttpClientFactory httpClientFactory, ILogger<UnitTest1> logger)
		{
			this.databaseProvider = databaseProvider;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[Fact]
		public async Task UserCanUpdateTheirProfile()
		{
			const string firstName = "Alfred";
			const string lastName = "Smith";
			const string pronouns = "he, him";

			var requestBuilder = new RequestBuilder(httpClientFactory, logger);

			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, requestBuilder);

			var user = await requestBuilder.GetModelAsync<User>("/api/v1/users/current_user");
			Assert.NotNull(user);
			Assert.Null(user.FirstName);
			Assert.Null(user.LastName);
			var role = Assert.Single(user.Roles);
			Assert.Equal(UserAccessType.Referee, role.AccessType);

			user = await requestBuilder.PatchModelAsync<User>($"/api/v1/referees/{user.Id}", new()
			{
				["bio"] = null,
				["export_name"] = true,
				["first_name"] = firstName,
				["last_name"] = lastName,
				["ngb_data"] = new Dictionary<int, string> { [3] = "primary" },
				["pronouns"] = pronouns,
				["show_pronouns"] = true,
				["submitted_payment_at"] = null,
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
		}
	}
}
