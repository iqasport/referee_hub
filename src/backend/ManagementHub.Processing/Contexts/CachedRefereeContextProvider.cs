using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Processing.Contexts;

/// <summary>
/// Wrapper around another context provider that caches the result. Should be registered scoped to a request.
/// </summary>
/// <remarks>
/// Only caches single RefereeView and RefereeTest contexts. Other operations are pass through.
/// </remarks>
public class CachedRefereeContextProvider : IRefereeContextProvider
{
	private readonly ConcurrentDictionary<UserIdentifier, IRefereeTestContext> refereeTestContextCache = new();
	private readonly ConcurrentDictionary<UserIdentifier, IRefereeViewContext> refereeViewContextCache = new();
	private readonly IRefereeContextProvider innerProvider;

	public CachedRefereeContextProvider(IRefereeContextProvider innerProvider)
	{
		this.innerProvider = innerProvider;
	}

	public Task<IRefereeEmailFeedbackContext> GetRefereeEmailFeedbackContextAsync(TestAttemptIdentifier testAttemptId, CancellationToken cancellationToken)
	{
		return this.innerProvider.GetRefereeEmailFeedbackContextAsync(testAttemptId, cancellationToken);
	}

	public async Task<IRefereeTestContext> GetRefereeTestContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		if (!this.refereeTestContextCache.TryGetValue(userId, out IRefereeTestContext? context))
		{
			context = await this.innerProvider.GetRefereeTestContextAsync(userId, cancellationToken);
			this.refereeTestContextCache.TryAdd(userId, context);
		}

		return context;
	}

	public async Task<IRefereeViewContext> GetRefereeViewContextAsync(UserIdentifier userId, NgbConstraint ngbConstraint, CancellationToken cancellationToken)
	{
		if (!this.refereeViewContextCache.TryGetValue(userId, out IRefereeViewContext? context))
		{
			context = await this.innerProvider.GetRefereeViewContextAsync(userId, ngbConstraint, cancellationToken);
			this.refereeViewContextCache.TryAdd(userId, context);
		}

		return context;
	}

	public IAsyncEnumerable<IRefereeViewContext> GetRefereeViewContextAsyncEnumerable(NgbConstraint ngbConstraint)
	{
		return this.innerProvider.GetRefereeViewContextAsyncEnumerable(ngbConstraint);
	}

	public IQueryable<IRefereeViewContext> GetRefereeViewContextQueryable(NgbConstraint ngbConstraint)
	{
		return this.innerProvider.GetRefereeViewContextQueryable(ngbConstraint);
	}
}
