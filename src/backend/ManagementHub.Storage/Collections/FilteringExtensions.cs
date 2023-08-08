using System.Linq;

namespace ManagementHub.Storage.Collections;

public static class FilteringExtensions
{
	public static IQueryable<T> Page<T>(this IQueryable<T> query, FilteringParameters parameters)
	{
		var take = parameters.PageSize;
		var skip = (parameters.Page - 1) * take;
		return query.Skip(skip).Take(take);
	}
}
