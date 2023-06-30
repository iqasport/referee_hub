using System;

namespace ManagementHub.Models.Exceptions;

/// <summary>
/// Singals that the current transaction had to be rolled back.
/// </summary>
public class TransactionRollbackException : Exception
{
	public TransactionRollbackException(string message) : base(message)
	{ }
}
