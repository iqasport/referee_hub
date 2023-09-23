using System.Threading;
using System.Threading.Tasks;

namespace ManagementHub.Models.Abstraction.Commands.Migrations;

public interface IUserIdMigrationCommand
{
	/// <summary>
	/// Tries to migrate the user to the unique id if it hasn't been done already.
	/// </summary>
	/// <param name="email">User email.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task TryMigrateUserIdAsync(string email, CancellationToken cancellationToken);
}
