using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.API.Test.DatabaseClient;
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
            const string email = "asmith@example.com";

            var requestBuilder = new RequestBuilder(httpClientFactory, logger);

            await UserDbHelpers.DeleteUser(email, this.databaseProvider, requireUserToExist: false);
            await requestBuilder.RegisterUserAsync(firstName, lastName, email);

            var user = await requestBuilder.GetModelAsync<User>("/api/v1/users/current_user");
            Assert.NotNull(user);
            Assert.Null(user.FirstName);
            Assert.Null(user.LastName);
            Assert.Single(user.Roles);
            Assert.Equal(UserAccessType.Referee, user.Roles.Single().AccessType);

            user = await requestBuilder.PatchModelAsync<User>($"/api/v1/referees/{user.Id}", new()
            {
                ["bio"] = null,
                ["export_name"] = true,
                ["first_name"] = firstName,
                ["last_name"] = lastName,
                ["ngb_data"] = new Dictionary<int, string> { [3] = "primary" },
                ["pronouns"] = "he, him",
                ["show_pronouns"] = true,
                ["submitted_payment_at"] = null,
                ["teams_data"] = new { },
            });

            Assert.NotNull(user);
            Assert.Equal(firstName, user.FirstName);
            Assert.Equal(lastName, user.LastName);
            Assert.Equal(true, user.ExportName);
            Assert.Single(user.RefereeLocations);
            Assert.Equal(3, user.RefereeLocations.Single().NationalGoverningBodyId);
            Assert.Equal(RefereeNgbAssociationType.Primary, user.RefereeLocations.Single().AssociationType);
            Assert.Equal("he, him", user.Pronouns);
            Assert.Equal(true, user.ShowPronouns);
            Assert.Null(user.SubmittedPaymentAt);
            Assert.Empty(user.RefereeTeams);

            await UserDbHelpers.DeleteUser(email, this.databaseProvider);
        }
    }
}
