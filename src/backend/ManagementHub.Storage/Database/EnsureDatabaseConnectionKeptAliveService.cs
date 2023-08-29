using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

/// <summary>
/// Service that keeps a connection to the db open for the duration of the process.
/// It's a workaound for Sqlite in memory shared database - <see href="https://stackoverflow.com/a/56367786" />
/// </summary>
public class EnsureDatabaseConnectionKeptAliveService : IHostedService
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<EnsureDatabaseConnectionKeptAliveService> logger;
	private AsyncServiceScope? serviceScope;
	private ManagementHubDbContext? dbContext;

	public EnsureDatabaseConnectionKeptAliveService(IServiceProvider serviceProvider, ILogger<EnsureDatabaseConnectionKeptAliveService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x5497d700, "Openning a db connection for the duration of the process.");
		this.serviceScope = this.serviceProvider.CreateAsyncScope();
		this.dbContext = this.serviceScope?.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await this.dbContext!.Database.OpenConnectionAsync();
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x5497d701, "Closing db connection.");
		this.dbContext = null;
		if (this.serviceScope != null)
		{
			await this.serviceScope.Value.DisposeAsync();
		}
	}
}
