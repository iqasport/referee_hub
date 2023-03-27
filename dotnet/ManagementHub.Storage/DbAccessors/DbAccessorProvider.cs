using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagementHub.Storage.DbAccessors;

/// <summary>
/// Wrapper over service provider to get db accessors using a generic method at method call time.
/// </summary>
public class DbAccessorProvider : IDbAccessorProvider
{
	private readonly IServiceProvider serviceProvider;

	public DbAccessorProvider(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public IDbAccessor<TId> GetDbAccessor<TId>()
	{
		return this.serviceProvider.GetRequiredService<IDbAccessor<TId>>();
	}
}
