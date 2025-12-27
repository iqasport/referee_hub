# Phase 5: Team Manager Bootstrap (NGB Admin Access)

## Overview
This phase enables NGB Admins to manage team managers for teams under their jurisdiction:
- Adding team managers to teams via email
- Removing team managers from teams
- Listing team managers for a team
- Ensuring proper NGB jurisdiction validation
- Following the same pattern as NGB admin management

This addresses the bootstrap problem where teams need an initial manager before they can participate in tournaments.

## Prerequisites
- Phase 3 must be complete:
  - TeamManager entity exists
  - TeamManagerRole and authorization working
  - Team-based authorization functional

## Database Changes

### Update TeamManager Entity (if needed)
**Location:** `src/backend/ManagementHub.Storage/Data/TeamManager.cs`

Verify the entity has all necessary properties. If AddedByUserId doesn't exist, add it:

```csharp
public class TeamManager
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public long UserId { get; set; }
    public long AddedByUserId { get; set; }  // Tracks which user (NGB admin or team manager) added this manager
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User AddedBy { get; set; } = null!;
}
```

**Note:** AddedByUserId tracks who added the manager:
- NGB Admin adding manager: Set to the NGB admin's user ID
- Team Manager adding additional managers: Set to the adding manager's user ID

### Create EF Migration (if changes made)
```powershell
dotnet ef migrations add AddTeamManagerAddedBy --project ../ManagementHub.Storage
```

## API Implementation

### 1. Create Team Manager Command Interface
**Location:** `src/backend/ManagementHub.Models/Abstraction/Commands/IUpdateTeamManagerRoleCommand.cs`

**Pattern reference:** `IUpdateNgbAdminRoleCommand.cs`

```csharp
using System.Threading.Tasks;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUpdateTeamManagerRoleCommand
{
    public enum AddRoleResult
    {
        UserDoesNotExist,
        RoleAdded,
        UserCreatedWithRole,
    }
    
    Task<AddRoleResult> AddTeamManagerRoleAsync(
        TeamIdentifier teamId, 
        Email email, 
        bool createUserIfNotExists,
        UserIdentifier addedByUserId);
    
    Task<bool> DeleteTeamManagerRoleAsync(TeamIdentifier teamId, Email email);
}
```

### 2. Implement Team Manager Command
**Location:** `src/backend/ManagementHub.Storage/Commands/Team/UpdateTeamManagerRoleCommand.cs`

**Pattern reference:** `UpdateNgbAdminRoleCommand.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Team;

public class UpdateTeamManagerRoleCommand : IUpdateTeamManagerRoleCommand
{
    private readonly ManagementHubDbContext dbContext;
    private readonly ILogger<UpdateTeamManagerRoleCommand> logger;
    private readonly IDatabaseTransactionProvider databaseTransactionProvider;
    
    public UpdateTeamManagerRoleCommand(
        ManagementHubDbContext dbContext,
        ILogger<UpdateTeamManagerRoleCommand> logger,
        IDatabaseTransactionProvider databaseTransactionProvider)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.databaseTransactionProvider = databaseTransactionProvider;
    }
    
    public async Task<IUpdateTeamManagerRoleCommand.AddRoleResult> AddTeamManagerRoleAsync(
        TeamIdentifier teamId,
        Email email,
        bool createUserIfNotExists,
        UserIdentifier addedByUserId)
    {
        using var transaction = await this.databaseTransactionProvider.BeginAsync();
        bool userCreated = false;
        
        // Get or create user
        var user = await this.dbContext.Users.AsNoTracking()
            .WithEmail(email)
            .FirstOrDefaultAsync();
        
        if (user == null)
        {
            if (!createUserIfNotExists)
            {
                this.logger.LogInformation("User with email {Email} not found", email);
                return IUpdateTeamManagerRoleCommand.AddRoleResult.UserDoesNotExist;
            }
            
            this.logger.LogInformation("Creating user with email {Email}", email);
            user = new Models.Data.User
            {
                Email = email.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Other required fields...
            };
            this.dbContext.Users.Add(user);
            await this.dbContext.SaveChangesAsync();
            userCreated = true;
            
            this.logger.LogInformation("User created with ID {UserId}", user.Id);
        }
        
        // Get team database ID
        var teamDbId = await this.dbContext.Teams
            .Where(t => t.TeamId == teamId.ToString())
            .Select(t => t.Id)
            .FirstOrDefaultAsync();
        
        if (teamDbId == 0)
        {
            this.logger.LogWarning("Team {TeamId} not found", teamId);
            throw new InvalidOperationException($"Team {teamId} not found");
        }
        
        // Check if already a manager
        var existingManager = await this.dbContext.TeamManagers
            .Where(tm => tm.TeamId == teamDbId && tm.UserId == user.Id)
            .FirstOrDefaultAsync();
        
        if (existingManager != null)
        {
            this.logger.LogInformation("User {UserId} already is a manager of team {TeamId}", 
                user.Id, teamId);
            return IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded;
        }
        
        // Get current user (NGB admin) ID
        var currentUser = await this.dbContext.Users.AsNoTracking()
            .WithIdentifier(addedByUserId)
            .FirstOrDefaultAsync();
        
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current user not found");
        }
        
        // Add team manager
        this.logger.LogInformation("Adding team manager for user {UserId} to team {TeamId}", 
            user.Id, teamId);
        
        this.dbContext.TeamManagers.Add(new Models.Data.TeamManager
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TeamId = teamDbId,
            UserId = user.Id,
            AddedByUserId = currentUser.Id,  // NGB admin who added this manager
        });
        
        await this.dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return userCreated
            ? IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole
            : IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded;
    }
    
    public async Task<bool> DeleteTeamManagerRoleAsync(TeamIdentifier teamId, Email email)
    {
        using var transaction = await this.databaseTransactionProvider.BeginAsync();
        
        var user = await this.dbContext.Users.AsNoTracking()
            .WithEmail(email)
            .FirstOrDefaultAsync();
        
        if (user == null)
        {
            this.logger.LogInformation("User with email {Email} not found", email);
            return false;
        }
        
        // Get team database ID
        var teamDbId = await this.dbContext.Teams
            .Where(t => t.TeamId == teamId.ToString())
            .Select(t => t.Id)
            .FirstOrDefaultAsync();
        
        if (teamDbId == 0)
        {
            this.logger.LogWarning("Team {TeamId} not found", teamId);
            return false;
        }
        
        // Find and remove team manager assignment
        var manager = await this.dbContext.TeamManagers
            .Where(tm => tm.TeamId == teamDbId && tm.UserId == user.Id)
            .FirstOrDefaultAsync();
        
        if (manager == null)
        {
            this.logger.LogInformation(
                "User {UserId} is not a manager of team {TeamId}", 
                user.Id, teamId);
            return false;
        }
        
        this.logger.LogInformation(
            "Removing team manager assignment for user {UserId} from team {TeamId}", 
            user.Id, teamId);
        
        this.dbContext.TeamManagers.Remove(manager);
        await this.dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return true;
    }
}
```

### 3. Register Command in DI
**Location:** `src/backend/ManagementHub.Service/Program.cs` or DI configuration

```csharp
services.AddScoped<IUpdateTeamManagerRoleCommand, UpdateTeamManagerRoleCommand>();
```

### 4. Create View Models
**Location:** `src/backend/ManagementHub.Service/Areas/Teams/`

**TeamManagerCreationModel.cs:**
```csharp
public class TeamManagerCreationModel
{
    public string Email { get; set; } = string.Empty;
    public bool CreateAccountIfNotExists { get; set; }
}
```

**TeamManagerCreationStatus.cs:**
```csharp
public enum TeamManagerCreationStatus
{
    UserDoesNotExist = 0,
    ManagerRoleAdded = 1,
    ManagerUserCreated = 2,
}
```

**TeamManagerViewModel.cs:**
```csharp
public class TeamManagerViewModel
{
    public string Id { get; set; }  // UserIdentifier as string
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 5. Create TeamManagerOrNgbAdminPolicy
**Location:** `src/backend/ManagementHub.Service/Authorization/AuthorizationPolicies.cs`

**Pattern reference:** See how other policies are defined in the same file.

Add policy constant and registration method:
```csharp
public const string TeamManagerOrNgbAdminPolicy = nameof(TeamManagerOrNgbAdminPolicy);

public static void AddTeamManagerOrNgbAdminPolicy(this AuthorizationOptions options) =>
    options.AddPolicy(TeamManagerOrNgbAdminPolicy, policy =>
    {
        policy.AddRequirements(new CompoundAuthorizationRequirement(
            new TeamUserRoleAuthorizationRequirement<TeamManagerRole>(),
            new NgbUserRoleAuthorizationRequirement<NgbAdminRole>()));
    });
```

**Note:** This policy allows users with EITHER TeamManagerRole OR NgbAdminRole to access the endpoint. The CompoundAuthorizationRequirement evaluates as OR (not AND).

**Register in Program.cs:**
Find where authorization policies are configured and add:
```csharp
options.AddTeamManagerOrNgbAdminPolicy();
```

### 6. Add Endpoints to Teams Controller or Create New Controller
**Location:** `src/backend/ManagementHub.Service/Areas/Ngbs/NgbsController.cs` or separate Teams controller

Add to existing NGB controller (since teams are subresourced under NGBs) or create separate controller following NGB pattern.

**a) POST `/api/v2/Ngbs/{ngb}/teams/{teamId}/managers`** - Add team manager (NGB Admin only)

```csharp
[HttpPost("{ngb}/teams/{teamId}/managers")]
[Tags("Team")]
[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamManagerCreationStatus))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
public async Task<TeamManagerCreationStatus> AddTeamManager(
    [FromRoute] NgbIdentifier ngb,
    [FromRoute] TeamIdentifier teamId,
    [FromBody] TeamManagerCreationModel managerModel)
{
    var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Verify NGB admin has jurisdiction over this team
    var team = await this.teamContextProvider.GetTeamAsync(teamId);
    if (team == null)
    {
        this.Response.StatusCode = StatusCodes.Status404NotFound;
        return TeamManagerCreationStatus.UserDoesNotExist;  // Will be overridden by 404
    }
    
    var permissionConstraint = userContext.Roles
        .OfType<NgbAdminRole>()
        .FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();
    
    if (!permissionConstraint.AppliesTo(team.NgbId))
    {
        throw new AccessDeniedException($"No permission for team {teamId}");
    }
    
    // Parse and validate email
    if (!Email.TryParse(managerModel.Email, out var email))
    {
        this.Response.StatusCode = StatusCodes.Status400BadRequest;
        return TeamManagerCreationStatus.UserDoesNotExist;  // Will be overridden by 400
    }
    
    // Add manager
    var result = await this.updateTeamManagerRoleCommand.AddTeamManagerRoleAsync(
        teamId, email, managerModel.CreateAccountIfNotExists, userContext.UserId);
    
    return result switch
    {
        IUpdateTeamManagerRoleCommand.AddRoleResult.UserDoesNotExist =>
            TeamManagerCreationStatus.UserDoesNotExist,
        IUpdateTeamManagerRoleCommand.AddRoleResult.RoleAdded =>
            TeamManagerCreationStatus.ManagerRoleAdded,
        IUpdateTeamManagerRoleCommand.AddRoleResult.UserCreatedWithRole =>
            TeamManagerCreationStatus.ManagerUserCreated,
        _ => throw new InvalidOperationException($"Unexpected result {result}")
    };
}
```

**b) DELETE `/api/v2/Ngbs/{ngb}/teams/{teamId}/managers`** - Remove team manager (NGB Admin only)

```csharp
[HttpDelete("{ngb}/teams/{teamId}/managers")]
[Tags("Team")]
[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task DeleteTeamManager(
    [FromRoute] NgbIdentifier ngb,
    [FromRoute] TeamIdentifier teamId,
    [FromQuery] string email)
{
    var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Verify NGB admin has jurisdiction over this team
    var team = await this.teamContextProvider.GetTeamAsync(teamId);
    if (team == null)
    {
        this.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    
    var permissionConstraint = userContext.Roles
        .OfType<NgbAdminRole>()
        .FirstOrDefault()?.Ngb ?? NgbConstraint.Empty();
    
    if (!permissionConstraint.AppliesTo(team.NgbId))
    {
        throw new AccessDeniedException($"No permission for team {teamId}");
    }
    
    // Parse and validate email
    if (!Email.TryParse(email, out var email_))
    {
        this.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    
    // Remove manager
    var result = await this.updateTeamManagerRoleCommand.DeleteTeamManagerRoleAsync(
        teamId, email_);
    
    this.Response.StatusCode = result 
        ? StatusCodes.Status200OK 
        : StatusCodes.Status404NotFound;
}
```

**c) GET `/api/v2/Ngbs/{ngb}/teams/{teamId}/managers`** - List team managers (NGB Admin or Team Manager)

```csharp
[HttpGet("{ngb}/teams/{teamId}/managers")]
[Tags("Team")]
[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
public async Task<IEnumerable<TeamManagerViewModel>> GetTeamManagers(
    [FromRoute] NgbIdentifier ngb,
    [FromRoute] TeamIdentifier teamId)
{
    var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Verify team belongs to NGB
    var team = await this.teamContextProvider.GetTeamAsync(teamId);
    if (team == null)
    {
        return Enumerable.Empty<TeamManagerViewModel>();
    }
    
    if (!team.NgbId.Equals(ngb))
    {
        return Enumerable.Empty<TeamManagerViewModel>();
    }
    
    // Get managers
    var managers = await this.teamContextProvider.GetTeamManagersAsync(teamId);
    
    return managers.Select(m => new TeamManagerViewModel
    {
        Id = m.UserId.ToString(),
        Name = m.Name,
        Email = m.Email
    });
}
```

### 6. Extend Team Context Provider
**Location:** Team context provider implementation

Add method to interface:
```csharp
Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId);
```

**ManagerInfo** is the same class used in Phase 2 for tournament managers.

**Implementation:**
```csharp
public async Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId)
{
    var teamDbId = await context.Teams
        .Where(t => t.TeamId == teamId.ToString())
        .Select(t => t.Id)
        .FirstOrDefaultAsync();
    
    if (teamDbId == 0)
    {
        return Enumerable.Empty<ManagerInfo>();
    }
    
    var managers = await context.TeamManagers
        .Where(tm => tm.TeamId == teamDbId)
        .Join(
            context.Users,
            tm => tm.UserId,
            u => u.Id,
            (tm, u) => new ManagerInfo
            {
                UserId = u.UniqueId != null 
                    ? UserIdentifier.Parse(u.UniqueId) 
                    : UserIdentifier.FromLegacyUserId(u.Id),
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email
            })
        .ToListAsync();
    
    return managers;
}
```

## Design Decisions

1. **NGB Jurisdiction Enforcement:** NGB Admins can only add/remove managers for teams under their NGB. This is validated on each request.

2. **No Last Manager Protection:** Unlike tournament managers, team managers can all be removed. This allows NGB admins to completely reset team management if needed.

3. **AddedByUserId Audit Trail:** AddedByUserId is always set to track who added the manager. When NGB admin adds manager, it's set to the NGB admin's user ID. When team managers add other managers (Phase 3 functionality), it's set to the adding manager's ID.

4. **Email-Based Management:** Following NGB admin pattern, managers are added/removed by email address rather than UserIdentifier.

5. **User Creation:** NGB Admins can optionally create user accounts when adding managers if user doesn't exist (via CreateAccountIfNotExists flag).

6. **Authorization Separation:** 
   - NGB Admins use `NgbAdminPolicy` to add/remove managers
   - Both NGB Admins and Team Managers can list managers
   - Team Managers manage other managers via Phase 3 endpoints (if implemented)

7. **No Manager Limits:** No maximum number of managers per team.

8. **Manager Equality:** All team managers have equal permissions. No owner/admin distinction.

## Testing Checklist

After implementing:

1. **Add Team Manager (NGB Admin):**
   - NGB Admin can add manager to team in their jurisdiction
   - Cannot add manager to team in different NGB
   - Can add by email
   - Handles invalid email format
   - Handles user not found (with and without CreateAccountIfNotExists)
   - Idempotent (adding twice doesn't error)
   - New manager can immediately access team manager endpoints

2. **Remove Team Manager (NGB Admin):**
   - NGB Admin can remove manager from team in their jurisdiction
   - Cannot remove manager from team in different NGB
   - Can remove by email
   - Handles user not found
   - Handles user not being a manager
   - Removed user loses TeamManager role for that team

3. **List Team Managers:**
   - NGB Admin can list managers for teams in their jurisdiction
   - Team Manager can list managers for their own team
   - Other users cannot list managers
   - Returns correct manager details

4. **Authorization:**
   - Non-NGB-admins cannot add/remove managers
   - NGB Admin from different NGB cannot manage another NGB's teams
   - Team managers can view but not modify via these endpoints

5. **Integration:**
   - Works with existing team manager functionality from Phase 3
   - TeamManager roles load correctly after add/remove
   - Managers can participate in tournament invite workflow

## Implementation Notes

1. **Team Context Provider:** If team context provider doesn't exist yet, create following the pattern from tournament and NGB context providers.

2. **Teams Controller:** If controller doesn't exist, create following pattern from `NgbsController.cs`.

3. **WithEmail Extension:** Use the existing `WithEmail` extension method from `UserCollectionExtensions` to query users by email.

4. **Error Handling:** Follow existing patterns for 404 (not found), 400 (bad request), and 403 (forbidden) responses.

5. **Logging:** Add appropriate logging for all operations following existing command patterns.

6. **Transaction Handling:** Use `IDatabaseTransactionProvider` for all database operations to ensure atomicity.

## Open Questions

1. **Self-Management:** Should team managers be able to remove themselves? What if they're the last manager?

2. **Team Manager Self-Add:** Should team managers be able to add other managers, or is that a separate feature (already in Phase 3)?

3. **Audit Trail:** AddedByUserId now tracks which NGB admin (or team manager) added each manager. Should we also track removal operations?

4. **Bulk Operations:** Should we support adding/removing multiple managers in one call?

5. **Manager Permissions:** Should there be different levels of team managers (e.g., owner vs manager)?

6. **Email Notifications:** Should managers receive email when added/removed by NGB admin?
