using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ManagementHub.Storage.Database.Transactions;

public interface IDatabaseTransactionProvider
{
	Task<IDbContextTransaction> BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}
