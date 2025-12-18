# Phase 2: Manage Tournament Managers

## Overview
This phase builds on Phase 1 to enable multi-manager tournaments by:
- Implementing endpoints to list tournament managers
- Adding ability to add additional managers
- Adding ability to remove managers
- Ensuring proper authorization checks throughout

## Prerequisites
- Phase 1 must be complete:
  - Tournament entity exists
  - TournamentManager entity exists
  - TournamentManagerRole and authorization policy working
  - Basic tournament CRUD operational

## Database Changes

### Update TournamentManager Entity
**Location:** `src/backend/ManagementHub.Storage/Data/TournamentManager.cs`

Add new property:
- `long AddedByUserId` - foreign key to User who added this manager

Updated properties:
- `long Id` - auto-increment primary key
- `long TournamentId` - foreign key to Tournament
- `long UserId` - foreign key to User (the manager)
- `long AddedByUserId` - foreign key to User (who added this manager)
- `DateTime CreatedAt`
- `DateTime UpdatedAt`

Navigation properties:
- `Tournament Tournament`
- `User User` - the manager
- `User AddedBy` - the user who added this manager

### Create EF Migration
Run in terminal from `src/backend/ManagementHub.Service`:
```powershell
dotnet ef migrations add AddTournamentManagerAddedBy --project ../ManagementHub.Storage
```

## API Implementation

### 1. Extend Tournament Context Provider
**Location:** `src/backend/ManagementHub.Storage/Contexts/TournamentContextProvider.cs` (or wherever implemented in Phase 1)

Add methods to interface and implementation:

```csharp
// Interface additions
Task<IEnumerable<ManagerInfo>> GetTournamentManagersAsync(TournamentIdentifier tournamentId);
Task AddTournamentManagerAsync(
    TournamentIdentifier tournamentId, 
    UserIdentifier userId, 
    UserIdentifier addedByUserId);
Task<bool> RemoveTournamentManagerAsync(TournamentIdentifier tournamentId, UserIdentifier userId);
```

**ManagerInfo class** - Define in appropriate location:
```csharp
public class ManagerInfo
{
    public UserIdentifier UserId { get; set; }
    public string Name { get; set; }  // First + Last name
    public string Email { get; set; }
}
```

**Implementation notes:**
- `GetTournamentManagersAsync`: Join TournamentManager with User to get user details
- `AddTournamentManagerAsync`: 
  - Verify tournament exists
  - Verify user exists using `context.Users.WithIdentifier(userId)` extension method
  - Check if already a manager (idempotent - don't error if already manager)
  - Get user's database ID: `var user = await context.Users.WithIdentifier(userId).FirstOrDefaultAsync(); var userDbId = user.Id;`
  - Insert TournamentManager record with `UserId = userDbId` and `AddedByUserId = addedByUser.Id`
  - Set `CreatedAt` and `UpdatedAt` to current time
- `RemoveTournamentManagerAsync`:
  - Check manager count first - if only 1 manager exists, throw `InvalidOperationException`
  - Use `WithIdentifier(userId)` to get user's database ID for query
  - Return true if removed, false if user wasn't a manager

**Pattern reference:** Use `WithIdentifier` extension from `UserCollectionExtensions` to resolve UserIdentifier to database User entity. This handles both new ULID-based identifiers and legacy numeric IDs.

### 2. Add View Model for Manager List Response
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/TournamentManagerViewModel.cs`

```csharp
public class TournamentManagerViewModel
{
    public string Id { get; set; }  // UserIdentifier as string
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 3. Add View Model for Adding Manager
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/AddTournamentManagerModel.cs`

```csharp
public class AddTournamentManagerModel
{
    public string Email { get; set; }  // Email of user to add
}
```

Alternative: Use UserIdentifier directly if you want to add by ID instead of email.

### 4. Implement Manager Endpoints in Controller
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/TournamentsController.cs`

Add these three endpoints:

**a) GET `/api/v2/tournaments/{tournamentId}/managers`** - List managers

```csharp
[HttpGet("{tournamentId}/managers")]
[Tags("Tournament")]
[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
public async Task<IEnumerable<TournamentManagerViewModel>> GetTournamentManagers(
    [FromRoute] TournamentIdentifier tournamentId)
{
    var managers = await this.tournamentContextProvider.GetTournamentManagersAsync(tournamentId);
    return managers.Select(m => new TournamentManagerViewModel
    {
        Id = m.UserId.ToString(),
        Name = m.Name,
        Email = m.Email
    });
}
```

**b) POST `/api/v2/tournaments/{tournamentId}/managers`** - Add manager

```csharp
[HttpPost("{tournamentId}/managers")]
[Tags("Tournament")]
[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> AddTournamentManager(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromBody] AddTournamentManagerModel model)
{
    // Parse and validate email
    if (!Email.TryParse(model.Email, out var email))
    {
        return BadRequest(new { error = "Invalid email format" });
    }

    // Look up user by email
    var userId = await this.userContextProvider.GetUserIdByEmailAsync(email, this.HttpContext.RequestAborted);
    if (!userId.HasValue)
    {
        return NotFound(new { error = "User not found" });
    }

    // Get current user for audit trail
    var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();

    // Add manager
    await this.tournamentContextProvider.AddTournamentManagerAsync(
        tournamentId, userId.Value, currentUser.UserId);
    
    return Ok();
}
```

**c) DELETE `/api/v2/tournaments/{tournamentId}/managers`** - Remove manager

```csharp
[HttpDelete("{tournamentId}/managers")]
[Tags("Tournament")]
[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> RemoveTournamentManager(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromQuery] string email)
{
    // Parse and validate email
    if (!Email.TryParse(email, out var email_))
    {
        return BadRequest(new { error = "Invalid email format" });
    }

    // Look up user by email
    var userId = await this.userContextProvider.GetUserIdByEmailAsync(email_, this.HttpContext.RequestAborted);
    if (!userId.HasValue)
    {
        return NotFound(new { error = "User not found" });
    }

    // Remove manager
    try
    {
        var removed = await this.tournamentContextProvider.RemoveTournamentManagerAsync(
            tournamentId, userId.Value);
        
        return removed ? Ok() : NotFound(new { error = "User is not a manager" });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("last manager"))
    {
        return BadRequest(new { error = "Cannot remove the last manager of a tournament" });
    }
}
```

**Pattern reference:** See how NGB admin management is done in `src/backend/ManagementHub.Service/Areas/Ngbs/NgbsController.cs` lines 150-196 (AddNgbAdmin, DeleteNgbAdmin).

**Authorization note:** All three endpoints use `TournamentManagerPolicy`, which means the requesting user must already be a manager of that tournament to add/remove other managers.

### 5. Add User Lookup Method
**Location:** `src/backend/ManagementHub.Storage/Contexts/User/DbUserContextProvider.cs` (or wherever user context provider is implemented)

Add to interface (`IUserContextProvider` or similar):
```csharp
Task<UserIdentifier?> GetUserIdByEmailAsync(Email email, CancellationToken cancellationToken = default);
```

**Implementation:**
```csharp
public async Task<UserIdentifier?> GetUserIdByEmailAsync(Email email, CancellationToken cancellationToken = default)
{
    var user = await this.dbContext.Users
        .Where(u => u.Email == email.Value)
        .Select(u => new { u.Id, u.UniqueId })
        .FirstOrDefaultAsync(cancellationToken);
    
    if (user == null)
        return null;
    
    if (user.UniqueId != null)
        return UserIdentifier.Parse(user.UniqueId);
    return UserIdentifier.FromLegacyUserId(user.Id);
}
```

**Note:** There is a `UserStore.FindByEmailAsync` method, but it's specific to authentication and returns `UserIdentity`. We need a simpler method that just returns `UserIdentifier`.

### 6. Role Loading
**Note:** Roles are loaded fresh from the database on each request, so when a user is added or removed as a manager, the change will take effect on their next request. No cache invalidation or special handling is needed.

## Additional Considerations

### 1. Self-Removal
Should a manager be able to remove themselves? Current implementation allows it.

If you want to prevent it:
```csharp
var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
if (userId == currentUser.UserId)
{
    return BadRequest(new { error = "Cannot remove yourself as manager" });
}
```

### 2. Manager Invitation vs Direct Add
Current implementation directly adds user as manager (immediate access). This is simpler and matches the NGB admin pattern. If an invitation workflow is needed later, it can be added as an enhancement.

### 3. Audit Trail
**Decision: Track who added each manager.**

The `AddedByUserId` column has been added to track who added each manager. `CreatedAt` provides the timestamp. For removals, we use hard delete (no soft delete needed for this use case).

## Testing Checklist

After implementing:

1. **List Managers:**
   - Only accessible to tournament managers
   - Returns all managers with correct details
   - Includes email addresses

2. **Add Manager:**
   - Only tournament managers can add
   - Can add by email
   - Handles invalid email format
   - Handles user not found
   - Idempotent (adding twice doesn't error)
   - New manager can immediately access manager endpoints

3. **Remove Manager:**
   - Only tournament managers can remove
   - Can remove by email
   - Handles user not found
   - Handles user not being a manager
   - Removed user loses access to manager endpoints
   - **Cannot remove last manager** - returns 400 Bad Request with appropriate error
   - Database has `AddedByUserId` populated correctly when managers are added

4. **Authorization:**
   - Non-managers cannot access any manager endpoints
   - Managers can only manage their own tournaments
   - Site admins (if applicable) can manage any tournament

5. **Edge Cases:**
   - Tournament with single manager
   - Attempting to remove non-existent user
   - Attempting to add user twice
   - Invalid email formats
   - Concurrent manager additions/removals

## Open Questions

1. **Self-Removal:** Should managers be allowed to remove themselves? What if they're the last manager? (Note: Last manager protection will prevent removing the last manager regardless of who initiates it)

2. **Add by Email vs ID:** The spec shows email, but would adding by UserIdentifier directly be useful (e.g., from a user search/picker in UI)? Current implementation uses email.

3. **Manager Limits:** Should there be a maximum number of managers per tournament?

4. **Manager Permissions:** Are all managers equal, or should we support different permission levels (e.g., owner vs manager)? Current spec implies all equal.

5. **Bulk Operations:** Should we support adding/removing multiple managers in one call, or is one-at-a-time sufficient?
