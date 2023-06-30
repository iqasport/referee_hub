using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Storage.Identity;

// Other method required by the interfaces of Identity implemented by UserStore
public partial class UserStore
{
	public Task<UserIdentity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
		this.FindByEmailAsync(normalizedUserName, cancellationToken);

	public Task<string?> GetEmailAsync(UserIdentity user, CancellationToken cancellationToken) =>
		this.GetNormalizedEmailAsync(user, cancellationToken);

	public Task<string?> GetNormalizedUserNameAsync(UserIdentity user, CancellationToken cancellationToken) =>
		this.GetNormalizedEmailAsync(user, cancellationToken);

	public Task<string?> GetUserNameAsync(UserIdentity user, CancellationToken cancellationToken) =>
		this.GetNormalizedEmailAsync(user, cancellationToken);

	public Task SetEmailAsync(UserIdentity user, string? email, CancellationToken cancellationToken) =>
		this.SetNormalizedEmailAsync(user, email, cancellationToken);

	public Task SetNormalizedUserNameAsync(UserIdentity user, string? normalizedName, CancellationToken cancellationToken) =>
		this.SetEmailAsync(user, normalizedName, cancellationToken);

	public Task SetUserNameAsync(UserIdentity user, string? userName, CancellationToken cancellationToken) =>
		this.SetEmailAsync(user, userName, cancellationToken);
}
