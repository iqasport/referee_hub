using System.Linq;

namespace ManagementHub.Storage.Collections;

public static class FilteringExtensions
{
	public static IQueryable<T> Page<T>(this IQueryable<T> query, FilteringParameters parameters)
	{
		// if skip paging is null it means it wasn't set through the request middleware - assume we don't want to page
		if (parameters.SkipPaging ?? true)
		{
			return query;
		}

		var take = parameters.PageSize;
		var skip = (parameters.Page - 1) * take;
		return query.Skip(skip).Take(take);
	}
}
