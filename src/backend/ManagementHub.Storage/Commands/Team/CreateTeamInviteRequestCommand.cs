using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Commands.Team;

public class CreateTeamInviteRequestCommand : ICreateTeamInviteRequestCommand
{
	private readonly ManagementHubDbContext dbContext;

	public CreateTeamInviteRequestCommand(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task<ICreateTeamInviteRequestCommand.CreateResult> CreateTeamInviteRequestAsync(
		TeamIdentifier teamId,
		string normalizedEmail,
		long currentUserDbId,
		CancellationToken cancellationToken)
	{
		var teamSettings = await this.dbContext.Teams
			.Where(team => team.Id == teamId.Id)
			.Select(team => new { team.Id, team.Name, team.AutoApprovePlayerRequests })
			.FirstOrDefaultAsync(cancellationToken);

		if (teamSettings == null)
		{
			return new ICreateTeamInviteRequestCommand.CreateResult(
				ICreateTeamInviteRequestCommand.CreateResultCode.TeamNotFound);
		}

		var hasPendingRequest = await this.dbContext.TeamInvitations
			.AnyAsync(
				invite =>
					invite.TeamId == teamId.Id &&
					invite.Email.ToLower() == normalizedEmail &&
					invite.RevokedAt == null &&
					invite.AcceptedAt == null &&
					invite.DeclinedAt == null,
				cancellationToken);

		if (hasPendingRequest)
		{
			return new ICreateTeamInviteRequestCommand.CreateResult(
				ICreateTeamInviteRequestCommand.CreateResultCode.AlreadyPending);
		}

		var requestedAt = DateTime.UtcNow;
		var invitation = new ManagementHub.Models.Data.TeamInvitation
		{
			TeamId = teamId.Id,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			CreatedAt = requestedAt,
		};
		this.dbContext.TeamInvitations.Add(invitation);

		this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
		{
			TeamId = teamId.Id,
			UserId = currentUserDbId,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			ActivityType = TeamPlayerActivityType.InviteCreated,
			CreatedAt = requestedAt,
		});

		if (teamSettings.AutoApprovePlayerRequests)
		{
			var approvedAt = DateTime.UtcNow;
			invitation.AcceptedAt = approvedAt;
			invitation.RespondedByUserId = currentUserDbId;

			var existingPlayerMembership = await this.dbContext.RefereeTeams
				.FirstOrDefaultAsync(
					membership =>
						membership.RefereeId == currentUserDbId &&
						membership.AssociationType == RefereeTeamAssociationType.Player,
					cancellationToken);

			if (existingPlayerMembership == null)
			{
				this.dbContext.RefereeTeams.Add(new ManagementHub.Models.Data.RefereeTeam
				{
					AssociationType = RefereeTeamAssociationType.Player,
					RefereeId = currentUserDbId,
					TeamId = teamId.Id,
					CreatedAt = approvedAt,
					UpdatedAt = approvedAt,
				});
			}
			else if (existingPlayerMembership.TeamId != teamId.Id && existingPlayerMembership.TeamId.HasValue)
			{
				this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
				{
					TeamId = existingPlayerMembership.TeamId.Value,
					UserId = currentUserDbId,
					Email = normalizedEmail,
					InitiatorUserId = currentUserDbId,
					ActivityType = TeamPlayerActivityType.PlayerRemoved,
					CreatedAt = approvedAt,
				});

				existingPlayerMembership.TeamId = teamId.Id;
				existingPlayerMembership.UpdatedAt = approvedAt;
			}

			this.dbContext.TeamPlayerActivities.Add(new ManagementHub.Models.Data.TeamPlayerActivity
			{
				TeamId = teamId.Id,
				UserId = currentUserDbId,
				Email = normalizedEmail,
				InitiatorUserId = currentUserDbId,
				ActivityType = TeamPlayerActivityType.InviteAccepted,
				CreatedAt = approvedAt,
			});

			await this.dbContext.SaveChangesAsync(cancellationToken);

			return new ICreateTeamInviteRequestCommand.CreateResult(
				ICreateTeamInviteRequestCommand.CreateResultCode.AutoApproved);
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);

		return new ICreateTeamInviteRequestCommand.CreateResult(
			ICreateTeamInviteRequestCommand.CreateResultCode.RequestCreated,
			teamSettings.Name);
	}
}
