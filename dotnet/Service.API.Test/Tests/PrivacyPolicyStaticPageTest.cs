using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.API.Test.WebsiteClient;
using Xunit;

namespace Service.API.Test.Tests
{
	public class PrivacyPolicyStaticPageTest
	{
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<PrivacyPolicyStaticPageTest> logger;

		public PrivacyPolicyStaticPageTest(IHttpClientFactory httpClientFactory, ILogger<PrivacyPolicyStaticPageTest> logger)
		{
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[Fact]
		public async Task GetPrivacy_ReturnsPage()
		{
			var requestBuilder = new RequestBuilder(httpClientFactory, logger)
				.Anonymous();

			// The content of the page is loaded with JS so we only verify it finishes with a successful Http code.
			await requestBuilder.GetStringAsync("/privacy");
		}
	}
}
