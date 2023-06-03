using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

/// <summary>
/// Base class for <see cref="EnsureDatabaseCreatedService"/> and <see cref="EnsureDatabaseMigratedService"/>
/// which encapsulates creating the db context for database related operations.
/// </summary>
public abstract class DatabaseStartupService : BackgroundService
{
	private readonly IServiceProvider serviceProvider;
	protected readonly ILogger logger;

	public DatabaseStartupService(IServiceProvider serviceProvider, ILogger logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using var scope = this.serviceProvider.CreateAsyncScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await this.ExecuteAsync(dbContext, stoppingToken);
	}

	protected abstract Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken);
}
