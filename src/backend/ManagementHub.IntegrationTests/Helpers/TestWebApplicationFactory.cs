using System;
using System.IO;
using System.Threading.Tasks;
using ManagementHub.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

		// Load test-specific configuration
		builder.ConfigureAppConfiguration((context, config) =>
		{
			var testConfigPath = Path.Combine(
				Directory.GetCurrentDirectory(),
				"appsettings.Testing.json");

			if (File.Exists(testConfigPath))
			{
				config.AddJsonFile(testConfigPath, optional: false, reloadOnChange: false);
			}

			// Override database connection with container settings
			if (this._postgresContainer != null)
			{
				config.AddInMemoryCollection(new[]
				{
					new System.Collections.Generic.KeyValuePair<string, string?>(
						"DatabaseConnection:Host", this._postgresContainer.Hostname),
					new System.Collections.Generic.KeyValuePair<string, string?>(
						"DatabaseConnection:Port", this._postgresContainer.GetMappedPublicPort(5432).ToString()),
					new System.Collections.Generic.KeyValuePair<string, string?>(
						"DatabaseConnection:Database", DatabaseName),
					new System.Collections.Generic.KeyValuePair<string, string?>(
						"DatabaseConnection:Username", Username),
					new System.Collections.Generic.KeyValuePair<string, string?>(
						"DatabaseConnection:Password", Password),
				});
			}
		});

		builder.ConfigureServices(services =>
		{
			// No additional service overrides needed
			// The database configuration is handled by appsettings.Testing.json and container config
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
