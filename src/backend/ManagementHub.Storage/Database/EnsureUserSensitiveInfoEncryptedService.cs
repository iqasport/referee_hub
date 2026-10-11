using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Services;
using ManagementHub.Models.Data;
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
			var updatedCount = await this.EncryptUsersInBatchesAsync(dbContext, stoppingToken);
			this.LogEncryptionResult(updatedCount);
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x57c2a103, ex, "Error while encrypting user sensitive info.");
			throw;
		}
	}

	private async Task<int> EncryptUsersInBatchesAsync(ManagementHubDbContext dbContext, CancellationToken stoppingToken)
	{
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
			updatedCount += await this.EncryptUserBatchAsync(dbContext, users, stoppingToken);
		}

		return updatedCount;
	}

	private async Task<int> EncryptUserBatchAsync(ManagementHubDbContext dbContext, List<User> users, CancellationToken stoppingToken)
	{
		var updatedCount = users.Sum(this.EncryptUserSensitiveInfo);
		if (updatedCount > 0)
		{
			await dbContext.SaveChangesAsync(stoppingToken);
		}

		foreach (var user in users)
		{
			dbContext.Entry(user).State = EntityState.Detached;
		}

		return updatedCount;
	}

	private int EncryptUserSensitiveInfo(User user)
	{
		var updatedCount = 0;
		if (user.Bio != null && !user.Bio.StartsWith(UserSensitiveInfoProtection.ProtectedPrefix, StringComparison.Ordinal))
		{
			user.Bio = this.sensitiveInfoProtector.Protect(user.Bio);
			updatedCount++;
		}

		if (user.Pronouns != null && !user.Pronouns.StartsWith(UserSensitiveInfoProtection.ProtectedPrefix, StringComparison.Ordinal))
		{
			user.Pronouns = this.sensitiveInfoProtector.Protect(user.Pronouns);
			updatedCount++;
		}

		return updatedCount;
	}

	private void LogEncryptionResult(int updatedCount)
	{
		if (updatedCount > 0)
		{
			this.logger.LogInformation(0x57c2a101, "Encrypted {UpdatedCount} user sensitive info values.", updatedCount);
		}
		else
		{
			this.logger.LogInformation(0x57c2a102, "User sensitive info already encrypted.");
		}
	}
}
