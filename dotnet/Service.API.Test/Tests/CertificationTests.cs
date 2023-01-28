using System;
using System.Net.Http;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using Microsoft.Extensions.Logging;
using Service.API.Test.DatabaseClient;
using Service.API.Test.Helpers;
using Service.API.Test.WebsiteClient;
using Xunit;

namespace Service.API.Test.Tests
{
	/// <summary>
	/// /api/v1/certifications
	/// </summary
	public class CertificationTests
	{
		private readonly DatabaseProvider databaseProvider;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<CertificationTests> logger;

		public CertificationTests(DatabaseProvider databaseProvider, IHttpClientFactory httpClientFactory, ILogger<CertificationTests> logger)
		{
			this.databaseProvider = databaseProvider;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[Fact(Skip = "In Devise unauthenticated requests are redirected to the login page.")]
		public async Task Certifications_Index_WithoutUser_Returns401()
		{
			var reqBuilder = new RequestBuilder(httpClientFactory, logger).Anonymous();

			var ex = await Assert.ThrowsAsync<Exception>(() => reqBuilder.GetModelListAsync<Certification>("/api/v1/certifications"));
			Assert.Contains("Unauthorized", ex.Message);
		}

		[Fact]
		public async Task Certifications_Index_WithUser_ReturnsCertifications()
		{
			await using var userContext = await UserContext.NewRefereeAsync(databaseProvider, new RequestBuilder(httpClientFactory, logger));

			var certs = await userContext.WebClient.GetModelListAsync<Certification>("/api/v1/certifications");
			Assert.NotNull(certs);
			foreach(var certLevel in Enum.GetValues<CertificationLevel>())
			{
				foreach (var certVersion in Enum.GetValues<CertificationVersion>())
				{
					// Certification with each level and version was expected
					Assert.Contains(certs, c => c.Level == certLevel && c.Version == certVersion);
				}
			}
		}
	}
}
