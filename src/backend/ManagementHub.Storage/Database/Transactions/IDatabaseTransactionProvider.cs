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
	/// Returns the execution strategy configured for the database connection.
	/// When retry-on-failure is enabled (e.g. <see cref="NpgsqlDbContextOptionsBuilderExtensions.EnableRetryOnFailure"/>),
	/// EF Core requires that user-initiated transactions are wrapped using the execution strategy
	/// so that transient failures can be retried correctly. Use this method to obtain the strategy
	/// and wrap any operation that opens an explicit transaction with <c>strategy.ExecuteAsync(...)</c>.
	/// </summary>
	/// <returns>The <see cref="IExecutionStrategy"/> for the underlying database connection.</returns>
	IExecutionStrategy GetExecutionStrategy();
}
