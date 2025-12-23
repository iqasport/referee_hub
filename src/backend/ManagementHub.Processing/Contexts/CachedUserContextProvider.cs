using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Processing.Contexts;

/// <summary>
/// Wrapper around another context provider that caches the result. Should be registered scoped to a request.
/// </summary>
public class CachedUserContextProvider : IUserContextProvider
{
	private readonly ConcurrentDictionary<UserIdentifier, IUserContext> userContextCache = new();
	private readonly ConcurrentDictionary<UserIdentifier, IUserDataContext> userDataContextCache = new();
	private readonly ConcurrentDictionary<UserIdentifier, IUserAvatarContext> userAvatarContextCache = new();
	private readonly ConcurrentDictionary<UserIdentifier, UserAttributes> userAtrributesCache = new();
	private readonly IUserContextProvider innerProvider;

	public CachedUserContextProvider(IUserContextProvider innerProvider)
	{
		this.innerProvider = innerProvider;
	}

	public async Task<UserAttributes> GetUserAttributesAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		if (!this.userAtrributesCache.TryGetValue(userId, out UserAttributes? attributes))
		{
			attributes = await this.innerProvider.GetUserAttributesAsync(userId, cancellationToken);
			this.userAtrributesCache.TryAdd(userId, attributes);
		}

		return attributes;
	}

	public async Task<IUserAvatarContext> GetUserAvatarContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		if (!this.userAvatarContextCache.TryGetValue(userId, out IUserAvatarContext? userContext))
		{
			userContext = await this.innerProvider.GetUserAvatarContextAsync(userId, cancellationToken);
			this.userAvatarContextCache.TryAdd(userId, userContext);
		}

		return userContext;
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

	public async Task<UserIdentifier?> GetUserIdByEmailAsync(Email email, CancellationToken cancellationToken = default)
	{
		// Don't cache this - it's not a frequent operation and email lookups are relatively fast
		return await this.innerProvider.GetUserIdByEmailAsync(email, cancellationToken);
	}
}
