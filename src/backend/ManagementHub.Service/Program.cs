using System.Diagnostics;
using DotNetEd.CoreAdmin;
using Hangfire;
using ManagementHub.Mailers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Models.Misc;
using ManagementHub.Processing.Domain.Tests.Policies.Extensions;
using ManagementHub.Processing.Export;
using ManagementHub.Serialization;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Configuration;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Telemetry;
using ManagementHub.Storage.DependencyInjection;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.AmbientMetadata;
using Microsoft.Extensions.Telemetry.Enrichment;
using Microsoft.Extensions.Telemetry.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using static System.Net.Mime.MediaTypeNames;

namespace ManagementHub.Service;

public static class Program
{
	private static readonly string Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;

	public static void Main(string[] args)
	{
		var builder = WebHost.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration(BuildConfiguration)
			.ConfigureLogging(ConfigureLogging)
			.ConfigureServices(ConfigureInstrumentation)
			.ConfigureServices(ConfigureServices)
			.ConfigureServices(ConfigureWebServices)
			.Configure(ConfigureWebApp);

		ConfigureDevelopmentTimeWebRoot(builder);

		var app = builder.Build();

		app.Run();
	}

	[Conditional("Debug")]
	private static void ConfigureDevelopmentTimeWebRoot(IWebHostBuilder builder)
	{
		// ContentRoot = current directory (in VS that it directory of the project; when deployed it should be folder containing the EXE)
		//    this is where appsettings files are read from for example
		// WebRoot is relative to content root
		//    this is where html/js files are read from

		if (Environment == Environments.Development)
		{
			// In development we will use the external folder for quicker dev-loop
			builder.UseWebRoot("../../frontend/dist");
		}
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
		services.AddManagementHubBlobStorage(localFileSystem: true);
		services.AddManagementHubIdentity();

		services.AddHangfire(inMemoryStorage: true);

		services.AddMailers(inMemory: true);

		services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUserGetter, CurrentUserGetter>();
		services.AddScoped<IUserContextAccessor, UserContextAccessor>();
		services.AddScoped<IRefereeContextAccessor, RefereeContextAccessor>();

		services.AddTestPolicies();
		services.AddExportProcessors();

		services.AddScoped<IAuthorizationHandler, UserRoleAuthorizationHandler>();
		services.AddAuthorization(options =>
		{
			options.AddRefereePolicy();
			options.AddRefereeViewerPolicy();
			options.AddTechAdminPolicy();
		});

		services.AddCoreAdmin(new CoreAdminOptions
		{
			CustomAuthorisationMethod = () => Task.FromResult(true),
			ShowPageSizes = true,
			Title = "Generic DB Admin portal",
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
		services.AddSwaggerGen(options =>
		{
			// Assign a fixed operation ID to each API endpoint to equal the name of the method.
			// Operation IDs must be unique and are used to generate operation names for the UX.
			options.CustomOperationIds(endpoint => endpoint.ActionDescriptor.RouteValues["action"]);
			DefaultJsonSerialization.MapSwaggerTypes(options);
		});

		services.AddSingleton<DistributedContextPropagator, CookieTraceContextPropagator>();
		services.AddScoped<TraceCookieMiddleware>();
		services.AddScoped<ImpersonationMiddleware>();
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
		var exceptionHandlerPipeline = app.New();
		exceptionHandlerPipeline.Run(HandleRequestError);
		app.UseExceptionHandler(new ExceptionHandlerOptions
		{
			AllowStatusCode404Response = true,
			ExceptionHandler = exceptionHandlerPipeline.Build(),
		});

		app.UseMiddleware<TraceCookieMiddleware>();
		app.UseStaticFiles();
		app.UseRouting();
		app.UseAuthentication();
		app.UseMiddleware<ImpersonationMiddleware>();
		app.UseAuthorization();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
			endpoints.MapRazorPages();
			endpoints.MapSwagger();
			endpoints.MapHangfireDashboard("/admin/jobs").RequireAuthorization(AuthorizationPolicies.TechAdminPolicy);
			endpoints.MapControllerRoute(
				name: "coreadminroute",
				pattern: "admin/db/{controller=CoreAdmin}/{action=Index}/{id?}")
				.RequireAuthorization(AuthorizationPolicies.TechAdminPolicy);
			endpoints.MapFallbackToFile("index.html");
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
			case AuthenticationRequiredException authRequired:
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync(authRequired.Message);
				break;
			case InvalidOperationException invalidOperation:
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync(invalidOperation.Message);
				break;
			case ArgumentException argument:
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync(argument.Message);
				break;
			default:
				await context.Response.WriteAsync("Unexpected error occured.");
				break;
		}
	}

	private static void ConfigureInstrumentation(WebHostBuilderContext context, IServiceCollection services)
	{
		// Adding the OtlpExporter creates a GrpcChannel.
		// This switch must be set before creating a GrpcChannel/HttpClient when calling an insecure gRPC service.
		// See: https://docs.microsoft.com/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client
		AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

		services.AddOpenTelemetry()
			.ConfigureResource(o => o.AddService("ManagementHub"))
			.WithTracing(builder => builder
				.AddAspNetCoreInstrumentation(options =>
				{
					options.RecordException = true;
					options.EnrichWithHttpRequest = (activity, req) =>
					{
						Baggage.SetBaggage("request", $"{req.Method} {activity.DisplayName}");
					};
					options.EnrichWithHttpResponse = (activity, resp) => activity.DisplayName = $"{activity.GetTagItem("http.method")} {activity.DisplayName} {resp.StatusCode}";
				})
				.AddEntityFrameworkCoreInstrumentation(options =>
				{
					options.SetDbStatementForText = true;
					options.SetDbStatementForStoredProcedure = true;
					options.EnrichWithIDbCommand = (activity, dbCommand) => activity.DisplayName = "efcore";
				})
				.AddHangfireInstrumentation(options =>
				{
					options.RecordException = true;
					options.DisplayNameFunc = (info) => $"JOB {info.Job.Method.Name.AsDisplayName()}";
				})
				.AddServiceTraceEnricher(options =>
				{
					options.ApplicationName = false;
					options.BuildVersion = true;
					options.EnvironmentName = true;
					options.DeploymentRing = false;
				})
				.AddSource("ManagementHub", "ManagementHub.Mailers"))
			.WithMetrics(builder => builder
				.AddAspNetCoreInstrumentation()
				.AddProcessInstrumentation()
				.AddRuntimeInstrumentation()
				.AddEventCountersInstrumentation(o => o.AddEventSources(/* TODO names of source? */)));

		services.AddApplicationMetadata(metadata =>
		{
			metadata.ApplicationName = context.HostingEnvironment.ApplicationName;
			metadata.BuildVersion = typeof(Program).Assembly.GetName().Version?.ToString();
			metadata.EnvironmentName = context.HostingEnvironment.EnvironmentName;
		});
		services.AddProcessLogEnricher();
		services.AddServiceLogEnricher(options =>
		{
			options.ApplicationName = false;
			options.BuildVersion = true;
			options.EnvironmentName = true;
			options.DeploymentRing = false;
		});

		var settings = context.Configuration.GetSection("Telemetry").Get<TelemetrySettings>() ?? new TelemetrySettings();

		if (settings.Exporter == "Otlp")
		{
			services.AddOpenTelemetry()
				.WithTracing(b => b.AddOtlpExporter(otlpOptions =>
				{
					otlpOptions.Endpoint = settings.OtlpEndpoint;
				}))
				.WithMetrics(b => b.AddOtlpExporter(otlpOptions =>
				{
					otlpOptions.Endpoint = settings.OtlpEndpoint;
				}));
		}
	}

	public static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder builder)
	{
		builder.AddOpenTelemetryLogging(options =>
		{
			options.IncludeStackTrace = true;
			options.UseFormattedMessage = true;
		});
		builder.Services.AddLogEnricher(new BaggageLogEnricher("request"));
		builder.Services.AddLogEnricher(new BaggageLogEnricher("user_id"));

		var settings = context.Configuration.GetSection("Telemetry").Get<TelemetrySettings>() ?? new TelemetrySettings();

		if (settings.Exporter == "Otlp")
		{
			var exporterOptions = new OtlpExporterOptions()
			{
				Endpoint = settings.OtlpEndpoint,
			};
			var batchOptions = exporterOptions.BatchExportProcessorOptions;
			var otlpExporter = (BaseExporter<LogRecord>?)Activator.CreateInstance(typeof(OtlpExporterOptions).Assembly.GetType("OpenTelemetry.Exporter.OtlpLogExporter")!, exporterOptions);
			builder.AddProcessor(new BatchLogRecordExportProcessor(
						otlpExporter!,
						batchOptions.MaxQueueSize,
						batchOptions.ScheduledDelayMilliseconds,
						batchOptions.ExporterTimeoutMilliseconds,
						batchOptions.MaxExportBatchSize));
		}
	}
}
