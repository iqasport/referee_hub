using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace ManagementHub.Storage.Database.Transactions;

public interface IDatabaseTransactionProvider
{
	/// <summary>
	/// Starts a new transaction if one is not already running and returns an object for managing the current transaction context.
	/// </summary>
	/// <param name="isolationLevel">Isolation level of the transaction which describes how data of other workers will be viewable in the current context.</param>
	/// <returns>A disposable transaction object.</returns>
	Task<IDbContextTransaction> BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

	/// <summary>
	/// Returns the execution strategy for the current database. This is required when using explicit transactions
	/// alongside a resilient retry execution strategy (e.g. <see cref="Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy"/>
	/// configured via EnableRetryOnFailure), because EF Core does not allow starting user-initiated transactions without
	/// wrapping them inside an execution strategy callback.
	/// </summary>
	IExecutionStrategy GetExecutionStrategy();
}
