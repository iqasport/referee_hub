using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IReviewNgbTransferCommand
{
	public enum ReviewDecision
	{
		Approve,
		Reject,
	}

	public enum ReviewResultCode
	{
		NotFound,
		AlreadyReviewed,
		Reviewed,
	}

	Task<ReviewResultCode> ReviewAsync(
		NgbIdentifier ngb,
		TeamInvitationIdentifier invitationId,
		UserIdentifier reviewerUserId,
		ReviewDecision decision,
		CancellationToken cancellationToken);
}
