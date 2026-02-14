using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Enums;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Referee;

public class UpdateRefereeRoleCommand : IUpdateRefereeRoleCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<UpdateRefereeRoleCommand> logger;
	private readonly IDatabaseTransactionProvider transactionProvider;
	private readonly ISystemClock clock;

	public UpdateRefereeRoleCommand(ManagementHubDbContext dbContext, ILogger<UpdateRefereeRoleCommand> logger, IDatabaseTransactionProvider transactionProvider, ISystemClock clock)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.transactionProvider = transactionProvider;
		this.clock = clock;
	}

	public async Task UpdateRefereeRoleAsync(UserIdentifier userId, Func<RefereeRole, RefereeRole> updater, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x5fd20700, "Performing update on referee ({userId})", userId);

		await using var transaction = await this.transactionProvider.BeginAsync();

		var currentRefereeRole = await this.dbContext.Users.WithIdentifier(userId)
			.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.AccessType == UserAccessType.Referee))
			.Include(u => u.RefereeLocations).ThenInclude(rl => rl.NationalGoverningBody)
			.Include(u => u.RefereeTeams).ThenInclude(rl => rl.Team)
			.Select(u => new RefereeRole
			{
				IsActive = true,
				CoachingTeam = u.RefereeTeams.Where(rt => rt.AssociationType == RefereeTeamAssociationType.Coach).Select(rt => new TeamIdentifier(rt.Team!.Id)).Cast<TeamIdentifier?>().FirstOrDefault(),
				PlayingTeam = u.RefereeTeams.Where(rt => rt.AssociationType == RefereeTeamAssociationType.Player).Select(rt => new TeamIdentifier(rt.Team!.Id)).Cast<TeamIdentifier?>().FirstOrDefault(),
				NationalTeam = u.RefereeTeams.Where(rt => rt.AssociationType == RefereeTeamAssociationType.NationalTeamPlayer).Select(rt => new TeamIdentifier(rt.Team!.Id)).Cast<TeamIdentifier?>().FirstOrDefault(),
				PrimaryNgb = u.RefereeLocations.Where(rt => rt.AssociationType == RefereeNgbAssociationType.Primary).Select(rt => NgbIdentifier.Parse(rt.NationalGoverningBody.CountryCode)).Cast<NgbIdentifier?>().FirstOrDefault(),
				SecondaryNgb = u.RefereeLocations.Where(rt => rt.AssociationType == RefereeNgbAssociationType.Secondary).Select(rt => NgbIdentifier.Parse(rt.NationalGoverningBody.CountryCode)).Cast<NgbIdentifier?>().FirstOrDefault(),
			})
			.SingleAsync(cancellationToken);

		var newRefereeRole = updater(currentRefereeRole);

		const int numberOfPropertiesOfExtendedUserData = 6;
		var propertyNames = new List<string>(capacity: numberOfPropertiesOfExtendedUserData);

		bool updateIsActive = false;
		bool updateCoachingTeam = false;
		bool updatePlayingTeam = false;
		bool updateNationalTeam = false;
		bool updatePrimaryNgb = false;
		bool updateSecondaryNgb = false;

		if (newRefereeRole.IsActive != currentRefereeRole.IsActive)
		{
			updateIsActive = true;
			propertyNames.Add(nameof(newRefereeRole.IsActive));
		}

		if (newRefereeRole.CoachingTeam != currentRefereeRole.CoachingTeam)
		{
			updateCoachingTeam = true;
			propertyNames.Add(nameof(newRefereeRole.CoachingTeam));
		}

		if (newRefereeRole.PlayingTeam != currentRefereeRole.PlayingTeam)
		{
			updatePlayingTeam = true;
			propertyNames.Add(nameof(newRefereeRole.PlayingTeam));
		}

		if (newRefereeRole.NationalTeam != currentRefereeRole.NationalTeam)
		{
			updateNationalTeam = true;
			propertyNames.Add(nameof(newRefereeRole.NationalTeam));
		}

		if (newRefereeRole.PrimaryNgb != currentRefereeRole.PrimaryNgb)
		{
			updatePrimaryNgb = true;
			propertyNames.Add(nameof(newRefereeRole.PrimaryNgb));
		}

		if (newRefereeRole.SecondaryNgb != currentRefereeRole.SecondaryNgb)
		{
			updateSecondaryNgb = true;
			propertyNames.Add(nameof(newRefereeRole.SecondaryNgb));
		}

		if (propertyNames.Count == 0)
		{
			this.logger.LogInformation(0x5fd20701, "No changes have been made to the referee.");
			return;
		}

		this.logger.LogInformation(0x5fd20702, "Updating referee ({userId}) on properties: {propertyNames}.", userId, string.Join(", ", propertyNames));

		Models.Data.User? referee = null;

		if (updateIsActive)
		{
			throw new NotImplementedException("Updating IsActive is currently not supported.");
		}

		if (updateCoachingTeam)
		{
			if (currentRefereeRole.CoachingTeam != null)
			{
				var oldTeamId = currentRefereeRole.CoachingTeam.Value.Id;
				var team = this.dbContext.RefereeTeams
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeTeamAssociationType.Coach);

				if (newRefereeRole.CoachingTeam == null)
				{
					this.logger.LogInformation(0x5fd20703, "Removing coaching team ({teamId}) from referee ({userId}).", oldTeamId, userId);
					await team.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newTeamId = newRefereeRole.CoachingTeam.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0x5fd20704, "Updating coaching team ({oldTeamId} -> {newTeamId}) for referee ({userId}).", oldTeamId, newTeamId, userId);
					await team.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.TeamId, newTeamId)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				var newTeamId = newRefereeRole.CoachingTeam?.Id ?? throw new InvalidOperationException("Both current and new are null?");
				var now = this.clock.UtcNow.UtcDateTime;

				this.logger.LogInformation(0x5fd20705, "Adding coaching team ({teamId}) for referee ({userId}).", newTeamId, userId);
				this.dbContext.RefereeTeams.Add(new RefereeTeam
				{
					AssociationType = RefereeTeamAssociationType.Coach,
					Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
					TeamId = newTeamId,
					CreatedAt = now,
					UpdatedAt = now,
				});
				await this.dbContext.SaveChangesAsync(cancellationToken);
			}
		}

		if (updatePlayingTeam)
		{
			if (currentRefereeRole.PlayingTeam != null)
			{
				var oldTeamId = currentRefereeRole.PlayingTeam.Value.Id;
				var team = this.dbContext.RefereeTeams
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeTeamAssociationType.Player);

				if (newRefereeRole.PlayingTeam == null)
				{
					this.logger.LogInformation(0x5fd20706, "Removing playing team ({teamId}) from referee ({userId}).", oldTeamId, userId);
					await team.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newTeamId = newRefereeRole.PlayingTeam.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0x5fd20707, "Updating playing team ({oldTeamId} -> {newTeamId}) for referee ({userId}).", oldTeamId, newTeamId, userId);
					await team.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.TeamId, newTeamId)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				var newTeamId = newRefereeRole.PlayingTeam?.Id ?? throw new InvalidOperationException("Both current and new are null?");
				var now = this.clock.UtcNow.UtcDateTime;

				this.logger.LogInformation(0x5fd20708, "Adding playing team ({teamId}) for referee ({userId}).", newTeamId, userId);
				this.dbContext.RefereeTeams.Add(new RefereeTeam
				{
					AssociationType = RefereeTeamAssociationType.Player,
					Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
					TeamId = newTeamId,
					CreatedAt = now,
					UpdatedAt = now,
				});
				await this.dbContext.SaveChangesAsync(cancellationToken);
			}
		}

		if (updateNationalTeam)
		{
			if (currentRefereeRole.NationalTeam != null)
			{
				var oldTeamId = currentRefereeRole.NationalTeam.Value.Id;
				var team = this.dbContext.RefereeTeams
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeTeamAssociationType.NationalTeamPlayer);

				if (newRefereeRole.NationalTeam == null)
				{
					this.logger.LogInformation(0x5fd20711, "Removing national team ({teamId}) from referee ({userId}).", oldTeamId, userId);
					await team.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newTeamId = newRefereeRole.NationalTeam.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0x5fd20712, "Updating national team ({oldTeamId} -> {newTeamId}) for referee ({userId}).", oldTeamId, newTeamId, userId);
					await team.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.TeamId, newTeamId)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				var newTeamId = newRefereeRole.NationalTeam?.Id ?? throw new InvalidOperationException("Both current and new are null?");
				var now = this.clock.UtcNow.UtcDateTime;

				this.logger.LogInformation(0x5fd20713, "Adding national team ({teamId}) for referee ({userId}).", newTeamId, userId);
				this.dbContext.RefereeTeams.Add(new RefereeTeam
				{
					AssociationType = RefereeTeamAssociationType.NationalTeamPlayer,
					Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
					TeamId = newTeamId,
					CreatedAt = now,
					UpdatedAt = now,
				});
				await this.dbContext.SaveChangesAsync(cancellationToken);
			}
		}

		if (updatePrimaryNgb)
		{
			if (currentRefereeRole.PrimaryNgb != null)
			{
				var oldNgbId = currentRefereeRole.PrimaryNgb.Value.NgbCode;
				var ngb = this.dbContext.RefereeLocations
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeNgbAssociationType.Primary);

				if (newRefereeRole.PrimaryNgb == null)
				{
					this.logger.LogInformation(0x5fd20709, "Removing primary NGB ({ngbId}) from referee ({userId}).", oldNgbId, userId);
					await ngb.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newNgbId = newRefereeRole.PrimaryNgb.Value.NgbCode;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0x5fd2070a, "Updating primary NGB ({oldNgbId} -> {newNgbId}) for referee ({userId}).", oldNgbId, newNgbId, userId);
					if (currentRefereeRole.SecondaryNgb == newRefereeRole.PrimaryNgb)
					{
						if (!updateSecondaryNgb)
						{
							throw new InvalidOperationException("Referee can't have the same NGB set as both primary and secondary");
						}

						this.logger.LogInformation(0x5fd20710, "Swapping NGB from secondary to primary. Need to delete secondary first.");
						await this.dbContext.RefereeLocations
							.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
							.Where(t => t.AssociationType == RefereeNgbAssociationType.Secondary)
							.ExecuteDeleteAsync(cancellationToken);

						// set as null so that a new record will be added below
						currentRefereeRole.SecondaryNgb = null;
					}

					var newNgbIdLong = await this.GetNgbIdForUpdate(newRefereeRole.PrimaryNgb.Value, cancellationToken);
					await ngb.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.NationalGoverningBodyId, newNgbIdLong)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				var newNgbId = newRefereeRole.PrimaryNgb?.NgbCode ?? throw new InvalidOperationException("Both current and new are null?");
				var now = this.clock.UtcNow.UtcDateTime;

				this.logger.LogInformation(0x5fd2070b, "Adding primary NGB ({ngbId}) for referee ({userId}).", newNgbId, userId);
				var newNgbIdLong = await this.GetNgbIdForUpdate(newRefereeRole.PrimaryNgb.Value, cancellationToken);
				this.dbContext.RefereeLocations.Add(new RefereeLocation
				{
					AssociationType = RefereeNgbAssociationType.Primary,
					Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
					NationalGoverningBodyId = newNgbIdLong,
					CreatedAt = now,
					UpdatedAt = now,
				});
				await this.dbContext.SaveChangesAsync(cancellationToken);
			}
		}

		if (updateSecondaryNgb)
		{
			if (currentRefereeRole.SecondaryNgb != null)
			{
				var oldNgbId = currentRefereeRole.SecondaryNgb.Value.NgbCode;
				var ngb = this.dbContext.RefereeLocations
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeNgbAssociationType.Secondary);

				if (newRefereeRole.SecondaryNgb == null)
				{
					this.logger.LogInformation(0x5fd2070c, "Removing secondary NGB ({ngbId}) from referee ({userId}).", oldNgbId, userId);
					await ngb.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newNgbId = newRefereeRole.SecondaryNgb.Value.NgbCode;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0x5fd2070d, "Updating secondary NGB ({oldNgbId} -> {newNgbId}) for referee ({userId}).", oldNgbId, newNgbId, userId);
					var newNgbIdLong = await this.GetNgbIdForUpdate(newRefereeRole.SecondaryNgb.Value, cancellationToken);
					await ngb.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.NationalGoverningBodyId, newNgbIdLong)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				if (newRefereeRole.SecondaryNgb == null)
				{
					// secondary has been moved to primary and got removed
				}
				else
				{
					var newNgbId = newRefereeRole.SecondaryNgb.Value.NgbCode;
					var now = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0x5fd2070e, "Adding secondary NGB ({ngbId}) for referee ({userId}).", newNgbId, userId);
					var newNgbIdLong = await this.GetNgbIdForUpdate(newRefereeRole.SecondaryNgb.Value, cancellationToken);
					this.dbContext.RefereeLocations.Add(new RefereeLocation
					{
						AssociationType = RefereeNgbAssociationType.Secondary,
						Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
						NationalGoverningBodyId = newNgbIdLong,
						CreatedAt = now,
						UpdatedAt = now,
					});
					await this.dbContext.SaveChangesAsync(cancellationToken);
				}
			}
		}

		await transaction.CommitAsync(cancellationToken);

		this.logger.LogInformation(0x5fd2070f, "Successfully updated referee ({userId})", userId);
	}

	private Task<long> GetNgbIdForUpdate(NgbIdentifier ngbIdentifier, CancellationToken cancellationToken)
	{
		return this.dbContext.NationalGoverningBodies.WithIdentifier(ngbIdentifier).Select(ngb => ngb.Id).SingleAsync(cancellationToken);
	}
}
