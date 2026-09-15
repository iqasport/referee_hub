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
using ManagementHub.Models.Enums;
using ManagementHub.Storage.Extensions;
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
		UserIdentifier userId,
		RefereeTeamAssociationType requestedAssociationType,
		CancellationToken cancellationToken)
	{
		var currentUserDbId = await this.dbContext.Users
			.WithIdentifier(userId)
			.Select(user => user.Id)
			.SingleAsync(cancellationToken);

		var teamSettings = await this.dbContext.Teams
			.Where(team => team.Id == teamId.Id)
			.Select(team => new
			{
				team.Id,
				team.Name,
				team.AutoApprovePlayerRequests,
				team.NationalGoverningBodyId,
				team.GroupAffiliation,
			})
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

		// Determine origin team: current membership OR most recent PlayerRemoved activity.
		var originTeamId = await this.ResolveOriginTeamIdAsync(currentUserDbId, cancellationToken);

		// Determine if internal transfer (both teams share the same NGB).
		bool? isInternalTransfer = null;
		long? originNgbId = null;
		if (originTeamId.HasValue)
		{
			originNgbId = await this.dbContext.Teams
				.Where(t => t.Id == originTeamId.Value)
				.Select(t => (long?)t.NationalGoverningBodyId)
				.FirstOrDefaultAsync(cancellationToken);

			isInternalTransfer = originNgbId.HasValue
				&& teamSettings.NationalGoverningBodyId.HasValue
				&& originNgbId.Value == teamSettings.NationalGoverningBodyId.Value;
		}

		var requestedAt = DateTime.UtcNow;
		var invitation = new ManagementHub.Models.Data.TeamInvitation
		{
			TeamId = teamId.Id,
			Email = normalizedEmail,
			InitiatorUserId = currentUserDbId,
			CreatedAt = requestedAt,
			OriginTeamId = originTeamId,
			IsInternalTransfer = isInternalTransfer,
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

		// NGB transfer approvals apply only to playing-team transfer requests and
		// not to destination national teams.
		var requiresNgbTransferApproval =
			requestedAssociationType == RefereeTeamAssociationType.Player
			&& teamSettings.GroupAffiliation != TeamGroupAffiliation.National;
		IReadOnlyCollection<NgbIdentifier> pendingNgbApprovals = [];

		if (requiresNgbTransferApproval && originTeamId.HasValue)
		{
			pendingNgbApprovals = await this.CreateNgbTransferApprovalsAsync(
				invitation,
				originNgbId,
				teamSettings.NationalGoverningBodyId,
				isInternalTransfer == true,
				requestedAt,
				cancellationToken);
		}

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
			teamSettings.Name,
			new TeamInvitationIdentifier(invitation.Id),
			pendingNgbApprovals);
	}

	/// <summary>
	/// Returns the origin team ID for a player:
	/// 1. Their current player membership (if any).
	/// 2. Their most recent PlayerRemoved activity (if they were removed but have no current team).
	/// Returns null for players who have never been on a team.
	/// </summary>
	private async Task<long?> ResolveOriginTeamIdAsync(long userId, CancellationToken cancellationToken)
	{
		var currentTeamId = await this.dbContext.RefereeTeams
			.Where(rt => rt.RefereeId == userId && rt.AssociationType == RefereeTeamAssociationType.Player)
			.Select(rt => rt.TeamId)
			.FirstOrDefaultAsync(cancellationToken);

		if (currentTeamId.HasValue)
		{
			return currentTeamId;
		}

		// Player has no current team — look up their last known team from activity history.
		return await this.dbContext.TeamPlayerActivities
			.Where(a => a.UserId == userId && a.ActivityType == TeamPlayerActivityType.PlayerRemoved)
			.OrderByDescending(a => a.CreatedAt)
			.Select(a => (long?)a.TeamId)
			.FirstOrDefaultAsync(cancellationToken);
	}

	/// <summary>
	/// Creates NgbTransferApproval records for the NGBs involved in the transfer.
	/// Auto-approves internal transfers when the NGB has that setting enabled.
	/// </summary>
	private async Task<IReadOnlyCollection<NgbIdentifier>> CreateNgbTransferApprovalsAsync(
		ManagementHub.Models.Data.TeamInvitation invitation,
		long? originNgbId,
		long? destinationNgbId,
		bool isInternalTransfer,
		DateTime createdAt,
		CancellationToken cancellationToken)
	{
		// Load NGB auto-approve settings in a single query.
		var ngbIds = new[] { originNgbId, destinationNgbId }
			.Where(id => id.HasValue)
			.Select(id => id!.Value)
			.Distinct()
			.ToArray();

		var ngbSettings = await this.dbContext.NationalGoverningBodies
			.Where(ngb => ngbIds.Contains(ngb.Id))
			.Select(ngb => new { ngb.Id, ngb.CountryCode, ngb.AutoApproveInternalTransfers })
			.ToDictionaryAsync(ngb => ngb.Id, cancellationToken);
		var pendingNgbApprovals = new List<NgbIdentifier>();

		// Create an approval record for each NGB involved.
		foreach (var (ngbId, isOrigin) in new[] { (originNgbId, true), (destinationNgbId, false) })
		{
			if (!ngbId.HasValue)
			{
				continue;
			}

			// For international transfers the destination and origin NGBs differ — both need records.
			// For internal transfers there is only one NGB but we still create two records so the
			// query stays uniform; duplicate is prevented by the unique index on (invitation, ngb).
			// Skip the duplicate for internal transfers.
			if (isInternalTransfer && !isOrigin)
			{
				continue;
			}

			var autoApprove = isInternalTransfer
				&& ngbSettings.TryGetValue(ngbId.Value, out var settings)
				&& settings.AutoApproveInternalTransfers;

			this.dbContext.NgbTransferApprovals.Add(new NgbTransferApproval
			{
				TeamInvitation = invitation,
				NgbId = ngbId.Value,
				IsOriginNgb = isOrigin,
				CreatedAt = createdAt,
				ApprovedAt = autoApprove ? createdAt : null,
			});

			if (!autoApprove && ngbSettings.TryGetValue(ngbId.Value, out settings))
			{
				pendingNgbApprovals.Add(new NgbIdentifier(settings.CountryCode));
			}
		}

		return pendingNgbApprovals;
	}
}
