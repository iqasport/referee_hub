---
name: authorization-policy
description: Rules for creating and using authorization policies and role requirements. Use when adding API endpoints, defining roles, or tightening access control.
---

# Authorization Policies

**Trigger**: Any new API endpoint, role definition, or access control change.

**Use when**: Adding authorization to controllers, defining new user roles, or creating compound policies.

---

## The Pattern

Authorization uses **policies** (named strings) + **requirements** (classes) + **handlers** (logic). The system is role-based with scoped evaluation for NGB/tournament/team boundaries.

### Policy constants

All policies and their registration helpers live in **`ManagementHub.Service/Authorization/AuthorizationPolicies.cs`** — read it for the full list of constants and extension methods.

### Role hierarchy

Roles are defined in `ManagementHub.Models/Domain/User/Roles/` and implement scoped interfaces (`IUserRole`, `INgbUserRole`, `ITournamentUserRole`, `ITeamUserRole`). Read the role definitions for the full hierarchy.

### Requirement classes

Located in `ManagementHub.Service/Authorization/UserRoleAuthorizationRequirement.cs`:

| Requirement | Scope Check |
|-------------|-------------|
| `UserRoleAuthorizationRequirement<T>` | No scope check (system-wide) |
| `NgbUserRoleAuthorizationRequirement<T>` | Checks `ngb` route parameter |
| `TournamentUserRoleAuthorizationRequirement<T>` | Checks `tournamentId` route parameter |
| `TeamUserRoleAuthorizationRequirement<T>` | Checks `teamId` route parameter |
| `CompoundOrAuthorizationRequirement` | OR logic across requirements | |

---

## Rules

### ALWAYS:

1. **Use `[Authorize(Policy = AuthorizationPolicies.<Name>)]`** — Never use `[Authorize(Roles = "...")]`
2. **Define policy constants in `AuthorizationPolicies`** — Not inline strings
3. **Register policies with extension methods** — `Add<Name>Policy(this AuthorizationOptions)`
4. **Use scoped requirements when the endpoint has a route scope** — `NgbUserRoleAuthorizationRequirement` for `ngb` route param, `TournamentUserRoleAuthorizationRequirement` for `tournamentId`, etc.
5. **Use `CompoundOrAuthorizationRequirement` for OR logic** — Not multiple `[Authorize]` attributes
6. **Prefer narrower policies** — `TournamentManagerPolicy` over `IqaAdminPolicy` when possible
7. **Add policy registration in startup** — Usually in `Program.cs` or `Startup.cs`

### NEVER:

1. **Bypass authorization** — Never add `[AllowAnonymous]` without justification
2. **Use string policy names inline** — Always reference `AuthorizationPolicies.<Name>`
3. **Compare roles directly** — Use requirements, not `role.Type == "admin"`
4. **Skip authorization on write endpoints** — Every POST/PUT/PATCH/DELETE needs a policy
5. **Use system-wide requirements for scoped data** — If the endpoint has `ngb` in the route, use `NgbUserRoleAuthorizationRequirement`

### Controller usage:

Read example controllers in `ManagementHub.Service/Areas/` for patterns:
- `NgbsController.cs` — NGB-scoped authorization
- `TournamentsController.cs` — Tournament-scoped authorization
- `TeamsController.cs` — Compound policies

### Adding a new policy:

1. **Define the role** in `ManagementHub.Models/Domain/User/Roles/`
2. **Add the requirement** (or reuse existing) — see `UserRoleAuthorizationRequirement.cs`
3. **Add the policy constant** in `AuthorizationPolicies.cs`
4. **Add the registration helper** in `AuthorizationPolicies.cs`
5. **Register in startup** — Call the extension method in `Program.cs`
6. **Apply to controller/action** — `[Authorize(AuthorizationPolicies.<NewPolicy>)]`

### Compound policies (OR):

Use `CompoundOrAuthorizationRequirement` for OR logic — see `AuthorizationPolicies.cs` for examples like `TeamManagerOrNgbAdminPolicy` and `TeamManagerOrAnyNgbAdminPolicy`.

---

## Why This Matters

- **Explicit access control** — Policies document who can access what
- **Scoped evaluation** — Requirements check route parameters for NGB/tournament/team scope
- **Composability** — `CompoundOrAuthorizationRequirement` enables flexible policies
- **Type safety** — Role types are enforced at compile time
- **Auditability** — All authorization flows through a single system
