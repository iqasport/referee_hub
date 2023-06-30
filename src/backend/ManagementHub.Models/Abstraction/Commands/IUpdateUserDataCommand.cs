using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;
public interface IUpdateUserDataCommand
{
	/// <summary>
	/// Updates the user data for <paramref name="userId"/> by applying the <paramref name="updater"/> function.
	/// </summary>
	/// <param name="userId">Id of the user to update.</param>
	/// <param name="updater">Updating function.</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task UpdateUserDataAsync(UserIdentifier userId, Func<ExtendedUserData, ExtendedUserData> updater, CancellationToken cancellationToken);
}
