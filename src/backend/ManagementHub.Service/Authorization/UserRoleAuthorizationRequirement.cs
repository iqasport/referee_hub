using ManagementHub.Models.Abstraction;
using Microsoft.AspNetCore.Authorization;

namespace ManagementHub.Service.Authorization;

/// <summary>
/// Asbtract base for <see cref="UserRoleAuthorizationRequirement{TUserRole}"/>.
/// </summary>
public abstract class UserRoleAuthorizationRequirement : IAuthorizationRequirement
{
	/// <summary>
	/// Checks if the role object satisfier further requirements. This method takes an object of the <see cref="UserRoleType"/>.
	/// </summary>
	/// <remarks>Expected to be overridden in <see cref="UserRoleAuthorizationRequirement{TUserRole}"/>.</remarks>
	public abstract bool Satisfies(IUserRole role);
}

/// <summary>
/// Authorization requirement that a user has a certain role.
/// </summary>
/// <typeparam name="TUserRole">Type of the user role to check against.</typeparam>
public class UserRoleAuthorizationRequirement<TUserRole> : UserRoleAuthorizationRequirement
	where TUserRole : IUserRole
{
	/// <summary>
	/// Checks if the role object satisfier further requirements. If not overriden, returns true because the user has the role.
	/// </summary>
	/// <param name="role">Role object to check details of.</param>
	/// <returns><c>true</c> if <paramref name="role"/> satisfies the authorization requirement, <c>false</c> otherwise.</returns>
	public virtual bool Satisfies(TUserRole role) => true;

	public sealed override bool Satisfies(IUserRole role) => role is TUserRole userRole && this.Satisfies(userRole);

	public override string ToString() => $"{typeof(TUserRole).Name} authorization requirement";
}
