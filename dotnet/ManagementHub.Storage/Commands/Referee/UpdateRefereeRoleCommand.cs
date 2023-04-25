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
		this.logger.LogInformation(0, "Performing update on referee ({userId})", userId);

		await using var transaction = await this.transactionProvider.BeginAsync();

		var currentRefereeRole = await this.dbContext.Users.WithIdentifier(userId)
			.Join(this.dbContext.Roles.Where(r => r.AccessType == UserAccessType.Referee), u => u.Id, r => r.UserId, (u, _) => u)
			.GroupJoin(this.dbContext.RefereeLocations, u => u.Id, l => l.RefereeId, (u, l) => new { User = u, Locations = l })
			.SelectMany(g => g.Locations.DefaultIfEmpty(), (g, location) => new { g.User, Location = location })
			.GroupJoin(this.dbContext.RefereeTeams, g => g.User.Id, t => t.RefereeId, (g, t) => new
			{
				g.User,
				g.Location,
				Teams = t,
			})
			.SelectMany(g => g.Teams.DefaultIfEmpty(), (g, team) => new
			{
				g.User,
				g.Location,
				Team = team,
			})
			.GroupBy(g => g.User.Id, (u, g) => new
			{
				PrimaryLocationId = g.Where(gx => gx.Location != null && gx.Location.AssociationType == RefereeNgbAssociationType.Primary).Select(gx => gx.Location.NationalGoverningBodyId).SingleOrDefault(),
				SecondaryLocationId = g.Where(gx => gx.Location != null && gx.Location.AssociationType == RefereeNgbAssociationType.Secondary).Select(gx => gx.Location.NationalGoverningBodyId).SingleOrDefault(),
				CoachingTeamId = g.Where(gx => gx.Team != null && gx.Team.AssociationType == RefereeTeamAssociationType.Coach).Select(gx => gx.Team.TeamId).SingleOrDefault(),
				PlayingTeamId = g.Where(gx => gx.Team != null && gx.Team.AssociationType == RefereeTeamAssociationType.Player).Select(gx => gx.Team.TeamId).SingleOrDefault(),
			})
			.Select(g => new RefereeRole
			{
				IsActive = true,
				CoachingTeam = g.CoachingTeamId != null ? new TeamIdentifier(g.CoachingTeamId.Value) : null,
				PlayingTeam = g.PlayingTeamId != null ? new TeamIdentifier(g.PlayingTeamId.Value) : null,
				PrimaryNgb = g.PrimaryLocationId != default ? new NgbIdentifier(g.PrimaryLocationId) : null,
				SecondaryNgb = g.SecondaryLocationId != default ? new NgbIdentifier(g.SecondaryLocationId) : null,
			})
			.SingleAsync(cancellationToken);

		var newRefereeRole = updater(currentRefereeRole);

		const int numberOfPropertiesOfExtendedUserData = 5;
		var propertyNames = new List<string>(capacity: numberOfPropertiesOfExtendedUserData);

		bool updateIsActive = false;
		bool updateCoachingTeam = false;
		bool updatePlayingTeam = false;
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
			this.logger.LogInformation(0, "No changes have been made to the referee.");
			return;
		}

		this.logger.LogInformation(0, "Updating referee ({userId}) on properties: {propertyNames}.", userId, string.Join(", ", propertyNames));

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
					this.logger.LogInformation(0, "Removing coaching team ({teamId}) from referee ({userId}).", oldTeamId, userId);
					await team.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newTeamId = newRefereeRole.CoachingTeam.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0, "Updating coaching team ({oldTeamId} -> {newTeamId}) for referee ({userId}).", oldTeamId, newTeamId, userId);
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
				
				this.logger.LogInformation(0, "Adding coaching team ({teamId}) for referee ({userId}).", newTeamId, userId);
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
					this.logger.LogInformation(0, "Removing playing team ({teamId}) from referee ({userId}).", oldTeamId, userId);
					await team.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newTeamId = newRefereeRole.PlayingTeam.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0, "Updating playing team ({oldTeamId} -> {newTeamId}) for referee ({userId}).", oldTeamId, newTeamId, userId);
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

				this.logger.LogInformation(0, "Adding playing team ({teamId}) for referee ({userId}).", newTeamId, userId);
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

		if (updatePrimaryNgb)
		{
			if (currentRefereeRole.PrimaryNgb != null)
			{
				var oldNgbId = currentRefereeRole.PrimaryNgb.Value.Id;
				var ngb = this.dbContext.RefereeLocations
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeNgbAssociationType.Primary);

				if (newRefereeRole.PrimaryNgb == null)
				{
					this.logger.LogInformation(0, "Removing primary NGB ({ngbId}) from referee ({userId}).", oldNgbId, userId);
					await ngb.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newNgbId = newRefereeRole.PrimaryNgb.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0, "Updating primary NGB ({oldNgbId} -> {newNgbId}) for referee ({userId}).", oldNgbId, newNgbId, userId);
					await ngb.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.NationalGoverningBodyId, newNgbId)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				var newNgbId = newRefereeRole.PrimaryNgb?.Id ?? throw new InvalidOperationException("Both current and new are null?");
				var now = this.clock.UtcNow.UtcDateTime;

				this.logger.LogInformation(0, "Adding primary NGB ({ngbId}) for referee ({userId}).", newNgbId, userId);
				this.dbContext.RefereeLocations.Add(new RefereeLocation
				{
					AssociationType = RefereeNgbAssociationType.Primary,
					Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
					NationalGoverningBodyId = newNgbId,
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
				var oldNgbId = currentRefereeRole.SecondaryNgb.Value.Id;
				var ngb = this.dbContext.RefereeLocations
					.Join(this.dbContext.Users.WithIdentifier(userId), t => t.RefereeId, u => u.Id, (t, _) => t)
					.Where(t => t.AssociationType == RefereeNgbAssociationType.Secondary);

				if (newRefereeRole.SecondaryNgb == null)
				{
					this.logger.LogInformation(0, "Removing secondary NGB ({ngbId}) from referee ({userId}).", oldNgbId, userId);
					await ngb.ExecuteDeleteAsync(cancellationToken);
				}
				else
				{
					var newNgbId = newRefereeRole.SecondaryNgb.Value.Id;
					var updatedAt = this.clock.UtcNow.UtcDateTime;

					this.logger.LogInformation(0, "Updating secondary NGB ({oldNgbId} -> {newNgbId}) for referee ({userId}).", oldNgbId, newNgbId, userId);
					await ngb.ExecuteUpdateAsync(t => t
						.SetProperty(x => x.NationalGoverningBodyId, newNgbId)
						.SetProperty(x => x.UpdatedAt, updatedAt),
						cancellationToken);
				}
			}
			else
			{
				var newNgbId = newRefereeRole.SecondaryNgb?.Id ?? throw new InvalidOperationException("Both current and new are null?");
				var now = this.clock.UtcNow.UtcDateTime;

				this.logger.LogInformation(0, "Adding secondary NGB ({ngbId}) for referee ({userId}).", newNgbId, userId);
				this.dbContext.RefereeLocations.Add(new RefereeLocation
				{
					AssociationType = RefereeNgbAssociationType.Secondary,
					Referee = referee ??= await this.dbContext.Users.WithIdentifier(userId).SingleAsync(cancellationToken),
					NationalGoverningBodyId = newNgbId,
					CreatedAt = now,
					UpdatedAt = now,
				});
				await this.dbContext.SaveChangesAsync(cancellationToken);
			}
		}

		await transaction.CommitAsync(cancellationToken);
	}
}
