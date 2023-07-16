using ManagementHub.Models.Abstraction.Contexts;

namespace ManagementHub.Service.Filtering;

public static class PagingExtensions
{
	public static IQueryable<T> Page<T>(this IQueryable<T> query, PagingParameters parameters)
	{
		var take = parameters.PageSize;
		var skip = (parameters.Page - 1) * take;
		return query.Skip(skip).Take(take);
	}

	public static IQueryable<IRefereeViewContext> ApplyFilter(this IQueryable<IRefereeViewContext> query, FilteringParameters filteringParameters)
		=> filteringParameters.Filter is null ? query : query.Where(ctx => ctx.DisplayName.Contains(filteringParameters.Filter, StringComparison.InvariantCultureIgnoreCase));

	public static IQueryable<ITeamContext> ApplyFilter(this IQueryable<ITeamContext> query, FilteringParameters filteringParameters)
		=> filteringParameters.Filter is null ? query : query.Where(ctx => ctx.TeamData.Name.Contains(filteringParameters.Filter, StringComparison.InvariantCultureIgnoreCase));
}
