using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUpdateRefereeRoleCommand
{
	/// <summary>
	/// Updates the user's referee role for <paramref name="userId"/> by applying the <paramref name="updater"/> function.
	/// </summary>
	/// <param name="userId">Id of the user to update.</param>
	/// <param name="updater">Updating function.</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task UpdateRefereeRoleAsync(UserIdentifier userId, Func<RefereeRole, RefereeRole> updater, CancellationToken cancellationToken);
}
