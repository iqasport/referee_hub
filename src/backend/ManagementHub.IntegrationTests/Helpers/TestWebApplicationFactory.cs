using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ManagementHub.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Xunit;

namespace ManagementHub.IntegrationTests.Helpers;

/// <summary>
/// Custom WebApplicationFactory that uses Testcontainers for PostgreSQL.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	private PostgreSqlContainer? _postgresContainer;
	private const string DatabaseName = "managementhub_test";
	private const string Username = "postgres";
	private const string Password = "postgres";

	protected override IWebHostBuilder CreateWebHostBuilder()
	{
		// Explicitly call your Program's CreateWebHostBuilder with empty args
		return Program.CreateWebHostBuilder(Array.Empty<string>());
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");

		builder.ConfigureAppConfiguration((context, config) =>
		{
			// Configure all test settings in code
			var settings = new Dictionary<string, string?>
			{
				// Logging settings
				["Logging:LogLevel:Default"] = "Information",
				["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
				["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
				
				// Service configuration
				["Services:UseInMemoryDatabase"] = "false",
				["Services:SeedDatabaseWithTestData"] = "true",
				["Services:UseInMemoryJobSystem"] = "true",
				["Services:UseLocalFilesystemBlobStorage"] = "true",
				["Services:UseDebugMailer"] = "true",
			};

			// Add database connection settings from container
			if (this._postgresContainer != null)
			{
				settings["DatabaseConnection:Host"] = this._postgresContainer.Hostname;
				settings["DatabaseConnection:Port"] = this._postgresContainer.GetMappedPublicPort(5432).ToString();
				settings["DatabaseConnection:Database"] = DatabaseName;
				settings["DatabaseConnection:Username"] = Username;
				settings["DatabaseConnection:Password"] = Password;
				settings["DatabaseConnection:TrustServerCertificate"] = "true";
			}

			config.AddInMemoryCollection(settings);
		});

		builder.ConfigureServices(services =>
		{
			// Seeding is now synchronous, so default timeout should be sufficient
			// Keeping a reasonable timeout for safety
			services.Configure<Microsoft.Extensions.Hosting.HostOptions>(options =>
			{
				options.ShutdownTimeout = TimeSpan.FromSeconds(30);
				options.StartupTimeout = TimeSpan.FromSeconds(30);
			});
		});
	}

	/// <summary>
	/// Initialize the PostgreSQL container before tests run.
	/// </summary>
	public async Task InitializeAsync()
	{
		this._postgresContainer = new PostgreSqlBuilder()
			.WithImage("postgres:16-alpine")
			.WithDatabase(DatabaseName)
			.WithUsername(Username)
			.WithPassword(Password)
			.Build();

		await this._postgresContainer.StartAsync();
	}

	/// <summary>
	/// Dispose of the PostgreSQL container after tests complete.
	/// </summary>
	public new async Task DisposeAsync()
	{
		if (this._postgresContainer != null)
		{
			await this._postgresContainer.DisposeAsync();
		}

		await base.DisposeAsync();
	}
}
