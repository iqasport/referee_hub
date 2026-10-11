using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Services;
using ManagementHub.Storage.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Database;

public class EnsureUserSensitiveInfoEncryptedService : DatabaseStartupService
{
	private const int BatchSize = 500;
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

			var updatedCount = 0;
			long lastUserId = 0;
			while (true)
			{
				var users = await dbContext.Users
					.Where(user => user.Id > lastUserId && (user.Bio != null || user.Pronouns != null))
					.OrderBy(user => user.Id)
					.Take(BatchSize)
					.ToListAsync(stoppingToken);

				if (users.Count == 0)
				{
					break;
				}

				lastUserId = users[^1].Id;
				var updatedBatchCount = 0;
				foreach (var user in users)
				{
					if (user.Bio != null && !user.Bio.StartsWith(UserSensitiveInfoProtection.ProtectedPrefix, StringComparison.Ordinal))
					{
						user.Bio = this.sensitiveInfoProtector.Protect(user.Bio);
						updatedBatchCount++;
					}

					if (user.Pronouns != null && !user.Pronouns.StartsWith(UserSensitiveInfoProtection.ProtectedPrefix, StringComparison.Ordinal))
					{
						user.Pronouns = this.sensitiveInfoProtector.Protect(user.Pronouns);
						updatedBatchCount++;
					}
				}

				if (updatedBatchCount > 0)
				{
					await dbContext.SaveChangesAsync(stoppingToken);
					updatedCount += updatedBatchCount;
				}

				foreach (var user in users)
				{
					dbContext.Entry(user).State = EntityState.Detached;
				}
			}

			if (updatedCount > 0)
			{
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