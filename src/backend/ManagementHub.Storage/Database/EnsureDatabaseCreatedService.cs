using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

/// <summary>
/// Service which ensures the database defined in the EF context is created.
/// </summary>
/// <remarks>
/// This should be used for when database is created by the service. For migration based long-lived db see <see cref="EnsureDatabaseMigratedService"/>.
/// </remarks>
public class EnsureDatabaseCreatedService : DatabaseStartupService
{
	public EnsureDatabaseCreatedService(IServiceProvider serviceProvider, ILogger<EnsureDatabaseCreatedService> logger)
		: base(serviceProvider, logger)
	{
	}

	protected override async Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
		try
		{
			this.logger.LogInformation(0x6b0bd000, "Ensuring database is created...");

			await dbContext.Database.EnsureCreatedAsync(stoppingToken);

			this.logger.LogInformation(0x6b0bd001, "Ensuring database is created completed.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x6b0bd002, ex, "Error while creating database.");
			throw;
		}
	}
}
