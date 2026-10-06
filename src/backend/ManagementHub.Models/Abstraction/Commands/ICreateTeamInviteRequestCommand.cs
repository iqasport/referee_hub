using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Abstraction.Commands;

public interface ICreateTeamInviteRequestCommand
{
	public enum CreateResultCode
	{
		/// <summary>The specified team does not exist.</summary>
		TeamNotFound,

		/// <summary>A pending invite for this email + team already exists.</summary>
		AlreadyPending,

		/// <summary>Invite created and queued for manager approval.</summary>
		RequestCreated,

		/// <summary>Invite created and automatically approved (team has auto-approve enabled).</summary>
		AutoApproved,
	}

	public record CreateResult(CreateResultCode Code, string? TeamName = null);

	/// <summary>
	/// Creates a team invite request on behalf of a referee (self-initiated join request).
	/// Handles duplicate prevention, activity logging, and auto-approval when the team enables it.
	/// Callers are responsible for dispatching notifications based on the returned result code.
	/// </summary>
	Task<CreateResult> CreateTeamInviteRequestAsync(
		TeamIdentifier teamId,
		string normalizedEmail,
		long currentUserDbId,
		CancellationToken cancellationToken);
}
