using ManagementHub.Service.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ManagementHub.Service;

public static class Program
{
    public static void Main(string[] args)
    {
		var builder = new WebHostBuilder()
			.ConfigureAppConfiguration(builder => builder.Add(KnownEnvironmentVariablesConfigurationProvider.Source))
			.UseKestrel((context, options) =>
			{
				// load kestrel configuration from the "Hosting" section in appsettings.json
				options.Configure(context.Configuration.GetSection("Hosting"));
			})
			.ConfigureServices(ConfigureServices)
			.ConfigureServices(ConfigureWebServices)
			.Configure(ConfigureWebApp)
			.UseStartup<Startup>();

        var app = builder.Build();

        app.Run();
    }

	public static void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
	{

	}

	public static void ConfigureWebServices(WebHostBuilderContext context, IServiceCollection services)
	{
		services.AddControllers();
		services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(options =>
			{
				options.LoginPath = "/login";
				options.LogoutPath = "/logout";
				options.AccessDeniedPath = "/forbidden";
			});
		services.AddAuthorization(); // TODO: add policy
	}

	public static void ConfigureWebApp(WebHostBuilderContext context, IApplicationBuilder app)
	{
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseEndpoints(endpoints => endpoints.MapControllers());
		
	}
}
