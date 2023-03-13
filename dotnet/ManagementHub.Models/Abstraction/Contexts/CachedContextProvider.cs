using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts;

/// <summary>
/// Wrapper around another context provider that caches the result. Should be registered scoped to a request.
/// </summary>
public class CachedContextProvider : IContextProvider
{
	private readonly ConcurrentDictionary<UserIdentifier, IUserContext> userContextCache = new();
	private readonly ConcurrentDictionary<UserIdentifier, IUserDataContext> userDataContextCache = new();
	private readonly IContextProvider innerProvider;

	public CachedContextProvider(IContextProvider innerProvider)
	{
		this.innerProvider = innerProvider;
	}

	public async Task<IUserContext> GetUserContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		if (!this.userContextCache.TryGetValue(userId, out IUserContext? userContext))
		{
			userContext = await this.innerProvider.GetUserContextAsync(userId, cancellationToken);
			this.userContextCache.TryAdd(userId, userContext);
		}

		return userContext;
	}

	public async Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		if (!this.userDataContextCache.TryGetValue(userId, out IUserDataContext? userDataContext))
		{
			userDataContext = await this.innerProvider.GetUserDataContextAsync(userId, cancellationToken);
			this.userDataContextCache.TryAdd(userId, userDataContext);
		}

		return userDataContext;
	}
}
