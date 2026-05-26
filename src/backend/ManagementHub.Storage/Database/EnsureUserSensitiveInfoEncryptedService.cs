using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

public class EnsureUserSensitiveInfoEncryptedService : DatabaseStartupService
{
	private const string ProtectedPrefix = "dp:";
	private readonly IUserSensitiveInfoProtector sensitiveInfoProtector;

	public EnsureUserSensitiveInfoEncryptedService(IServiceProvider serviceProvider, IUserSensitiveInfoProtector sensitiveInfoProtector, ILogger<EnsureUserSensitiveInfoEncryptedService> logger)
		: base(serviceProvider, logger)
	{
		this.sensitiveInfoProtector = sensitiveInfoProtector;
	}

	protected override async Task ExecuteAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
		try
		{
			this.logger.LogInformation(0x57c2a100, "Ensuring user sensitive info is encrypted...");

			var users = await dbContext.Users
				.Where(user => user.Bio != null || user.Pronouns != null)
				.ToListAsync(stoppingToken);

			var updatedCount = 0;
			foreach (var user in users)
			{
				if (user.Bio != null && !user.Bio.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
				{
					user.Bio = this.sensitiveInfoProtector.Protect(user.Bio);
					updatedCount++;
				}

				if (user.Pronouns != null && !user.Pronouns.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
				{
					user.Pronouns = this.sensitiveInfoProtector.Protect(user.Pronouns);
					updatedCount++;
				}
			}

			if (updatedCount > 0)
			{
				await dbContext.SaveChangesAsync(stoppingToken);
				this.logger.LogInformation(0x57c2a101, "Encrypted {UpdatedCount} user sensitive info values.", updatedCount);
			}
			else
			{
				this.logger.LogInformation(0x57c2a102, "User sensitive info already encrypted.");
			}
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x57c2a103, ex, "Error while encrypting user sensitive info.");
			throw;
		}
	}
}