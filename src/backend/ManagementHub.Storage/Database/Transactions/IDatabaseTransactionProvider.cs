using System.Data;
using System.Threading.Tasks;
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
}
