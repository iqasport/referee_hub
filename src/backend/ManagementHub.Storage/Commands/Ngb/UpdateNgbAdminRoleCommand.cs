using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Ngb;

public class UpdateNgbAdminRoleCommand : IUpdateNgbAdminRoleCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<UpdateNgbAdminRoleCommand> logger;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;

	public UpdateNgbAdminRoleCommand(ManagementHubDbContext dbContext, ILogger<UpdateNgbAdminRoleCommand> logger, IDatabaseTransactionProvider databaseTransactionProvider)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.databaseTransactionProvider = databaseTransactionProvider;
	}

	public async Task<IUpdateNgbAdminRoleCommand.AddRoleResult> AddNgbAdminRoleAsync(NgbIdentifier ngb, Email email, bool createUserIfNotExists)
	{
		using var transaction = await this.databaseTransactionProvider.BeginAsync();
		bool userCreated = false;
		var user = await this.dbContext.Users.AsNoTracking().WithEmail(email)
			.Select(u => new Models.Data.User { Id = u.Id }).SingleOrDefaultAsync();
		if (user == null)
		{
			if (!createUserIfNotExists)
			{
				this.logger.LogInformation("User (by email) doesn't exist.");
				return IUpdateNgbAdminRoleCommand.AddRoleResult.UserDoesNotExist;
			}

			this.logger.LogInformation("User (by email) doesn't exist, but we're creating a new account for them.");
			user = new Models.Data.User
			{
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				// we will assume email is correct, because we need to later allow the user to reset their password
				ConfirmedAt = DateTime.UtcNow,

				Email = email.Value,
				//InvitedById = TODO: get current user id
			};
			this.dbContext.Users.Add(user);
			await this.dbContext.SaveChangesAsync();
			userCreated = true;
		}

		var ngbAdminRole = await this.dbContext.Roles.AsNoTracking()
			.Where(r => r.UserId == user.Id && r.AccessType == Models.Enums.UserAccessType.NgbAdmin)
			.FirstOrDefaultAsync();
		if (ngbAdminRole != null)
		{
			this.logger.LogInformation("User already has an NGB admin role.");
		}
		else
		{
			this.logger.LogInformation("Adding NGB admin role to user.");
			this.dbContext.Roles.Add(new Models.Data.Role
			{
				AccessType = Models.Enums.UserAccessType.NgbAdmin,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				UserId = user.Id,
			});
			await this.dbContext.SaveChangesAsync();
		}

		var ngbDbId = await this.dbContext.NationalGoverningBodies.AsNoTracking().WithIdentifier(ngb).Select(n => n.Id).SingleAsync();

		var ngbAssignment = await this.dbContext.NationalGoverningBodyAdmins.AsNoTracking()
			.Where(ngba => ngba.UserId == user.Id && ngba.NationalGoverningBodyId == ngbDbId)
			.FirstOrDefaultAsync();
		if (ngbAssignment != null)
		{
			this.logger.LogInformation("User already is assigned as this NGBs admin.");
			return IUpdateNgbAdminRoleCommand.AddRoleResult.RoleAdded;
		}

		this.logger.LogInformation("Adding NGB admin assignment.");
		this.dbContext.NationalGoverningBodyAdmins.Add(new Models.Data.NationalGoverningBodyAdmin
		{
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,

			NationalGoverningBodyId = ngbDbId,
			UserId = user.Id,
		});
		await this.dbContext.SaveChangesAsync();

		await transaction.CommitAsync();

		return userCreated
			? IUpdateNgbAdminRoleCommand.AddRoleResult.UserCreatedWithRole
			: IUpdateNgbAdminRoleCommand.AddRoleResult.RoleAdded;
	}

	public async Task<bool> DeleteNgbAdminRoleAsync(NgbIdentifier ngb, Email email)
	{
		using var transaction = await this.databaseTransactionProvider.BeginAsync();
		var user = await this.dbContext.Users.AsNoTracking().WithEmail(email)
			.Select(u => new Models.Data.User { Id = u.Id }).SingleOrDefaultAsync();
		if (user == null)
		{
			this.logger.LogInformation("User doesn't exist.");
			return false;
		}

		var ngbDbId = await this.dbContext.NationalGoverningBodies.AsNoTracking().WithIdentifier(ngb).Select(n => n.Id).SingleAsync();

		var ngbAssignment = await this.dbContext.NationalGoverningBodyAdmins
			.Where(ngba => ngba.UserId == user.Id && ngba.NationalGoverningBodyId == ngbDbId)
			.FirstOrDefaultAsync();
		if (ngbAssignment == null)
		{
			this.logger.LogInformation("User is not this NGBs admin.");
			return false;
		}

		this.dbContext.Remove(ngbAssignment);
		await this.dbContext.SaveChangesAsync();

		var otherNgbAssignments = await this.dbContext.NationalGoverningBodyAdmins
			.Where(ngba => ngba.UserId == user.Id && ngba.NationalGoverningBodyId != ngbDbId)
			.CountAsync();
		// user has no more NGBs assigned, remove the NGB admin role.
		if (otherNgbAssignments == 0)
		{
			var ngbAdminRole = await this.dbContext.Roles
				.Where(r => r.UserId == user.Id && r.AccessType == Models.Enums.UserAccessType.NgbAdmin)
				.FirstOrDefaultAsync();
			if (ngbAdminRole != null)
			{
				this.logger.LogInformation("Removing NGB admin role.");
				this.dbContext.Remove(ngbAdminRole);
				await this.dbContext.SaveChangesAsync();
			}
			else
			{
				this.logger.LogInformation("No NGB admin role was present.");
			}
		}

		await transaction.CommitAsync();

		return true;
	}
}
