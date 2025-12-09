# Phase 1: Get and Create Tournament, New Tournament Manager Role

## Overview
This phase establishes the foundation for tournament management by:
- Creating the core Tournament entity and database structure
- Implementing the TournamentIdentifier type with ULID backing
- Creating the TournamentManager role with authorization
- Implementing basic tournament CRUD operations (GET list, GET single, POST create)
- Setting up tournament banner image upload functionality

## Database Changes

### 1. Create New ID Type
**Location:** `src/backend/ManagementHub.Models/Domain/Tournament/TournamentIdentifier.cs`

Create a new strongly-typed ID following the existing pattern from `TestAttemptIdentifier.cs`:
- Use ULID backing with prefix "TR_"
- Implement record struct pattern
- Include static methods: `NewTournamentId()`, `TryParse()`, `Parse()`
- Override `ToString()` to return formatted ID

**Reference pattern:**
```csharp
public record struct TournamentIdentifier(Ulid UniqueId)
{
    private const string IdPrefix = "TR_";
    private const int UlidAsStringLength = 26;
    
    public static TournamentIdentifier NewTournamentId() => new TournamentIdentifier(Ulid.NewUlid());
    // ... TryParse, Parse, ToString methods
}
```

See `src/backend/ManagementHub.Models/Domain/Tests/TestAttemptIdentifier.cs` for complete pattern.

### 2. Create TournamentType Enum
**Location:** `src/backend/ManagementHub.Models/Enums/TournamentType.cs`

```csharp
public enum TournamentType
{
    Club = 0,
    National = 1,
    Youth = 2,
    Fantasy = 3
}
```

**Pattern reference:** See other enums in `src/backend/ManagementHub.Models/Enums/` directory.

### 3. Create Tournament Entity
**Location:** `src/backend/ManagementHub.Storage/Data/Tournament.cs`

**Pattern reference:** Follow `src/backend/ManagementHub.Storage/Data/Team.cs`

Properties needed:
- `long Id` - auto-increment primary key for EF compatibility
- `string UniqueId` - the actual tournament identifier (ULID stored as string)
- `string Name` - tournament name
- `string Description` - tournament description
- `DateOnly StartDate` - when tournament starts
- `DateOnly EndDate` - when tournament ends
- `TournamentType Type` - tournament type enum
- `string Country` - country location
- `string City` - city location
- `string? Place` - specific place/venue
- `bool IsPrivate` - visibility flag
- `DateTime CreatedAt`
- `DateTime UpdatedAt`

Navigation properties (initialize in constructor):
- `ICollection<TournamentManager> TournamentManagers` - for Phase 2
- Leave placeholders for participants (Phase 3)

**Note:** Banner image will be stored using the ActiveStorageAttachment system (not as URL property).

### 4. Create TournamentManager Entity
**Location:** `src/backend/ManagementHub.Storage/Data/TournamentManager.cs`

**Pattern reference:** Follow `src/backend/ManagementHub.Storage/Data/NationalGoverningBodyAdmin.cs`

Properties:
- `long Id` - auto-increment primary key
- `long TournamentId` - foreign key to Tournament
- `long UserId` - foreign key to User
- `DateTime CreatedAt`
- `DateTime UpdatedAt`

Navigation properties:
- `Tournament Tournament`
- `User User`

### 5. Update DbContext
**Location:** `src/backend/ManagementHub.Storage/ManagementHubDbContext.cs`

Add DbSets after existing ones:
```csharp
public virtual DbSet<Tournament> Tournaments { get; set; } = null!;
public virtual DbSet<TournamentManager> TournamentManagers { get; set; } = null!;
```

In `OnModelCreating` method, add entity configurations:

**Pattern reference:** See how Team, NationalGoverningBodyAdmin, and RefereeCertification are configured in the same file (lines 150+).

Configure Tournament:
- Table name: "tournaments"
- Primary key: Id
- Unique index on UniqueId
- Column mappings with snake_case names
- Value converters for DateOnly, TournamentType enum
- Navigation property to TournamentManagers

Configure TournamentManager:
- Table name: "tournament_managers"
- Primary key: Id
- Foreign keys to Tournament and User with `DeleteBehavior.Cascade` on Tournament FK, `DeleteBehavior.Restrict` on User FK
  - When tournament is deleted, its managers are deleted
  - Deleting a user is prevented if they're a tournament manager
- Composite unique index on (TournamentId, UserId)
- Navigation properties

### 6. Create EF Migration
Run in terminal from `src/backend/ManagementHub.Service`:
```powershell
dotnet ef migrations add AddTournamentTables --project ../ManagementHub.Storage
```

Review the generated migration file in `src/backend/ManagementHub.Storage/Migrations/` and ensure:
- Tables are created with proper constraints
- Indexes are added
- Foreign keys are configured

## Authorization

### 1. Create TournamentManager Role
**Location:** `src/backend/ManagementHub.Models/Domain/User/Roles/TournamentManagerRole.cs`

**Pattern reference:** `src/backend/ManagementHub.Models/Domain/User/Roles/NgbAdminRole.cs`

```csharp
public record TournamentManagerRole(TournamentConstraint Tournament) : ITournamentUserRole
{
}
```

### 2. Create TournamentConstraint
**Location:** `src/backend/ManagementHub.Models/Domain/Tournament/TournamentConstraint.cs`

**Pattern reference:** See `NgbConstraint` in `src/backend/ManagementHub.Models/Domain/Ngb/NgbConstraint.cs`

Implement exactly like NgbConstraint:
- `Empty()` - no tournaments (returns empty set)
- `Single(TournamentIdentifier)` - single tournament (calls Set with one item)
- `Set(IEnumerable<TournamentIdentifier>)` - multiple specific tournaments
- `Any` property - all tournaments (for site admins)
- `AppliesTo(TournamentIdentifier)` - check if constraint applies to given tournament

**Implementation pattern:**
```csharp
public abstract class TournamentConstraint
{
    public abstract bool AppliesToAny { get; }
    public abstract bool AppliesTo(TournamentIdentifier tournamentId);
    
    public static TournamentConstraint Any { get; } = new AnyTournamentConstraint();
    public static TournamentConstraint Single(TournamentIdentifier id) => Set(new[] { id });
    public static TournamentConstraint Set(IEnumerable<TournamentIdentifier> ids) => new SetTournamentConstraint(ids);
    public static TournamentConstraint Empty() => new SetTournamentConstraint(Array.Empty<TournamentIdentifier>());
    
    // Private sealed classes: AnyTournamentConstraint, SetTournamentConstraint
}
```

### 3. Create ITournamentUserRole Interface
**Location:** `src/backend/ManagementHub.Models/Abstraction/ITournamentUserRole.cs`

```csharp
public interface ITournamentUserRole : IUserRole
{
    TournamentConstraint Tournament { get; }
}
```

### 4. Create Authorization Requirement
**Location:** `src/backend/ManagementHub.Service/Authorization/TournamentUserRoleAuthorizationRequirement.cs`

**Pattern reference:** `src/backend/ManagementHub.Service/Authorization/UserRoleAuthorizationRequirement.cs` (lines 43-50 for NgbUserRoleAuthorizationRequirement)

```csharp
public class TournamentUserRoleAuthorizationRequirement<TUserRole> : UserRoleAuthorizationRequirement<TUserRole>
    where TUserRole : ITournamentUserRole
{
    override public bool Satisfies(TUserRole role, AuthorizationContext context) =>
        context.RouteParameters.TryGetValue("tournamentId", out var tournamentIdObject) && 
        tournamentIdObject is string tournamentId &&
        role.Tournament.AppliesTo(TournamentIdentifier.Parse(tournamentId));
}
```

### 5. Add Authorization Policy
**Location:** `src/backend/ManagementHub.Service/Authorization/AuthorizationPolicies.cs`

**Pattern reference:** See how NgbAdminPolicy is defined (lines 28-34)

Add:
```csharp
public const string TournamentManagerPolicy = nameof(TournamentManagerPolicy);

public static void AddTournamentManagerPolicy(this AuthorizationOptions options) =>
    options.AddPolicy(TournamentManagerPolicy, policy =>
    {
        policy.AddRequirements(new TournamentUserRoleAuthorizationRequirement<TournamentManagerRole>());
    });
```

### 6. Register Policy in Startup
**Location:** `src/backend/ManagementHub.Service/Program.cs`

Find where authorization options are configured and add:
```csharp
options.AddTournamentManagerPolicy();
```

**Pattern reference:** Search for `AddNgbAdminPolicy` in Program.cs to find the location.

### 7. Update Role Loading
**Location:** Find where user roles are loaded from database (likely in a context accessor or user context provider)

Add logic to load TournamentManagerRole from TournamentManager table.

**Pattern reference:** Search for where NgbAdminRole is loaded from NationalGoverningBodyAdmin.

## API Implementation

### 1. Create Tournament Data Model
**Location:** `src/backend/ManagementHub.Models/Domain/Tournament/TournamentData.cs`

**Pattern reference:** `src/backend/ManagementHub.Models/Domain/Team/TeamData.cs`

Properties matching the entity (without IDs and timestamps):
- Name, Description, StartDate, EndDate, Type, Country, City, Place, IsPrivate

### 2. Create View Models
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/` (new directory)

Create these view models:

**TournamentViewModel.cs:**
```csharp
public class TournamentViewModel
{
    public string Id { get; set; }  // formatted TournamentIdentifier
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Type { get; set; }
    public string Country { get; set; }
    public string Location { get; set; }  // City + Place
    public string? BannerImageUrl { get; set; }
    public List<ManagerViewModel> Organizers { get; set; }
}

public class ManagerViewModel
{
    public string Id { get; set; }  // formatted UserIdentifier
    public string Name { get; set; }
}
```

**TournamentCreateModel.cs:**
```csharp
public class TournamentCreateModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TournamentType Type { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string? Place { get; set; }
    public bool IsPrivate { get; set; }
}
```

**Pattern reference:** See view models in `src/backend/ManagementHub.Service/Areas/Ngbs/`

### 3. Create Tournament Context Provider Interface
**Location:** `src/backend/ManagementHub.Models/Abstraction/Contexts/Providers/ITournamentContextProvider.cs`

Define methods:
- `QueryTournaments()` - returns IQueryable<TournamentContext>
- `GetTournamentContextAsync(TournamentIdentifier)` - get single tournament
- `CreateTournamentAsync(TournamentData, UserIdentifier)` - create tournament and assign creator as manager
- `UpdateTournamentAsync(TournamentIdentifier, TournamentData)` - update existing tournament
- `GetTournamentBannerUriAsync(TournamentIdentifier)` - get banner image URL (retrieves from attachment system)
- `GetTournamentManagerIds(TournamentIdentifier)` - get list of manager user IDs

**Pattern reference:** See `INgbContextProvider` pattern.

**Note on banner retrieval:**
Use `IAttachmentRepository.GetAttachmentAsync(tournamentId, "banner", cancellationToken)` then `IAccessFileCommand.GetFileAccessUriAsync()` - same pattern as user avatar retrieval in `DbUserAvatarContextFactory`.

### 4. Implement Tournament Context Provider
**Location:** `src/backend/ManagementHub.Storage/Contexts/TournamentContextProvider.cs`

Implement the interface defined above.

**Pattern reference:** Look for NgbContextProvider implementation.

### 5. Create Tournaments Controller
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/TournamentsController.cs`

**Pattern reference:** `src/backend/ManagementHub.Service/Areas/Ngbs/NgbsController.cs`

Implement three endpoints:

**a) GET `/api/v2/tournaments`** - List tournaments
- Use `[FromQuery] FilteringParameters filtering` for pagination
- Return `Filtered<TournamentViewModel>`
- Filter private tournaments: only show if user is participant or manager
- Use `.AsFiltered()` extension method

**b) GET `/api/v2/tournaments/{tournamentId}`** - Get single tournament
- Route parameter: `[FromRoute] TournamentIdentifier tournamentId`
- Return 404 if tournament is private and user shouldn't have access
- Load tournament with managers
- Return `TournamentViewModel`

**c) POST `/api/v2/tournaments`** - Create tournament
- Body: `[FromBody] TournamentCreateModel model`
- Create tournament via context provider
- Automatically add creating user as manager
- Return created `TournamentIdentifier` or full `TournamentViewModel`

**d) PUT `/api/v2/tournaments/{tournamentId}`** - Update tournament
- Route parameter: `[FromRoute] TournamentIdentifier tournamentId`
- Body: `[FromBody] TournamentCreateModel model` (reuse same model)
- Authorization: `[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]`
- Updates entire tournament object (full replacement)
- Verify user is a manager of this tournament (policy handles this)
- Return updated `TournamentViewModel`

**Authorization:**
- GET endpoints: `[Authorize]` (any authenticated user)
- POST endpoint: `[Authorize]` (any authenticated user can create)
- PUT endpoint: `[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]` (only managers can update)

**Pattern for checking private tournament access:**
```csharp
// For Phase 1: Check if user is a manager
// For Phase 3: Also check if user is a participant
var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

// Check if user is tournament manager
var isManager = userContext.Roles.OfType<TournamentManagerRole>()
    .Any(r => r.Tournament.AppliesTo(tournamentId));

if (!isManager)
{
    // Phase 3: Add check for participant here
    // For now, deny access to private tournaments if not a manager
    return NotFound(); // Return 404 for private tournaments user shouldn't see
}
```

**Implementation notes:**
- In GET list endpoint: Filter out private tournaments where user is not a manager
- In GET single endpoint: Return 404 if tournament is private and user is not a manager
- This will be extended in Phase 3 to also check participant status

### 6. Create Upload Banner Command
**Location:** `src/backend/ManagementHub.Models/Abstraction/Commands/IUpdateTournamentBannerCommand.cs`

```csharp
public interface IUpdateTournamentBannerCommand
{
    Task<Uri> UpdateTournamentBannerAsync(
        TournamentIdentifier tournamentId,
        string contentType,
        Stream content,
        CancellationToken cancellationToken);
}
```

**Implementation Location:** `src/backend/ManagementHub.Storage/Commands/Tournament/UpdateTournamentBannerCommand.cs`

**Pattern reference:** Follow `UpdateUserAvatarCommand` in `src/backend/ManagementHub.Storage/Commands/User/UpdateUserAvatarCommand.cs`

**Implementation approach:**
- Use `IAttachmentRepository` to store banner via ActiveStorageAttachment/ActiveStorageBlob system
- Attachment name: `"banner"`
- Record type: `"Tournament"` (add to `AttachmentRepository.identifierToRecordTypeMapping`)
- Call `attachmentRepository.UpsertAttachmentAsync(tournamentId, "banner", blob, cancellationToken)`
- Return temporary access URI via `IAccessFileCommand.GetFileAccessUriAsync()`

**Key difference from UserAvatar:**
- User avatar name is `"avatar"`, tournament banner name is `"banner"`
- Record type is `"Tournament"` instead of `"User"`

### 7. Add Banner Upload Endpoint
In TournamentsController:

```csharp
[HttpPut("{tournamentId}/banner")]
[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
public async Task<Uri> UpdateTournamentBanner(
    [FromRoute] TournamentIdentifier tournamentId,
    IFormFile bannerBlob)
{
    var bannerUri = await this.updateTournamentBannerCommand.UpdateTournamentBannerAsync(
        tournamentId,
        bannerBlob.ContentType,
        bannerBlob.OpenReadStream(),
        this.HttpContext.RequestAborted);
    return bannerUri;
}
```

**Pattern reference:** See `UpdateNgbAvatar` in NgbsController (line 138) and `UpdateCurrentUserAvatar` in UsersController (line 159).

### 8. Register Dependencies and Attachment System
**Location:** `src/backend/ManagementHub.Storage/DependencyInjection/DbServiceCollectionExtentions.cs`

Register:
- `ITournamentContextProvider` → `TournamentContextProvider`
- `IUpdateTournamentBannerCommand` → `UpdateTournamentBannerCommand`

**Pattern reference:** See where `IUpdateUserAvatarCommand` is registered (line 118).

**Also update AttachmentRepository:**
**Location:** `src/backend/ManagementHub.Storage/Attachments/AttachmentRepository.cs`

Add to `identifierToRecordTypeMapping` dictionary:
```csharp
private static readonly Dictionary<Type, string> identifierToRecordTypeMapping = new()
{
    [typeof(UserIdentifier)] = "User",
    [typeof(NgbIdentifier)] = "NationalGoverningBody",
    [typeof(TournamentIdentifier)] = "Tournament", // Add this
};
```

This enables the attachment system to store tournament banners.

## Testing Checklist

After implementing:

1. **Database Migration:**
   - Run migration successfully
   - Verify tables created with correct schema
   - Check indexes and foreign keys

2. **Authorization:**
   - Create tournament as authenticated user
   - Verify creator becomes tournament manager
   - Verify TournamentManagerRole is loaded correctly
   - Test policy enforcement on banner upload

3. **API Endpoints:**
   - List tournaments (public + private filtering)
   - Get single tournament (access control)
   - Create tournament (returns valid ID)
   - Update tournament (manager only)
   - Upload banner image

4. **Data Flow:**
   - Tournament creation assigns manager role
   - Manager can upload banner
   - Private tournaments hidden from non-participants
   - Pagination works correctly

## Implementation Notes

1. **Team Type Extension:** Adding "national team" type to `TeamGroupAffiliation` enum is deferred to Phase 3 when tournament-team type validation is needed.

2. **Archived Tournament Logic:** Tournament archival (based on end date) and associated restrictions are deferred to Phase 3 when participants and rosters are being managed.

3. **Tournament Visibility:** Private tournament visibility check is partially implemented in Phase 1:
   - Check if user is a tournament manager
   - Return 404 for private tournaments when user lacks access
   - Phase 3 will extend this to also check participant status

4. **Description Format:** Tournament description is stored as a plain string. The backend doesn't interpret or validate the format. Frontend can choose to render it as markdown if desired.

5. **Banner Image Specifications:** Follow the same patterns and constraints as user avatars:
   - Stored via ActiveStorageAttachment/ActiveStorageBlob system
   - No specific size/format restrictions in backend
   - Frontend can implement any desired constraints

6. **API Versioning:** Using constant `/api/v2/` prefix with manual versioning. No additional versioning strategy needed.

7. **Tournament Updates:** PUT endpoint for updating tournament details is included in Phase 1. It performs full object replacement and requires tournament manager authorization.
