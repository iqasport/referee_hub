using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Models.Abstraction.Commands.Mailers;

public interface ISendTeamInviteEmail
{
	Task SendTeamInviteEmailAsync(
		TeamIdentifier teamId,
		string invitedEmail,
		string? invitedByName,
		Uri hostUri,
		CancellationToken cancellationToken);

	Task SendTeamInviteResponseEmailAsync(
		TeamIdentifier teamId,
		string responderEmail,
		string? responderName,
		bool approved,
		Uri hostUri,
		CancellationToken cancellationToken);
}