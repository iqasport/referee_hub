using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Team;

public class UpdateTeamManagerRoleCommand : IUpdateTeamManagerRoleCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<UpdateTeamManagerRoleCommand> logger;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;

	public UpdateTeamManagerRoleCommand(
		ManagementHubDbContext dbContext,
		ILogger<UpdateTeamManagerRoleCommand> logger,
		IDatabaseTransactionProvider databaseTransactionProvider)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.databaseTransactionProvider = databaseTransactionProvider;
	}

	public async Task<IUpdateTeamManagerRoleCommand.AddRoleResult> AddTeamManagerRoleAsync(
		TeamIdentifier teamId,
		Email email,
		bool createUserIfNotExists,
		UserIdentifier addedByUserId)
	{
		using var transaction = await this.databaseTransactionProvider.BeginAsync();
		bool userCreated = false;

		// Get or create user
		var user = await this.dbContext.Users.AsNoTracking()
			.WithEmail(email)
			.Select(u => new Models.Data.User { Id = u.Id })
			.FirstOrDefaultAsync();

		if (user == null)
		{
			if (!createUserIfNotExists)
			{
				this.logger.LogInformation("User not found");
				return IUpdateTeamManagerRoleCommand.AddRoleResult.UserDoesNotExist;
			}

			this.logger.LogInformation("Creating user");
			user = new Models.Data.User
			{
				Email = email.Value,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				ConfirmedAt = DateTime.UtcNow,
			};
			this.dbContext.Users.Add(user);
			await this.dbContext.SaveChangesAsync();
			userCreated = true;

			this.logger.LogInformation("User created with ID {UserId}", user.Id);
		}

		// Get team database ID
		var teamDbId = teamId.Id;

		// Check if already a manager
		var existingManager = await this.dbContext.TeamManagers
			.Where(tm => tm.TeamId == teamDbId && tm.UserId == user.Id)
			.FirstOrDefaultAsync();

		if (existingManager != null)
		{
			this.logger.LogInformation("User {UserId} already is a manager of team {TeamId}",
				user.Id, teamId);
			return userCreated
				? IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole
				: IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded;
		}

		// Get current user (NGB admin) ID
		var currentUserDbId = await this.dbContext.Users.AsNoTracking()
			.WithIdentifier(addedByUserId)
			.Select(u => u.Id)
			.FirstOrDefaultAsync();

		if (currentUserDbId == 0)
		{
			throw new InvalidOperationException("Current user not found");
		}

		// Add team manager
		this.logger.LogInformation("Adding team manager for user {UserId} to team {TeamId}",
			user.Id, teamId);

		this.dbContext.TeamManagers.Add(new Models.Data.TeamManager
		{
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			TeamId = teamDbId,
			UserId = user.Id,
			AddedByUserId = currentUserDbId,
		});

		await this.dbContext.SaveChangesAsync();
		await transaction.CommitAsync();

		return userCreated
			? IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole
			: IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded;
	}

	public async Task<bool> DeleteTeamManagerRoleAsync(TeamIdentifier teamId, Email email)
	{
		using var transaction = await this.databaseTransactionProvider.BeginAsync();

		var user = await this.dbContext.Users.AsNoTracking()
			.WithEmail(email)
			.Select(u => new Models.Data.User { Id = u.Id })
			.FirstOrDefaultAsync();

		if (user == null)
		{
			this.logger.LogInformation("User not found");
			return false;
		}

		// Get team database ID
		var teamDbId = teamId.Id;

		// Find and remove team manager assignment
		var manager = await this.dbContext.TeamManagers
			.Where(tm => tm.TeamId == teamDbId && tm.UserId == user.Id)
			.FirstOrDefaultAsync();

		if (manager == null)
		{
			this.logger.LogInformation(
				"User {UserId} is not a manager of team {TeamId}",
				user.Id, teamId);
			return false;
		}

		this.logger.LogInformation(
			"Removing team manager assignment for user {UserId} from team {TeamId}",
			user.Id, teamId);

		this.dbContext.TeamManagers.Remove(manager);
		await this.dbContext.SaveChangesAsync();
		await transaction.CommitAsync();

		return true;
	}
}
