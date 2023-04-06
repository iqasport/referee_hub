namespace ManagementHub.Service.Paging;

public static class PagingExtensions
{
	public static IQueryable<T> Page<T>(this IQueryable<T> query, PagingParameters parameters)
	{
		var take = parameters.PageSize;
		var skip = (parameters.Page - 1) * take;
		return query.Skip(skip).Take(take);
	}
}
