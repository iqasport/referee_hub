using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Paging;

public class PagingParameters
{
	public int Page { get; set; } = 1;

	public int PageSize { get; set; } = 25;
}
