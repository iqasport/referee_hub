using System;
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

public class ReviewNgbTransferCommand : IReviewNgbTransferCommand
{
	private readonly ManagementHubDbContext dbContext;

	public ReviewNgbTransferCommand(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task<IReviewNgbTransferCommand.ReviewResultCode> ReviewAsync(
		NgbIdentifier ngb,
		TeamInvitationIdentifier invitationId,
		UserIdentifier reviewerUserId,
		IReviewNgbTransferCommand.ReviewDecision decision,
		CancellationToken cancellationToken)
	{
		var approval = await this.dbContext.NgbTransferApprovals
			.Include(a => a.TeamInvitation)
			.FirstOrDefaultAsync(
				a => a.TeamInvitationId == invitationId.Id && a.Ngb.CountryCode == ngb.NgbCode,
				cancellationToken);

		if (approval == null)
		{
			return IReviewNgbTransferCommand.ReviewResultCode.NotFound;
		}

		if (approval.ApprovedAt != null || approval.RejectedAt != null)
		{
			return IReviewNgbTransferCommand.ReviewResultCode.AlreadyReviewed;
		}

		var reviewerUserDbId = await this.dbContext.Users
			.WithIdentifier(reviewerUserId)
			.Select(user => user.Id)
			.SingleAsync(cancellationToken);

		var reviewedAt = DateTime.UtcNow;
		if (decision == IReviewNgbTransferCommand.ReviewDecision.Approve)
		{
			approval.ApprovedAt = reviewedAt;
			approval.ReviewedByUserId = reviewerUserDbId;
		}
		else
		{
			approval.RejectedAt = reviewedAt;
			approval.ReviewedByUserId = reviewerUserDbId;

			var invitation = approval.TeamInvitation;
			if (invitation.RevokedAt == null && invitation.AcceptedAt == null && invitation.DeclinedAt == null)
			{
				invitation.RevokedAt = reviewedAt;

				var playerUserId = await this.dbContext.Users
					.Where(user => user.Email.ToLower() == invitation.Email.ToLower())
					.Select(user => (long?)user.Id)
					.FirstOrDefaultAsync(cancellationToken);

				this.dbContext.TeamPlayerActivities.Add(new TeamPlayerActivity
				{
					TeamId = invitation.TeamId,
					UserId = playerUserId,
					Email = invitation.Email,
					InitiatorUserId = reviewerUserDbId,
					ActivityType = TeamPlayerActivityType.InviteRevoked,
					CreatedAt = reviewedAt,
				});
			}
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);
		return IReviewNgbTransferCommand.ReviewResultCode.Reviewed;
	}
}
