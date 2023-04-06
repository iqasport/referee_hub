using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Paging;

public class PagingParameters
{
	[FromQuery]
	public int Page { get; set; } = 1;

	[FromQuery]
	public int PageSize { get; set; } = 25;
}
