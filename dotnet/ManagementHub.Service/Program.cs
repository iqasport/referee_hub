using Hangfire;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Processing.Domain.Tests.Policies.Extensions;
using ManagementHub.Serialization;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Configuration;
using ManagementHub.Service.Contexts;
using ManagementHub.Storage.DependencyInjection;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using static System.Net.Mime.MediaTypeNames;

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
		services.AddmanagementHubBlobStorage(localFileSystem: true);
		services.AddManagementHubIdentity();

		services.AddHttpContextAccessor();
		services.AddScoped<IUserContextAccessor, UserContextAccessor>();
		services.AddScoped<IRefereeContextAccessor, RefereeContextAccessor>();

		services.AddTestPolicies();

		services.AddScoped<IAuthorizationHandler, UserRoleAuthorizationHandler>();
		services.AddAuthorization(options =>
		{
			options.AddRefereePolicy();
			options.AddRefereeViewerPolicy();
		});

		services.AddHangfireServer();
    }

    public static void ConfigureWebServices(WebHostBuilderContext context, IServiceCollection services)
	{
		services.AddControllers()
			.AddJsonOptions(options => DefaultJsonSerialization.ConfigureOptions(options.JsonSerializerOptions));
		services.AddRazorPages();
		services.AddDefaultIdentity<UserIdentity>(options => options.SignIn.RequireConfirmedAccount = false); // TODO: set it based on environment
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
			options.ExpireTimeSpan = TimeSpan.FromDays(1);

			options.LoginPath = "/login";
			options.LogoutPath = "/logout";
			options.AccessDeniedPath = "/";
			options.SlidingExpiration = true;
			OverrideRedirectsForApiEndpoints(options);
		});
		services.AddSwaggerGen(options => DefaultJsonSerialization.MapSwaggerTypes(options));
    }

	private static void OverrideRedirectsForApiEndpoints(CookieAuthenticationOptions options)
	{
		var onRedirectToLogin = options.Events.OnRedirectToLogin;
		options.Events.OnRedirectToLogin = async (context) =>
		{
			if (context.Request.Path.StartsWithSegments("/api"))
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync("Unauthorized");
				return;
			}
			else
			{
				await onRedirectToLogin(context);
			}
		};

		var onRedirectToAccessDenied = options.Events.OnRedirectToAccessDenied;
		options.Events.OnRedirectToAccessDenied = async (context) =>
		{
			if (context.Request.Path.StartsWithSegments("/api"))
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync("Forbidden");
				return;
			}
			else
			{
				await onRedirectToAccessDenied(context);
			}
		};
	}

	public static void ConfigureWebApp(WebHostBuilderContext context, IApplicationBuilder app)
	{
		app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(HandleRequestError));
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
			endpoints.MapRazorPages();
			endpoints.MapSwagger();
			endpoints.MapHangfireDashboard();
		});
		app.UseSwaggerUI();
	}

	private static async Task HandleRequestError(HttpContext context)
	{
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;

		context.Response.ContentType = Text.Plain;

		var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

		// TODO move this method to another file and refactor common code
		switch (exceptionHandlerPathFeature?.Error)
		{
			case NotFoundException notFound:
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				await context.Response.WriteAsync(notFound.Message);
				break;
			case AccessDeniedException accessDenied:
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync(accessDenied.Message);
				break;
			case InvalidOperationException invalidOperation:
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync(invalidOperation.Message);
				break;
			default:
				await context.Response.WriteAsync("Unexpected error occured.");
				break;
		}
	}
}
