using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands.Migrations;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Migrations;

public class UserIdMigrationCommand : IUserIdMigrationCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<UserIdMigrationCommand> logger;

	public UserIdMigrationCommand(ManagementHubDbContext dbContext, ILogger<UserIdMigrationCommand> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public async Task TryMigrateUserIdAsync(Email email, CancellationToken cancellationToken)
	{
		var userIdAndStatus = await this.dbContext.Users
			.Where(u => u.Email == email.Value)
			.Select(u => new { u.UniqueId, u.Id })
			.SingleOrDefaultAsync();

		if (userIdAndStatus == null)
		{
			this.logger.LogInformation(0x1eb9ea00, "Attempted user ID migration but user has not been found.");
			return;
		}
		else if (userIdAndStatus.UniqueId != null)
		{
			this.logger.LogInformation(0x1eb9ea01, "User already has unique ID - no migration needed.");
			return;
		}

		var newUserId = UserIdentifier.NewUserId();

		this.logger.LogInformation(0x1eb9ea02,
			"Migrating user ID from {legacyId} ({oldId}) to {newUserId}.",
			userIdAndStatus.Id,
			UserIdentifier.FromLegacyUserId(userIdAndStatus.Id).ToString(),
			newUserId);

		await this.dbContext.Users
			.Where(u => u.Id == userIdAndStatus.Id)
			.ExecuteUpdateAsync(update => update.SetProperty(u => u.UniqueId, newUserId.ToString()).SetProperty(u => u.UpdatedAt, DateTime.UtcNow));

		this.logger.LogInformation(0x1eb9ea03, "Migration completed.");
	}
}
