using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ManagementHub.Storage.Database.Transactions;
internal class DatabaseTransactionProvider : IDatabaseTransactionProvider
{
	private readonly ManagementHubDbContext dbContext;

	public DatabaseTransactionProvider(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public Task<IDbContextTransaction> BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
	{
		return this.dbContext.Database.BeginTransactionAsync(isolationLevel);
	}

}
