using System.Diagnostics;
using System.Security.Claims;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;

namespace ManagementHub.Service.Authorization;

public class ImpersonationMiddleware : IMiddleware
{
	private readonly IAuthorizationService authorizationService;
	private readonly ILogger logger;
	private readonly IUserContextAccessor userContextAccessor;

	public ImpersonationMiddleware(IAuthorizationService authorizationService, ILogger<ImpersonationMiddleware> logger, IUserContextAccessor userContextAccessor)
	{
		this.authorizationService = authorizationService;
		this.logger = logger;
		this.userContextAccessor = userContextAccessor;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		if (context.Request.Query.TryGetValue("impersonate", out var value) &&
			UserIdentifier.TryParse(value.First() ?? string.Empty, out var impersonatedUserId))
		{
			var impersonatingUser = await this.userContextAccessor.GetCurrentUserContextAsync();

			var result = await this.authorizationService.AuthorizeAsync(context.User, AuthorizationPolicies.TechAdminPolicy);
			if (!result.Succeeded)
			{
				throw new AccessDeniedException($"User does not have permission to perform impersonation.", customMessage: true);
			}

			Activity.Current?.AddTag("user.impersonatedby", impersonatingUser.UserId.ToString());
			this.logger.LogInformation(-0x6da5fd00, "Impersonating user ({userId})...", impersonatedUserId);

			try
			{
				var impersonatedUser = await this.userContextAccessor.GetUserContextAsync(impersonatedUserId);

				// Adds a secondary user identity which will be read in the UserContextAccessor as the current user
				context.User.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, impersonatedUserId.ToString()) }));

				context.Response.Headers.Add("X-Impersonated", impersonatedUser.UserId.ToString());
				context.Response.Headers.Add("X-Impersonated-By", impersonatingUser.UserId.ToString());

				this.logger.LogInformation(-0x6da5fcff, "User ({userId}) successfully impersonated.", impersonatedUserId);
			}
			catch (NotFoundException ex)
			{
				this.logger.LogError(-0x6da5fcfe, ex, "No such user ({userId}) - impersonation failed.", impersonatedUserId);
				context.Response.Headers.Add("X-Impersonated-Error", "No such user");

				// I'm writing out the response here,
				// because throwin an exception resets the HttpContext and anything we've done before with Response is lost (e.g. headers)
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				await context.Response.WriteAsync("User doesn't exist and could not be impersonated.");
				return;
			}
		}

		await next(context);
	}
}
