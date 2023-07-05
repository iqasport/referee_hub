using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

/// <summary>
/// Service which ensures the database defined in the EF context is up to date with the migrations.
/// </summary>
/// <remarks>
/// This should be used for when database is long-lived. For temporary db created by the service see <see cref="EnsureDatabaseCreatedService"/>.
/// </remarks>
public class EnsureDatabaseMigratedService : DatabaseStartupService
{
	public EnsureDatabaseMigratedService(IServiceProvider serviceProvider, ILogger<EnsureDatabaseCreatedService> logger)
		: base(serviceProvider, logger)
	{
	}

	// IMPORTANT: This method must execute synchronously in order to block startup until migrations are applied
	protected override Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
		try
		{
			this.logger.LogInformation(0, "Ensuring database is migrated...");

			var migrations = new List<string>(dbContext.Database.GetPendingMigrations());
			if (migrations.Count > 0)
			{
				this.logger.LogInformation(0, "Applying database migrations: {migrations}", string.Join(", ", migrations));

				dbContext.Database.Migrate();

				this.logger.LogInformation(0, "Database migrations have been successfully applied.");
			}
			else
			{
				this.logger.LogInformation(0, "Database already up to date.");
			}

			return Task.CompletedTask;
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Error while applying database migrations.");
			throw;
		}
	}
}
