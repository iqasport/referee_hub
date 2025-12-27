# Phase 4: Manage Team Rosters

## Overview
This phase implements tournament roster management, allowing team managers to specify which players, coaches, and staff from their team will participate in a specific tournament:
- Creating database table for roster entries
- Storing player-specific information (jersey number, gender)
- Implementing roster update endpoint
- Managing sensitive gender data with privacy and deletion requirements
- Enforcing roster modification restrictions for archived tournaments

**Note from Phase 1:** The private tournament filtering and `IsCurrentUserInvolved` computation is done at the database level in `DbTournamentContextProvider.QueryTournaments()`. This phase will further extend the filtering logic to also check if the user is on a tournament roster.

## Prerequisites
- Phase 3 must be complete
- TournamentTeamParticipant entity exists
- TeamManager role and authorization working
- Teams can join tournaments via invites

## Database Changes

### 1. Create UserDelicateInfo Entity
**Location:** `src/backend/ManagementHub.Storage/Data/UserDelicateInfo.cs`

This table stores sensitive user information (specifically gender) with audit tracking for scheduled deletion:

Properties:
- `long Id` - auto-increment primary key
- `long UserId` - foreign key to User (UNIQUE - one record per user)
- `string? Gender` - user's identified gender (nullable, as string for flexibility)
- `DateTime UpdatedAt` - timestamp of last update (for scheduled deletion)
- `DateTime CreatedAt`

Navigation properties:
- `User User`

**Privacy considerations:**
- Single source of truth for gender (no duplication)
- UpdatedAt enables scheduled deletion policies
- Gender can be null if user hasn't provided it
- Must be properly secured with access controls

### 2. Create TournamentTeamRosterEntry Entity
**Location:** `src/backend/ManagementHub.Storage/Data/TournamentTeamRosterEntry.cs`

Properties:
- `long Id` - auto-increment primary key
- `long TournamentTeamParticipantId` - foreign key to TournamentTeamParticipant
- `long UserId` - foreign key to User
- `RosterRole Role` - enum: Player, Coach, Staff
- `string? JerseyNumber` - player's number (null for coaches/staff), max length 5 characters
- `DateTime CreatedAt`
- `DateTime UpdatedAt`

Navigation properties:
- `TournamentTeamParticipant Participant`
- `User User`

**Constraints:**
- Unique index on (TournamentTeamParticipantId, UserId) - user can only appear once per roster
- JerseyNumber required if Role is Player

### 3. Create RosterRole Enum
**Location:** `src/backend/ManagementHub.Models/Enums/RosterRole.cs`

```csharp
public enum RosterRole
{
    Player = 0,
    Coach = 1,
    Staff = 2
}
```

### 4. Update DbContext
**Location:** `src/backend/ManagementHub.Storage/ManagementHubDbContext.cs`

Add DbSets:
```csharp
public virtual DbSet<UserDelicateInfo> UserDelicateInfos { get; set; } = null!;
public virtual DbSet<TournamentTeamRosterEntry> TournamentTeamRosterEntries { get; set; } = null!;
```

In `OnModelCreating`, configure:

**UserDelicateInfo:**
- Table name: "user_delicate_info"
- Primary key: Id
- Unique index on UserId
- Foreign key to User
- Column mappings with snake_case

**TournamentTeamRosterEntry:**
- Table name: "tournament_team_roster_entries"
- Primary key: Id
- Foreign keys to TournamentTeamParticipant and User
- Unique index on (TournamentTeamParticipantId, UserId)
- Enum value converter for RosterRole
- Column mappings with snake_case
- Check constraint: JerseyNumber required when Role = Player
- JerseyNumber max length: 5 characters

### 5. Update TournamentTeamParticipant
Add navigation property:
```csharp
public virtual ICollection<TournamentTeamRosterEntry> RosterEntries { get; set; }
```

Initialize in constructor:
```csharp
this.RosterEntries = new HashSet<TournamentTeamRosterEntry>();
```

### 6. Create EF Migration
```powershell
dotnet ef migrations add AddTournamentRosters --project ../ManagementHub.Storage
```

## Gender Data Management

### 1. Create Gender Data Service Interface
**Location:** `src/backend/ManagementHub.Models/Abstraction/Services/IUserDelicateInfoService.cs`

```csharp
public interface IUserDelicateInfoService
{
    Task<string?> GetUserGenderAsync(UserIdentifier userId);
    Task SetUserGenderAsync(UserIdentifier userId, string? gender);
    Task<Dictionary<UserIdentifier, string?>> GetMultipleUserGendersAsync(
        IEnumerable<UserIdentifier> userIds);
}
```

### 2. Implement Gender Data Service
**Location:** `src/backend/ManagementHub.Storage/Services/UserDelicateInfoService.cs`

Implementation notes:
- `GetUserGenderAsync`: 
  - Use `context.Users.WithIdentifier(userId)` to get user's database ID
  - Query UserDelicateInfo by `UserId == user.Id` (read-only, does not modify UpdatedAt)
- `SetUserGenderAsync`: 
  - Use `context.Users.WithIdentifier(userId)` to get user's database ID
  - Upsert record using `user.Id`, update UpdatedAt timestamp
- `GetMultipleUserGendersAsync`: 
  - For each UserIdentifier, use `WithIdentifier` to get database ID
  - Batch query UserDelicateInfo for efficiency (read-only, does not modify UpdatedAt)

**Pattern reference:** Use `WithIdentifier` extension from `UserCollectionExtensions` to resolve UserIdentifier to database User entity.

### 3. Gender Access Control

Gender should only be visible to:
- The user themselves
- Tournament managers of tournaments where user is a player
- Team managers of teams where user is a player

Implement authorization check:
```csharp
private async Task<bool> CanAccessUserGender(
    UserIdentifier targetUserId,
    UserContext currentUser)
{
    // User can see own gender
    if (targetUserId == currentUser.UserId)
        return true;
    
    // Check if current user is tournament manager where target is playing
    var tournamentManagerRole = currentUser.Roles
        .OfType<TournamentManagerRole>()
        .FirstOrDefault();
    if (tournamentManagerRole != null)
    {
        var isPlayerInTournament = await this.CheckIfUserIsPlayerInManagedTournament(
            targetUserId, tournamentManagerRole);
        if (isPlayerInTournament)
            return true;
    }
    
    // Check if current user is team manager where target is playing
    var teamManagerRole = currentUser.Roles
        .OfType<TeamManagerRole>()
        .FirstOrDefault();
    if (teamManagerRole != null)
    {
        var isPlayerInTeam = await this.CheckIfUserIsPlayerInManagedTeam(
            targetUserId, teamManagerRole);
        if (isPlayerInTeam)
            return true;
    }
    
    return false;
}
```

## API Implementation

### 1. Extend Participant View Model
**Location:** Update `TournamentParticipantViewModel` from Phase 3

Add roster properties:
```csharp
public class TournamentParticipantViewModel
{
    public string TeamId { get; set; }
    public string TeamName { get; set; }
    public string Type { get; set; }
    public List<PlayerViewModel> Players { get; set; }
    public List<StaffViewModel> Coaches { get; set; }
    public List<StaffViewModel> Staff { get; set; }
}

public class PlayerViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Number { get; set; }
    public string? Gender { get; set; }  // Only included if viewer has access
}

public class StaffViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
}
```

### 2. Create Roster Update Model
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/UpdateRosterModel.cs`

```csharp
public class UpdateRosterModel
{
    public List<RosterPlayerModel> Players { get; set; }
    public List<RosterStaffModel> Coaches { get; set; }
    public List<RosterStaffModel> Staff { get; set; }
}

public class RosterPlayerModel
{
    public string UserId { get; set; }
    public string Number { get; set; }
    public string? Gender { get; set; }  // Optional - user can provide/update
}

public class RosterStaffModel
{
    public string UserId { get; set; }
}
```

## Extensions to Phase 1 and Phase 3

### 1. Extend GetUserInvolvedTournamentIdsAsync Method
**Location:** `src/backend/ManagementHub.Storage/Contexts/TournamentContextProvider.cs`

This method from Phase 1 (extended in Phase 3) needs to be further extended to also check if the user is on a tournament roster.

Add query for roster involvement:
```csharp
public async Task<HashSet<TournamentIdentifier>> GetUserInvolvedTournamentIdsAsync(
    UserIdentifier userId,
    List<TournamentIdentifier> tournamentIds)
{
    var tournamentIdsList = tournamentIds.Select(id => id.ToString()).ToList();
    
    // Get user's database ID using WithIdentifier extension
    var user = await context.Users
        .WithIdentifier(userId)
        .Select(u => new { u.Id })
        .FirstOrDefaultAsync();
    
    if (user == null)
    {
        return new HashSet<TournamentIdentifier>();
    }
    
    // Check tournament manager (from Phase 1)
    var managerTournamentIds = await context.TournamentManagers
        .Where(tm => tm.UserId == user.Id && 
                     tournamentIdsList.Contains(tm.Tournament.UniqueId))
        .Select(tm => tm.Tournament.UniqueId)
        .ToListAsync();
    
    // Check if user is team manager for participating teams (from Phase 3)
    var teamManagerTournamentIds = await context.TeamManagers
        .Where(teamMgr => teamMgr.UserId == user.Id)
        .Join(
            context.TournamentTeamParticipants,
            teamMgr => teamMgr.TeamId,
            participant => participant.TeamId,
            (teamMgr, participant) => new { participant.Tournament.UniqueId })
        .Where(p => tournamentIdsList.Contains(p.UniqueId))
        .Select(p => p.UniqueId)
        .Distinct()
        .ToListAsync();
    
    // NEW IN PHASE 4: Check if user is on any tournament roster
    var rosterTournamentIds = await context.TournamentTeamRosterEntries
        .Where(entry => entry.UserId == user.Id)
        .Join(
            context.TournamentTeamParticipants,
            entry => entry.TournamentTeamParticipantId,
            participant => participant.Id,
            (entry, participant) => new { participant.Tournament.UniqueId })
        .Where(p => tournamentIdsList.Contains(p.UniqueId))
        .Select(p => p.UniqueId)
        .Distinct()
        .ToListAsync();
    
    // Combine all lists
    var allInvolvedIds = managerTournamentIds
        .Concat(teamManagerTournamentIds)
        .Concat(rosterTournamentIds)
        .Select(id => TournamentIdentifier.Parse(id))
        .ToHashSet();
    
    return allInvolvedIds;
}
```

### 2. Extend Private Tournament Access Check (GET Single)
**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/TournamentsController.cs`

Update the GET single endpoint (from Phase 3) to also allow roster members to access private tournaments:

```csharp
[HttpGet("{tournamentId}")]
[Tags("Tournament")]
[Authorize]
public async Task<ActionResult<TournamentViewModel>> GetTournament(
    [FromRoute] TournamentIdentifier tournamentId)
{
    // ... fetch tournament ...
    
    // Check access to private tournament
    if (tournament.IsPrivate)
    {
        var isManager = userContext.Roles.OfType<TournamentManagerRole>()
            .Any(r => r.Tournament.AppliesTo(tournamentId));
        
        // Check if user's team is a participant (from Phase 3)
        var isParticipant = false;
        var teamManagerRole = userContext.Roles.OfType<TeamManagerRole>().FirstOrDefault();
        if (teamManagerRole != null)
        {
            var user = await context.Users
                .WithIdentifier(userContext.UserId)
                .FirstOrDefaultAsync();
            
            if (user != null)
            {
                isParticipant = await context.TeamManagers
                    .Where(tm => tm.UserId == user.Id)
                    .Join(
                        context.TournamentTeamParticipants.Where(p => p.Tournament.UniqueId == tournamentId.ToString()),
                        tm => tm.TeamId,
                        p => p.TeamId,
                        (tm, p) => p)
                    .AnyAsync();
            }
        }
        
        // NEW IN PHASE 4: Also check if user is on a tournament roster
        var isOnRoster = false;
        if (!isManager && !isParticipant)
        {
            var user = await context.Users
                .WithIdentifier(userContext.UserId)
                .FirstOrDefaultAsync();
            
            if (user != null)
            {
                isOnRoster = await context.TournamentTeamRosterEntries
                    .Where(entry => entry.UserId == user.Id)
                    .Join(
                        context.TournamentTeamParticipants.Where(p => p.Tournament.UniqueId == tournamentId.ToString()),
                        entry => entry.TournamentTeamParticipantId,
                        p => p.Id,
                        (entry, p) => p)
                    .AnyAsync();
            }
        }
        
        if (!isManager && !isParticipant && !isOnRoster)
        {
            return NotFound();
        }
    }
    
    // Rest of implementation...
}
```

### 3. Extend Context Provider

Add to Tournament context provider:

```csharp
Task UpdateParticipantRosterAsync(
    TournamentIdentifier tournamentId,
    TeamIdentifier teamId,
    RosterUpdateData rosterData);
```

**RosterUpdateData class:**
```csharp
public class RosterUpdateData
{
    public List<RosterPlayerData> Players { get; set; }
    public List<RosterStaffData> Coaches { get; set; }
    public List<RosterStaffData> Staff { get; set; }
}

public class RosterPlayerData
{
    public UserIdentifier UserId { get; set; }
    public string JerseyNumber { get; set; }
    public string? Gender { get; set; }
}

public class RosterStaffData
{
    public UserIdentifier UserId { get; set; }
}
```

### 4. Implement Roster Update Logic

**Location:** In context provider implementation

```csharp
public async Task UpdateParticipantRosterAsync(
    TournamentIdentifier tournamentId,
    TeamIdentifier teamId,
    RosterUpdateData rosterData)
{
    // Get participant
    var participant = await context.TournamentTeamParticipants
        .Include(p => p.RosterEntries)
        .FirstOrDefaultAsync(p => 
            p.Tournament.UniqueId == tournamentId.UniqueId && 
            p.TeamId == teamId.Id);
    
    if (participant == null)
        throw new NotFoundException("Participant not found");
    
    // Validate all users exist and are on the team
    var allUserIds = rosterData.Players.Select(p => p.UserId)
        .Concat(rosterData.Coaches.Select(c => c.UserId))
        .Concat(rosterData.Staff.Select(s => s.UserId))
        .Distinct()
        .ToList();
    
    await ValidateUsersAreTeamMembers(allUserIds, teamId);
    
    // Clear existing roster
    context.TournamentTeamRosterEntries.RemoveRange(participant.RosterEntries);
    
    // Resolve all user identifiers to database IDs upfront
    var userIdMap = new Dictionary<UserIdentifier, long>();
    foreach (var userId in allUserIds)
    {
        var user = await context.Users
            .WithIdentifier(userId)
            .Select(u => new { u.Id })
            .FirstOrDefaultAsync();
        
        if (user == null)
            throw new NotFoundException($"User {userId} not found");
        
        userIdMap[userId] = user.Id;
    }
    
    // Add players
    foreach (var player in rosterData.Players)
    {
        participant.RosterEntries.Add(new TournamentTeamRosterEntry
        {
            UserId = userIdMap[player.UserId],
            Role = RosterRole.Player,
            JerseyNumber = player.JerseyNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        // Update gender if provided
        if (player.Gender != null)
        {
            await userDelicateInfoService.SetUserGenderAsync(
                player.UserId, player.Gender);
        }
    }
    
    // Add coaches
    foreach (var coach in rosterData.Coaches)
    {
        participant.RosterEntries.Add(new TournamentTeamRosterEntry
        {
            UserId = userIdMap[coach.UserId],
            Role = RosterRole.Coach,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
    
    // Add staff
    foreach (var staff in rosterData.Staff)
    {
        participant.RosterEntries.Add(new TournamentTeamRosterEntry
        {
            UserId = userIdMap[staff.UserId],
            Role = RosterRole.Staff,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
    
    await context.SaveChangesAsync();
}
```

### 5. Validate Team Membership

**Location:** Helper method in context provider or separate service

```csharp
private async Task ValidateUsersAreTeamMembers(
    List<UserIdentifier> userIds,
    TeamIdentifier teamId)
{
    // Check that all users are actually on the team
    // This depends on how team membership is tracked in your system
    // May need to check RefereeTeam or similar table
    
    // First, resolve all UserIdentifiers to database IDs
    var userDbIds = new HashSet<long>();
    foreach (var userId in userIds)
    {
        var user = await context.Users
            .WithIdentifier(userId)
            .Select(u => new { u.Id })
            .FirstOrDefaultAsync();
        
        if (user != null)
        {
            userDbIds.Add(user.Id);
        }
    }
    
    var teamMembers = await context.RefereeTeams
        .Where(rt => rt.TeamId == teamId.Id)
        .Select(rt => rt.RefereeId)
        .ToListAsync();
    
    var invalidUserDbIds = userDbIds
        .Where(uid => !teamMembers.Contains(uid))
        .ToList();
    
    if (invalidUserDbIds.Any())
    {
        throw new ValidationException(
            $"Some users are not on team");
    }
}
```

**Note:** The exact implementation depends on how team membership is tracked. Check existing team-user relationship tables. Use `WithIdentifier` extension to resolve UserIdentifier to database ID.

### 6. Update GET Participants Endpoint

Modify the endpoint from Phase 3 to include roster data:

```csharp
[HttpGet("{tournamentId}/participants")]
[Tags("Tournament")]
[Authorize]
public async Task<IEnumerable<TournamentParticipantViewModel>> GetParticipants(
    [FromRoute] TournamentIdentifier tournamentId)
{
    await this.VerifyTournamentAccessAsync(tournamentId);
    
    var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
    
    var participants = await this.tournamentContextProvider
        .GetTournamentParticipantsAsync(tournamentId);
    
    var result = new List<TournamentParticipantViewModel>();
    
    foreach (var participant in participants)
    {
        var roster = await this.LoadParticipantRosterAsync(participant.Id);
        
        // Get gender data with access control
        var playerUserIds = roster.Players.Select(p => p.UserId).ToList();
        var genderData = await this.GetAccessibleGenderDataAsync(
            playerUserIds, currentUser);
        
        result.Add(new TournamentParticipantViewModel
        {
            TeamId = participant.TeamId.ToString(),
            TeamName = participant.TeamName,
            Type = "team",
            Players = roster.Players.Select(p => new PlayerViewModel
            {
                UserId = p.UserId.ToString(),
                UserName = p.UserName,
                Number = p.JerseyNumber,
                Gender = genderData.TryGetValue(p.UserId, out var gender) 
                    ? gender : null
            }).ToList(),
            Coaches = roster.Coaches.Select(c => new StaffViewModel
            {
                UserId = c.UserId.ToString(),
                UserName = c.UserName
            }).ToList(),
            Staff = roster.Staff.Select(s => new StaffViewModel
            {
                UserId = s.UserId.ToString(),
                UserName = s.UserName
            }).ToList()
        });
    }
    
    return result;
}
```

**Helper method for gender access:**
```csharp
private async Task<Dictionary<UserIdentifier, string?>> GetAccessibleGenderDataAsync(
    List<UserIdentifier> userIds,
    UserContext currentUser)
{
    var result = new Dictionary<UserIdentifier, string?>();
    
    foreach (var userId in userIds)
    {
        if (await this.CanAccessUserGender(userId, currentUser))
        {
            var gender = await this.userDelicateInfoService
                .GetUserGenderAsync(userId);
            result[userId] = gender;
        }
    }
    
    return result;
}
```

### 7. Implement PUT Roster Endpoint

```csharp
[HttpPut("{tournamentId}/participants/{teamId}")]
[Tags("Tournament")]
[Authorize(AuthorizationPolicies.TeamManagerPolicy)]
public async Task<IActionResult> UpdateParticipantRoster(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromRoute] TeamIdentifier teamId,
    [FromBody] UpdateRosterModel model)
{
    var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Verify team manager for this specific team
    var isTeamManager = currentUser.Roles.OfType<TeamManagerRole>()
        .Any(r => r.Team.AppliesTo(teamId));
    
    if (!isTeamManager)
    {
        return Forbid();
    }
    
    // Check tournament not archived
    var tournament = await this.tournamentContextProvider
        .GetTournamentContextAsync(tournamentId);
    
    if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
    {
        return BadRequest(new { error = "Cannot modify roster of archived tournament" });
    }
    
    // Note: Gender data in UserDelicateInfo can still be updated even for archived tournaments,
    // as it's user-level data. Only the roster entries themselves are immutable.
    
    // Verify participant exists
    var participant = await this.tournamentContextProvider
        .GetParticipantAsync(tournamentId, teamId);
    
    if (participant == null)
    {
        return NotFound(new { error = "Team is not a participant" });
    }
    
    // Parse and validate roster data
    var rosterData = new RosterUpdateData
    {
        Players = model.Players.Select(p => new RosterPlayerData
        {
            UserId = UserIdentifier.Parse(p.UserId),
            JerseyNumber = p.Number,
            Gender = p.Gender
        }).ToList(),
        Coaches = model.Coaches.Select(c => new RosterStaffData
        {
            UserId = UserIdentifier.Parse(c.UserId)
        }).ToList(),
        Staff = model.Staff.Select(s => new RosterStaffData
        {
            UserId = UserIdentifier.Parse(s.UserId)
        }).ToList()
    };
    
    // Validate jersey numbers are unique
    var duplicateNumbers = rosterData.Players
        .GroupBy(p => p.JerseyNumber)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();
    
    if (duplicateNumbers.Any())
    {
        return BadRequest(new { 
            error = $"Duplicate jersey numbers: {string.Join(", ", duplicateNumbers)}" 
        });
    }
    
    // Update roster
    try
    {
        await this.tournamentContextProvider.UpdateParticipantRosterAsync(
            tournamentId, teamId, rosterData);
    }
    catch (ValidationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
    
    return Ok();
}
```

## User Gender Management Endpoints

### 1. Get User's Own Gender Data
**Location:** `src/backend/ManagementHub.Service/Areas/Users/UsersController.cs`

```csharp
[HttpGet("me/gender")]
[Authorize]
public async Task<UserGenderViewModel> GetMyGender()
{
    var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
    
    var gender = await this.userDelicateInfoService
        .GetUserGenderAsync(currentUser.UserId);
    
    // Get user's database ID
    var user = await this.context.Users
        .WithIdentifier(currentUser.UserId)
        .Select(u => new { u.Id })
        .FirstOrDefaultAsync();
    
    if (user == null)
    {
        return new UserGenderViewModel
        {
            Gender = gender,
            ReferencedInTournaments = new List<TournamentReferenceViewModel>()
        };
    }
    
    // Get tournaments where this gender is referenced
    var tournaments = await this.context.TournamentTeamRosterEntries
        .Where(entry => entry.UserId == user.Id && 
                        entry.Role == RosterRole.Player)
        .Join(
            this.context.TournamentTeamParticipants,
            entry => entry.TournamentTeamParticipantId,
            participant => participant.Id,
            (entry, participant) => participant.Tournament)
        .Select(t => new TournamentReferenceViewModel
        {
            Id = t.UniqueId,
            Name = t.Name,
            StartDate = t.StartDate,
            EndDate = t.EndDate
        })
        .ToListAsync();
    
    return new UserGenderViewModel
    {
        Gender = gender,
        ReferencedInTournaments = tournaments
    };
}
```

### 2. Delete User's Own Gender Data
**Location:** `src/backend/ManagementHub.Service/Areas/Users/UsersController.cs`

```csharp
[HttpDelete("me/gender")]
[Authorize]
public async Task<IActionResult> DeleteMyGender()
{
    var currentUser = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Get user's database ID
    var user = await this.context.Users
        .WithIdentifier(currentUser.UserId)
        .Select(u => new { u.Id })
        .FirstOrDefaultAsync();
    
    if (user != null)
    {
        await this.context.UserDelicateInfos
            .Where(udi => udi.UserId == user.Id)
            .ExecuteDeleteAsync();
        
        this.logger.LogInformation(
            "User {UserId} explicitly deleted their gender data",
            currentUser.UserId);
    }
    
    return Ok();
}
```

**Note:** Gender field in roster entries remains nullable. When gender is deleted, roster entries continue to function but without gender information.

### 3. View Models
**Location:** `src/backend/ManagementHub.Service/Areas/Users/`

```csharp
public class UserGenderViewModel
{
    public string? Gender { get; set; }
    public List<TournamentReferenceViewModel> ReferencedInTournaments { get; set; }
}

public class TournamentReferenceViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
```

## Scheduled Gender Data Deletion

### 1. Create Background Job
**Location:** `src/backend/ManagementHub.Service/Jobs/CleanupStaleGenderDataJob.cs`

**Pattern reference:** Follow `EnsureMonthlyStatsSnapshot.cs` in Jobs folder.

```csharp
using Hangfire;
using ManagementHub.Models.Abstraction.Commands;

namespace ManagementHub.Service.Jobs;

public class CleanupStaleGenderDataJob : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<ICleanupStaleGenderDataCommand>(
            "CleanupStaleGenderData",
            cmd => cmd.CleanupStaleGenderDataAsync(default),
            Cron.Daily, // Run daily
            TimeZoneInfo.Utc);

        return Task.CompletedTask;
    }
}
```

### 2. Create Cleanup Command Interface
**Location:** `src/backend/ManagementHub.Models/Abstraction/Commands/ICleanupStaleGenderDataCommand.cs`

```csharp
public interface ICleanupStaleGenderDataCommand
{
    Task CleanupStaleGenderDataAsync(CancellationToken cancellationToken);
}
```

### 3. Implement Cleanup Command
**Location:** `src/backend/ManagementHub.Storage/Commands/User/CleanupStaleGenderDataCommand.cs`

```csharp
public class CleanupStaleGenderDataCommand : ICleanupStaleGenderDataCommand
{
    private readonly ManagementHubDbContext context;
    private readonly IOptionsSnapshot<GenderDataRetentionSettings> settings;
    private readonly ILogger<CleanupStaleGenderDataCommand> logger;

    public CleanupStaleGenderDataCommand(
        ManagementHubDbContext context,
        IOptionsSnapshot<GenderDataRetentionSettings> settings,
        ILogger<CleanupStaleGenderDataCommand> logger)
    {
        this.context = context;
        this.settings = settings;
        this.logger = logger;
    }

    public async Task CleanupStaleGenderDataAsync(CancellationToken cancellationToken)
    {
        if (!this.settings.Value.EnableAutomaticDeletion)
        {
            this.logger.LogInformation("Automatic gender data deletion is disabled");
            return;
        }

        using var transaction = await this.context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var updateThreshold = now.AddMonths(-this.settings.Value.NotUpdatedForMonths);
        var tournamentEndThreshold = now.AddMonths(-this.settings.Value.MonthsSinceLastTournamentEnded);

        // Find users with stale gender data
        var staleGenderData = await this.context.UserDelicateInfos
            .Where(udi => udi.UpdatedAt < updateThreshold)
            .Select(udi => new { udi.UserId, udi.UpdatedAt })
            .ToListAsync(cancellationToken);

        this.logger.LogInformation(
            "Found {Count} gender records not updated since {Threshold}",
            staleGenderData.Count,
            updateThreshold);

        var deletedCount = 0;

        foreach (var record in staleGenderData)
        {
            // Check if user has played in any tournament that ended recently
            var hasRecentTournament = await this.context.TournamentTeamRosterEntries
                .Where(rosterEntry => 
                    rosterEntry.UserId == record.UserId &&
                    rosterEntry.Role == RosterRole.Player)
                .Join(
                    this.context.TournamentTeamParticipants,
                    rosterEntry => rosterEntry.TournamentTeamParticipantId,
                    participant => participant.Id,
                    (rosterEntry, participant) => participant.Tournament)
                .AnyAsync(tournament => 
                    tournament.EndDate > DateOnly.FromDateTime(tournamentEndThreshold),
                    cancellationToken);

            if (!hasRecentTournament)
            {
                // Gender data is stale - delete it
                await this.context.UserDelicateInfos
                    .Where(udi => udi.UserId == record.UserId)
                    .ExecuteDeleteAsync(cancellationToken);
                
                deletedCount++;
                
                this.logger.LogInformation(
                    "Deleted stale gender data for user {UserId} (last updated: {UpdatedAt})",
                    record.UserId,
                    record.UpdatedAt);
            }
        }

        this.logger.LogInformation(
            "Deleted {DeletedCount} stale gender records out of {TotalCount} candidates",
            deletedCount,
            staleGenderData.Count);

        await transaction.CommitAsync(cancellationToken);
    }
}
```

### 4. Configuration Settings Class
**Location:** `src/backend/ManagementHub.Service/Configuration/GenderDataRetentionSettings.cs`

```csharp
public class GenderDataRetentionSettings
{
    public int NotUpdatedForMonths { get; set; } = 6;
    public int MonthsSinceLastTournamentEnded { get; set; } = 3;
    public bool EnableAutomaticDeletion { get; set; } = true;
}
```

### 5. Configuration in appsettings.json

Add to `appsettings.json`:
```json
{
  "GenderDataRetention": {
    "NotUpdatedForMonths": 6,
    "MonthsSinceLastTournamentEnded": 3,
    "EnableAutomaticDeletion": true
  }
}
```

**Note:** Both time periods must be satisfied for deletion:
1. Gender record not updated in more than 6 months (configurable via `NotUpdatedForMonths`)
2. AND it's been 3 months since the last tournament the player participated in has ended (configurable via `MonthsSinceLastTournamentEnded`)

### 6. Register Dependencies
**Location:** `src/backend/ManagementHub.Storage/DependencyInjection/DbServiceCollectionExtentions.cs`

Register command:
```csharp
services.AddScoped<ICleanupStaleGenderDataCommand, CleanupStaleGenderDataCommand>();
```

**Location:** `src/backend/ManagementHub.Service/Program.cs`

Configure settings:
```csharp
services.Configure<GenderDataRetentionSettings>(
    configuration.GetSection("GenderDataRetention"));
```

Register job:
```csharp
services.AddHostedService<CleanupStaleGenderDataJob>();
```

**Pattern reference:** See how `EnsureMonthlyStatsSnapshot` is registered in Program.cs.

### 7. Register User Gender Service
**Location:** `src/backend/ManagementHub.Storage/DependencyInjection/DbServiceCollectionExtentions.cs`

Register service:
```csharp
services.AddScoped<IUserDelicateInfoService, UserDelicateInfoService>();
```

## Testing Checklist

1. **Roster CRUD:**
   - Team manager can set roster
   - Roster stored correctly with roles
   - Can update roster (full replacement)
   - Cannot set roster for non-participant team

2. **Validation:**
   - All roster users must be team members
   - Jersey numbers must be unique
   - Jersey numbers required for players
   - Cannot modify archived tournament roster

3. **Gender Data:**
   - Gender stored in separate table
   - Gender visible to appropriate users only
   - Gender hidden from unauthorized users
   - Gender can be updated via roster
   - UpdatedAt timestamp updated on write only
   - User can view their own gender and tournaments it's used in
   - User can delete their own gender data
   - Roster functions correctly when gender is null

4. **Authorization:**
   - Only team manager can update their roster
   - Tournament managers can view all rosters with gender
   - Other team managers cannot see other teams' gender data
   - Players can see their own gender

5. **Participant Listing:**
   - Rosters included in participant list
   - Gender filtered based on viewer permissions
   - Empty rosters handled correctly
   - Large rosters perform acceptably

## Design Decisions

1. **Team Membership:** Team membership tracking will be implemented in a later phase. For now, roster validation against team membership is deferred.

2. **Gender Validation:** No validation on gender values to be inclusive. Users can provide any string value.

3. **User Gender Endpoints:** Added GET and DELETE endpoints under `/api/v2/users/me/gender` for users to view and delete their own gender data.

4. **Gender Deletion:** Users can explicitly delete their gender data. Gender field in roster entries is nullable, so rosters continue functioning without gender information.

5. **Roster Size Limits:** No limits enforced at this time. Tournament managers handle capacity requirements.

6. **Jersey Number Format:** Max length is 5 characters. No other format validation (allows letters, numbers, special characters).

7. **Roster History:** Only current roster is stored. No historical tracking of roster changes.

8. **Partial Updates:** API accepts full roster replacement. Frontend can implement add/delete UX, but calls full roster endpoint.

9. **Gender Context:** Gender is user-level property stored once in UserDelicateInfo, not per-tournament. Based on assumption users identify with one gender at a time.

10. **Player Participation Rules:** Rules about playing for multiple teams will be added later once team membership is implemented.

11. **Archived Tournament Rosters:** Rosters for archived tournaments are immutable (cannot modify roster entries). However, gender data in UserDelicateInfo can still be updated as it's user-level data.

12. **Concurrent Updates:** Last write wins. No concurrency control at this scale.

13. **User Notification:** No notification sent before automatic gender data deletion.
