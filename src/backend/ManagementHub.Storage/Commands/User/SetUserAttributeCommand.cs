using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.User;

public class SetUserAttributeCommand : ISetUserAttributeCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<SetUserAttributeCommand> logger;
	private readonly ISystemClock clock;
	private readonly IDatabaseTransactionProvider transactionProvider;

	public SetUserAttributeCommand(ManagementHubDbContext dbContext, ILogger<SetUserAttributeCommand> logger, ISystemClock clock, IDatabaseTransactionProvider transactionProvider)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.clock = clock;
		this.transactionProvider = transactionProvider;
	}

	private async Task SetUserAttributeAsync(UserIdentifier userId, string prefix, string key, string attributeValue, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x5017fc00, "Setting attribute {prefix}.{key} on user ({userId})", prefix, key, userId);

		await using var transaction = await this.transactionProvider.BeginAsync();

		var userDbId = await this.dbContext.Users.AsNoTracking().WithIdentifier(userId).Select(u => (long?)u.Id).SingleOrDefaultAsync(cancellationToken);
		if (userDbId == null)
		{
			throw new NotFoundException(userId.ToString());
		}

		var attribute = await this.dbContext.UserAttributes.SingleOrDefaultAsync(ua =>
			ua.UserId == userDbId &&
			ua.Prefix == prefix &&
			ua.Key == key,
			cancellationToken);

		if (attribute != null)
		{
			attribute.Attribute = attributeValue;
			attribute.UpdatedAt = this.clock.UtcNow.UtcDateTime;
		}
		else
		{
			attribute = new Models.Data.UserAttribute
			{
				UserId = userDbId.Value,
				Prefix = prefix,
				Key = key,
				Attribute = attributeValue,
				CreatedAt = this.clock.UtcNow.UtcDateTime,
				UpdatedAt = this.clock.UtcNow.UtcDateTime,
			};
			this.dbContext.Add(attribute);
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);
		await transaction.CommitAsync(cancellationToken);
	}

	public Task SetRootUserAttributeAsync(UserIdentifier userId, string key, JsonDocument document, CancellationToken cancellationToken)
	{
		return this.SetUserAttributeAsync(userId, prefix: string.Empty, key, document.RootElement.GetRawText(), cancellationToken);
	}

	Task ISetUserAttributeCommand.SetUserAttributeAsync(UserIdentifier userId, NgbIdentifier ngb, string key, JsonDocument document, CancellationToken cancellationToken)
	{
		return this.SetUserAttributeAsync(userId, prefix: ngb.NgbCode, key, document.RootElement.GetRawText(), cancellationToken);
	}
}
