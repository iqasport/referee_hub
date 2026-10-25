using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Storage.Database;

/// <summary>
/// Service which ensures the database defined in the EF context is up to date with the migrations.
/// </summary>
/// <remarks>
/// This should be used for when database is long-lived. For temporary db created by the service see <see cref="EnsureDatabaseCreatedService"/>.
/// </remarks>
public class EnsureDatabaseMigratedService : DatabaseStartupService
{
	private readonly IXmlEncryptor? xmlEncryptor;

	public EnsureDatabaseMigratedService(
		IServiceProvider serviceProvider,
		IOptions<KeyManagementOptions> keyManagementOptions,
		ILogger<EnsureDatabaseCreatedService> logger)
		: base(serviceProvider, logger)
	{
		this.xmlEncryptor = keyManagementOptions.Value.XmlEncryptor;
	}

	// IMPORTANT: This method must execute synchronously in order to block startup until migrations are applied
	protected override Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
		try
		{
			this.logger.LogInformation(0x5225da00, "Ensuring database is migrated...");

			var migrations = new List<string>(dbContext.Database.GetPendingMigrations());
			if (migrations.Count > 0)
			{
				this.logger.LogInformation(0x5225da01, "Applying database migrations: {migrations}", string.Join(", ", migrations));

				dbContext.Database.Migrate();

				this.logger.LogInformation(0x5225da02, "Database migrations have been successfully applied.");
			}
			else
			{
				this.logger.LogInformation(0x5225da03, "Database already up to date.");
			}

			this.ProtectPersistedDataProtectionKeys(dbContext);

			return Task.CompletedTask;
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x5225da04, ex, "Error while applying database migrations.");
			throw;
		}
	}

	private void ProtectPersistedDataProtectionKeys(ManagementHubDbContext dbContext)
	{
		if (this.xmlEncryptor == null)
		{
			return;
		}

		var updatedCount = 0;
		foreach (var key in dbContext.DataProtectionKeys.ToList())
		{
			if (DataProtectionKeyXmlProtector.Protect(key, this.xmlEncryptor))
			{
				updatedCount++;
			}
		}

		if (updatedCount > 0)
		{
			dbContext.SaveChanges();
			this.logger.LogInformation(0x5225da05, "Encrypted {UpdatedCount} persisted Data Protection keys.", updatedCount);
		}
	}
}
