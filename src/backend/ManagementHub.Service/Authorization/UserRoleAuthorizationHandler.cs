﻿using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;

namespace ManagementHub.Service.Authorization;

/// <summary>
/// Authorization handler that checks current user's roles against the specified <see cref="UserRoleAuthorizationRequirement{TUserRole}"/>.
/// </summary>
public class UserRoleAuthorizationHandler : AuthorizationHandler<UserRoleAuthorizationRequirement>
{
	private readonly IUserContextAccessor currentContextAccessor;
	private readonly ILogger<UserRoleAuthorizationHandler> logger;
	private readonly IHttpContextAccessor httpContextAccessor;

	public UserRoleAuthorizationHandler(IUserContextAccessor currentContextAccessor, ILogger<UserRoleAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor)
	{
		this.currentContextAccessor = currentContextAccessor;
		this.logger = logger;
		this.httpContextAccessor = httpContextAccessor;
	}

	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleAuthorizationRequirement requirement)
	{
		IUserContext userContext;
		try
		{
			userContext = await this.currentContextAccessor.GetCurrentUserContextAsync();
		}
		catch (NotFoundException ex)
		{
			this.logger.LogWarning(0x25719402, ex, "User context was not found, which means we need to invalidate the session.");
			context.Fail(new AuthorizationFailureReason(this, "Unable to load user context. Sign in again."));
			return;
		}

		var authContext = new AuthorizationContext
		{
			RouteParameters = this.httpContextAccessor.HttpContext!.GetRouteData().Values,
		};

		foreach (var userRole in userContext.Roles)
		{
			if (requirement.Satisfies(userRole, authContext))
			{
				this.logger.LogInformation(0x25719400, "{requirement} has been satisfied.", requirement);
				context.Succeed(requirement);
				return;
			}
		}

		this.logger.LogWarning(0x25719401, "{requirement} failed to be satisfied.", requirement);
	}
}
