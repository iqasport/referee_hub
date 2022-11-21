using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.API.Test.DatabaseClient;
using Service.API.Test.EmailClient;
using Service.API.Test.Settings;
using Service.API.Test.WebsiteClient;
using Service.API.Test.WebsiteClient.HttpMessageMiddleware;

using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Service.API.Test;

/// <summary>
/// Startup class used by <see cref="Xunit.DependencyInjection"/> to configure
/// the host for running tests.
/// </summary>
public class Startup
{
	/// <summary>
	/// Configures the dependency injection services collection.
	/// </summary>
	public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
	{
		services.AddOptions();
		services.Configure<WebsiteClientSettings>(context.Configuration.GetRequiredSection("WebsiteClient"));
		services.Configure<DatabaseSettings>(context.Configuration.GetRequiredSection("Database"));
		services.Configure<EmailClientSettings>(context.Configuration.GetSection("EmailClient"));

		services.AddScoped<DatabaseProvider>();

		services.AddSingleton<EmailProvider>();

		services.AddTransient<FollowRedirectsMessageHandler>();
		services.AddTransient<CookieSessionMessageHandler>();

		services.AddHttpClient(RequestBuilder.HttpClientName, (serviceProvider, client) =>
		{
			var settings = serviceProvider.GetRequiredService<IOptions<WebsiteClientSettings>>().Value;
			client.BaseAddress = settings.BaseUrl;
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(settings.UserAgent, "1.0"));
		})
		.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			AllowAutoRedirect = false,
			UseCookies = false,
		})
		// Ordering of the handlers is important, from top (outermost) to bottom (innermost)
		.AddHttpMessageHandler<FollowRedirectsMessageHandler>()
		.AddHttpMessageHandler<CookieSessionMessageHandler>();
	}

	/// <summary>
	/// Configures the configuration source and sets up logging configuration.
	/// </summary>
	public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
		.ConfigureAppConfiguration(configBuilder =>
			configBuilder.AddJsonFile("appsettings.json"))
		.ConfigureLogging((context, loggingBuilder) =>
			loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging")));

	/// <summary>
	/// Configures XUnit specific logging adapter and disables filtering out Debug logs.
	/// </summary>
	public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
		loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor, (_, _) => true));
}
