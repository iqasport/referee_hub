using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Areas.Identity;
using ManagementHub.Service.Configuration;
using ManagementHub.Storage.DependencyInjection;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

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
		services.AddManagementHubStorage(inMemoryStorage: true);
		services.AddManagementHubIdentity();
    }

    public static void ConfigureWebServices(WebHostBuilderContext context, IServiceCollection services)
	{
		services.AddControllers();
		services.AddRazorPages();
		services.AddDefaultIdentity<UserIdentity>(options => options.SignIn.RequireConfirmedAccount = true);
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 8;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

			const string emailConfirmationProvider = "EmailConfirmation";
			options.Tokens.EmailConfirmationTokenProvider = emailConfirmationProvider;
            options.Tokens.ProviderMap[emailConfirmationProvider] = new TokenProviderDescriptor(typeof(EmailTokenProvider));
        });
        services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
			options.ExpireTimeSpan = TimeSpan.FromDays(7);

            options.LoginPath = "/login";
			options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/";
            options.SlidingExpiration = true;
        });

		// custom overrides over user manager
		services.AddScoped<UserManager<UserIdentity>, UserManager>();
    }

	public static void ConfigureWebApp(WebHostBuilderContext context, IApplicationBuilder app)
	{
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
			endpoints.MapRazorPages();
		});
		
	}
}
