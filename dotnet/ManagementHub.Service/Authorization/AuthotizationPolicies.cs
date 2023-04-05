using ManagementHub.Models.Domain.User.Roles;
using Microsoft.AspNetCore.Authorization;

namespace ManagementHub.Service.Authorization;

/// <summary>
/// Constants and helpers related to creating authorization policies.
/// </summary>
public static class AuthotizationPolicies
{
	public const string RefereePolicy = nameof(RefereePolicy);

	public static void AddRefereePolicy(this AuthorizationOptions options) =>
		options.AddPolicy(RefereePolicy, policy =>
		{
			policy.AddRequirements(new UserRoleAuthorizationRequirement<RefereeRole>());
		});

	public const string RefereeViewerPolicy = nameof(RefereeViewerPolicy);

	public static void AddRefereeViewerPolicy(this AuthorizationOptions options) =>
		options.AddPolicy(RefereeViewerPolicy, policy =>
		{
			policy.AddRequirements(new UserRoleAuthorizationRequirement<RefereeViewerRole>());
		});
}
