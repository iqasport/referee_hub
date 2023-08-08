using ManagementHub.Storage.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManagementHub.Service.Filtering;

public class CollectionFilteringActionFilter : IAsyncActionFilter
{
	private readonly CollectionFilteringContext collectionFilteringContext;
	private readonly ILogger logger;

	public CollectionFilteringActionFilter(CollectionFilteringContext collectionFilteringContext, ILogger<CollectionFilteringActionFilter> logger)
	{
		this.collectionFilteringContext = collectionFilteringContext;
		this.logger = logger;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		bool appliesFilter = false;
		if (context.ActionArguments.TryGetValue("filtering", out object? filteringObject) && filteringObject is FilteringParameters filtering)
		{
			this.collectionFilteringContext.FilteringParameters = filtering;
			this.collectionFilteringContext.FilteringMetadata = new();
			this.logger.LogInformation(0, "Applying filtering: Filter='{filter}', Page={page}, PageSize={pageSize}", filtering.Filter, filtering.Page, filtering.PageSize);
			appliesFilter = true;
		}

		var actionResult = await next();

		if (appliesFilter && actionResult.Result is ObjectResult result && result.Value is Filtered filtered)
		{
			filtered.Metadata = this.collectionFilteringContext.FilteringMetadata;
			this.logger.LogInformation(0, "Adding filtered metadata: TotalCount='{count}'", filtered.Metadata?.TotalCount);
		}
	}
}
