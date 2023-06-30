using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ManagementHub.Storage.Commands;

/// <summary>
/// Helper for Entity Framework update call.
/// </summary>
public static class EfUpdateHelper
{
	/// <summary>
	/// Invoke <see cref="RelationalQueryableExtensions.ExecuteUpdateAsync{TSource}(IQueryable{TSource}, Expression{Func{SetPropertyCalls{TSource}, SetPropertyCalls{TSource}}}, CancellationToken)"/>
	/// by combining together a collection of individual property setters.
	/// </summary>
	/// <typeparam name="T">Type of queryable collection.</typeparam>
	/// <param name="query">Queryable collection to perform update over.</param>
	/// <param name="setPropertyCalls">Collection of property setters to be invoked over <paramref name="query"/>.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Number of updated rows in the database.</returns>
	public static Task<int> ExecuteUpdateAsync<T>(this IQueryable<T> query, IEnumerable<Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>> setPropertyCalls, CancellationToken cancellationToken)
	{
		Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> combined = Combine(setPropertyCalls);
		return query.ExecuteUpdateAsync(combined, cancellationToken);
	}

	private static Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> Combine<T>(IEnumerable<Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>> setPropertyCalls)
	{
		return setPropertyCalls.Aggregate(Combine);
	}

	private static Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> Combine<T>(
		Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls1,
		Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls2)
	{
		var exprInner = (MethodCallExpression)setPropertyCalls1.Body;
		var exprOuter = (MethodCallExpression)setPropertyCalls2.Body;

		var call = Expression.Call(
				exprInner,
				exprOuter.Method,
				exprOuter.Arguments);

		return Expression.Lambda<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>(call, setPropertyCalls1.Parameters);
	}
}
