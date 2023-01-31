using System.Diagnostics;
using ManagementHub.Service.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ManagementHub.Service;

public static class Program
{
    public static void Main(string[] args)
    {
		var builder = WebHost.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration(BuildConfiguration)
			.ConfigureServices(ConfigureServices)
			.ConfigureServices(ConfigureWebServices)
			.Configure(ConfigureWebApp);

        var app = builder.Build();

        app.Run();
	}

	private static void BuildConfiguration(WebHostBuilderContext context, IConfigurationBuilder builder)
	{
		// configuration order matters - the last provider overrides the previous ones

		// appsettings.json and appsettings.<env>.json are added via WebHost.CreateDefaultBuilder()

		// add configuration specified with custom environment variables (e.g. in production by Heroku)
		builder.Add(KnownEnvironmentVariablesConfigurationProvider.Source);
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
