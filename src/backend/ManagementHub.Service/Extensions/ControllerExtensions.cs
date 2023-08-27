using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Extensions;

public static class ControllerExtensions
{
	public static Uri GetHostBaseUri(this ControllerBase controller)
	{
		var request = controller.HttpContext.Request;
		return new Uri($"{request.Scheme}://{request.Host.Value}");
	}
}
