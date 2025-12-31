using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using Microsoft.AspNetCore.Authorization;

namespace ManagementHub.Service.Authorization;

public class AuthorizationContext
{
	public RouteValueDictionary RouteParameters { get; set; } = new();
}

/// <summary>
/// Asbtract base for <see cref="UserRoleAuthorizationRequirement{TUserRole}"/>.
/// </summary>
public abstract class UserRoleAuthorizationRequirement : IAuthorizationRequirement
{
	/// <summary>
	/// Checks if the role object satisfier further requirements.
	/// </summary>
	/// <remarks>Expected to be overridden in <see cref="UserRoleAuthorizationRequirement{TUserRole}"/>.</remarks>
	public abstract bool Satisfies(IUserRole role, AuthorizationContext context);
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
	public virtual bool Satisfies(TUserRole role, AuthorizationContext context) => true;

	public sealed override bool Satisfies(IUserRole role, AuthorizationContext context) => role is TUserRole userRole && this.Satisfies(userRole, context);

	public override string ToString() => $"{typeof(TUserRole).Name} authorization requirement";
}

// For any endpoint with a route parameter "ngb" that is a NgbIdentifier, the user must have a role that applies to that NGB.
public class NgbUserRoleAuthorizationRequirement<TUserRole> : UserRoleAuthorizationRequirement<TUserRole>
	where TUserRole : INgbUserRole
{
	override public bool Satisfies(TUserRole role, AuthorizationContext context) =>
		context.RouteParameters.TryGetValue("ngb", out var ngbIdObject) && ngbIdObject is string ngbId &&
		role.Ngb.AppliesTo(NgbIdentifier.Parse(ngbId));
}

// For any endpoint with a route parameter "tournamentId" that is a TournamentIdentifier, the user must have a role that applies to that tournament.
public class TournamentUserRoleAuthorizationRequirement<TUserRole> : UserRoleAuthorizationRequirement<TUserRole>
	where TUserRole : ITournamentUserRole
{
	override public bool Satisfies(TUserRole role, AuthorizationContext context) =>
		context.RouteParameters.TryGetValue("tournamentId", out var tournamentIdObject) &&
		tournamentIdObject is string tournamentId &&
		role.Tournament.AppliesTo(TournamentIdentifier.Parse(tournamentId));
}

// For any endpoint with a route parameter "teamId" that is a TeamIdentifier, the user must have a role that applies to that team.
public class TeamUserRoleAuthorizationRequirement<TUserRole> : UserRoleAuthorizationRequirement<TUserRole>
	where TUserRole : ITeamUserRole
{
	override public bool Satisfies(TUserRole role, AuthorizationContext context) =>
		context.RouteParameters.TryGetValue("teamId", out var teamIdObject) &&
		teamIdObject is string teamId &&
		role.Team.AppliesTo(TeamIdentifier.Parse(teamId));
}

// Compound requirement that succeeds if ANY of the inner requirements are satisfied (OR logic)
public class CompoundAuthorizationRequirement : UserRoleAuthorizationRequirement
{
	private readonly UserRoleAuthorizationRequirement[] requirements;

	public CompoundAuthorizationRequirement(params UserRoleAuthorizationRequirement[] requirements)
	{
		this.requirements = requirements;
	}

	public override bool Satisfies(IUserRole role, AuthorizationContext context)
	{
		foreach (var requirement in this.requirements)
		{
			if (requirement.Satisfies(role, context))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString() => $"Compound authorization requirement ({string.Join(" OR ", this.requirements.Select(r => r.ToString()))})";
}
