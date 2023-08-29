using System.Diagnostics;
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

	public UserRoleAuthorizationHandler(IUserContextAccessor currentContextAccessor, ILogger<UserRoleAuthorizationHandler> logger)
	{
		this.currentContextAccessor = currentContextAccessor;
		this.logger = logger;
	}

	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleAuthorizationRequirement requirement)
	{
		var userContext = await this.currentContextAccessor.GetCurrentUserContextAsync();

		foreach (var userRole in userContext.Roles)
		{
			if (requirement.Satisfies(userRole))
			{
				this.logger.LogInformation(0x25719400, "{requirement} has been satisfied.", requirement);
				context.Succeed(requirement);
				return;
			}
		}

		this.logger.LogWarning(0x25719401, "{requirement} failed to be satisfied.", requirement);
	}
}
