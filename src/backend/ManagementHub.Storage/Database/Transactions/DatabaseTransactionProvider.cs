using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Exceptions;
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
		// if a transaction is already in progress in the outer scope operations will be included in that one,
		// so here we will return an object that ignores commit, but propagates rollback by throwing an exception.
		if (this.dbContext.Database.CurrentTransaction != null)
		{
			return Task.FromResult<IDbContextTransaction>(new InnerTransaction(this.dbContext.Database.CurrentTransaction));
		}

		return this.dbContext.Database.BeginTransactionAsync(isolationLevel);
	}

	private class InnerTransaction : IDbContextTransaction
	{
		private readonly IDbContextTransaction outerTransaction;

		public InnerTransaction(IDbContextTransaction outerTransaction)
		{
			this.outerTransaction = outerTransaction;
		}

		public Guid TransactionId => this.outerTransaction.TransactionId;

		// no-op
		public void Commit() { }

		// no-op
		public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

		// no-op
		public void Dispose() { }

		// no-op
		public ValueTask DisposeAsync() => ValueTask.CompletedTask;

		public void Rollback()
		{
			this.outerTransaction.Rollback();
			throw this.GetException();
		}

		public async Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			await this.outerTransaction.RollbackAsync(cancellationToken);
			throw this.GetException();
		}

		private Exception GetException() => new TransactionRollbackException("Rollback was called in an inner transaction which had to disrupt the outer transaction in an uncontrolled way.");
	}
}
