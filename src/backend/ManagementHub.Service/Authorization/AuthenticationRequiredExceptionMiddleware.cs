using System.Net.Mime;
using ManagementHub.Models.Exceptions;

namespace ManagementHub.Service.Authorization;

public class AuthenticationRequiredExceptionMiddleware
{
	private readonly RequestDelegate next;

	public AuthenticationRequiredExceptionMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await this.next(context);
		}
		catch (AuthenticationRequiredException exception) when (!context.Response.HasStarted)
		{
			context.Response.Clear();
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			context.Response.ContentType = MediaTypeNames.Text.Plain;
			await context.Response.WriteAsync(exception.Message, context.RequestAborted);
		}
	}
}