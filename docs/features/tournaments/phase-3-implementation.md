# Phase 3: Manage Invites and Create Participants

## Overview
This phase implements the tournament invitation/join system and team participant management:
- Creating database tables for invites and team participants
- Implementing invite creation (by manager or team requesting to join)
- Implementing invite approval/rejection workflow
- Converting approved invites to participants
- Adding TeamManager role and authorization
- Implementing participant listing and removal
- Adding email notifications
- Extending TeamGroupAffiliation enum for tournament type validation

## Prerequisites
- Phase 1 and 2 must be complete
- Tournament and TournamentManager entities exist
- TournamentManagerRole and authorization working

## Database Changes

### 1. Extend TeamGroupAffiliation Enum
**Location:** `src/backend/ManagementHub.Models/Enums/TeamGroupAffiliation.cs`

Add new value:
```csharp
[EnumMember(Value = "national")]
National = 4,
```

**Note:** Both University and Community are considered "club teams" for tournament type validation.

### 2. Create ApprovalStatus Enum
**Location:** `src/backend/ManagementHub.Models/Enums/ApprovalStatus.cs`

```csharp
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
```

### 3. Create TournamentInvite Entity
**Location:** `src/backend/ManagementHub.Storage/Data/TournamentInvite.cs`

Properties:
- `long Id` - auto-increment primary key
- `long TournamentId` - foreign key to Tournament
- `string ParticipantType` - "team" initially, extensible for future
- `string ParticipantId` - TeamIdentifier as string initially
- `long InitiatorUserId` - foreign key to User who created invite
- `DateTime CreatedAt`
- `ApprovalStatus TournamentManagerApproval` - manager's decision
- `DateTime? TournamentManagerApprovalDate`
- `ApprovalStatus ParticipantApproval` - participant's decision
- `DateTime? ParticipantApprovalDate`

Navigation properties:
- `Tournament Tournament`
- `User Initiator`

**Pattern reference:** Similar to join table entities like RefereeCertification.

**Computed Status:** Don't store status in DB. Derive it:
- Rejected if either approval is Rejected
- Approved if both approvals are Approved
- Pending otherwise

### 4. Create TournamentTeamParticipant Entity
**Location:** `src/backend/ManagementHub.Storage/Data/TournamentTeamParticipant.cs`

Properties:
- `long Id` - auto-increment primary key
- `long TournamentId` - foreign key to Tournament
- `long TeamId` - foreign key to Team
- `string TeamName` - denormalized for historical record
- `DateTime CreatedAt`
- `DateTime UpdatedAt`

Navigation properties:
- `Tournament Tournament`
- `Team Team`
- `ICollection<TournamentTeamRosterEntry> RosterEntries` - Phase 4

**Note:** TeamName is denormalized because team names can change, but tournament records should preserve the name at time of participation.

### 5. Create TeamManager Entity
**Location:** `src/backend/ManagementHub.Storage/Data/TeamManager.cs`

**Pattern reference:** Identical pattern to TournamentManager

Properties:
- `long Id` - auto-increment primary key
- `long TeamId` - foreign key to Team
- `long UserId` - foreign key to User
- `DateTime CreatedAt`
- `DateTime UpdatedAt`

Navigation properties:
- `Team Team`
- `User User`

**Note:** This table may already exist or team management may be handled differently. Check existing codebase first.

### 6. Update DbContext
**Location:** `src/backend/ManagementHub.Storage/ManagementHubDbContext.cs`

Add DbSets:
```csharp
public virtual DbSet<TournamentInvite> TournamentInvites { get; set; } = null!;
public virtual DbSet<TournamentTeamParticipant> TournamentTeamParticipants { get; set; } = null!;
public virtual DbSet<TeamManager> TeamManagers { get; set; } = null!;  // if not exists
```

In `OnModelCreating`, configure:

**TournamentInvite:**
- Table name: "tournament_invites"
- Primary key: Id
- Foreign keys to Tournament and User
- Composite unique index on (TournamentId, ParticipantType, ParticipantId) WHERE status != Rejected
  - Ensures only one pending or approved invite per participant
  - Allows re-invitation after rejection (rejected invites kept for audit trail)
- Column mappings with snake_case
- Enum value converters

**TournamentTeamParticipant:**
- Table name: "tournament_team_participants"
- Primary key: Id
- Foreign keys to Tournament and Team
- Unique index on (TournamentId, TeamId)
- Column mappings with snake_case

**TeamManager:**
- Table name: "team_managers"
- Follow same pattern as TournamentManager

### 7. Create EF Migration
```powershell
dotnet ef migrations add AddTournamentInvitesAndParticipants --project ../ManagementHub.Storage
```

## Authorization

### 1. Create TeamManager Role
**Location:** `src/backend/ManagementHub.Models/Domain/User/Roles/TeamManagerRole.cs`

**Pattern reference:** Similar to TournamentManagerRole

```csharp
public record TeamManagerRole(TeamConstraint Team) : ITeamUserRole
{
}
```

### 2. Create TeamConstraint
**Location:** `src/backend/ManagementHub.Models/Domain/Team/TeamConstraint.cs`

**Pattern reference:** Similar to TournamentConstraint

Methods:
- `Empty()`, `Single(TeamIdentifier)`, `All()`
- `AppliesTo(TeamIdentifier)`

### 3. Create ITeamUserRole Interface
**Location:** `src/backend/ManagementHub.Models/Abstraction/ITeamUserRole.cs`

```csharp
public interface ITeamUserRole : IUserRole
{
    TeamConstraint Team { get; }
}
```

### 4. Create Team Authorization Requirement and Policy
**Location:** `src/backend/ManagementHub.Service/Authorization/`

Create `TeamUserRoleAuthorizationRequirement` similar to Tournament and Ngb versions.

Add to `AuthorizationPolicies`:
```csharp
public const string TeamManagerPolicy = nameof(TeamManagerPolicy);

public static void AddTeamManagerPolicy(this AuthorizationOptions options) =>
    options.AddPolicy(TeamManagerPolicy, policy =>
    {
        policy.AddRequirements(new TeamUserRoleAuthorizationRequirement<TeamManagerRole>());
    });
```

Register in Program.cs.

### 5. Load TeamManager Roles
Find user role loading logic and add TeamManager role loading from TeamManager table.

## Business Logic

### 1. Tournament-Team Type Validation
**Location:** Create utility or add to context provider

Implement validation logic:
```csharp
public bool CanTeamJoinTournament(TournamentType tournamentType, TeamGroupAffiliation? teamAffiliation)
{
    return tournamentType switch
    {
        TournamentType.Club => teamAffiliation is TeamGroupAffiliation.University 
                               or TeamGroupAffiliation.Community,
        TournamentType.National => teamAffiliation is TeamGroupAffiliation.National,
        TournamentType.Youth => teamAffiliation is TeamGroupAffiliation.Youth,
        TournamentType.Fantasy => true,  // Any team can join
        _ => false
    };
}
```

### 2. Invite Status Computation
**Location:** Create utility or extension method

```csharp
public static InviteStatus GetInviteStatus(
    ApprovalStatus managerApproval, 
    ApprovalStatus participantApproval)
{
    if (managerApproval == ApprovalStatus.Rejected || 
        participantApproval == ApprovalStatus.Rejected)
        return InviteStatus.Rejected;
    
    if (managerApproval == ApprovalStatus.Approved && 
        participantApproval == ApprovalStatus.Approved)
        return InviteStatus.Approved;
    
    return InviteStatus.Pending;
}

public enum InviteStatus { Pending, Approved, Rejected }
```

### 3. Auto-Approval Logic
When creating an invite, if the initiator has both roles (tournament manager AND team manager for the relevant entities), automatically approve both sides:

```csharp
var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
    .Any(r => r.Tournament.AppliesTo(tournamentId));
var isTeamManager = userContext.Roles.OfType<TeamManagerRole>()
    .Any(r => r.Team.AppliesTo(teamId));

var invite = new TournamentInvite
{
    // ... other properties
    TournamentManagerApproval = isTournamentManager 
        ? ApprovalStatus.Approved : ApprovalStatus.Pending,
    ParticipantApproval = isTeamManager 
        ? ApprovalStatus.Approved : ApprovalStatus.Pending,
};
```

## API Implementation

### 1. Create View Models

**Location:** `src/backend/ManagementHub.Service/Areas/Tournaments/`

**TournamentInviteViewModel.cs:**
```csharp
public class TournamentInviteViewModel
{
    public string Id { get; set; }  // Invite ID
    public string ParticipantType { get; set; }
    public string ParticipantId { get; set; }
    public string ParticipantName { get; set; }  // Team name
    public string Status { get; set; }  // "pending", "approved", "rejected"
    public string InitiatorUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApprovalStatusViewModel TournamentManagerApproval { get; set; }
    public ApprovalStatusViewModel ParticipantApproval { get; set; }
}

public class ApprovalStatusViewModel
{
    public string Status { get; set; }  // "pending", "approved", "rejected"
    public DateTime? Date { get; set; }
}
```

**CreateInviteModel.cs:**
```csharp
public class CreateInviteModel
{
    public string ParticipantType { get; set; }  // "team" for now
    public string ParticipantId { get; set; }  // TeamIdentifier string
}
```

**InviteResponseModel.cs:**
```csharp
public class InviteResponseModel
{
    public bool Approved { get; set; }  // true = approve, false = reject
}
```

**TournamentParticipantViewModel.cs:**
```csharp
public class TournamentParticipantViewModel
{
    public string TeamId { get; set; }
    public string TeamName { get; set; }
    public string Type { get; set; }  // "team"
    // Roster details - Phase 4
}
```

### 2. Extend Context Providers

Add to Tournament context provider:

```csharp
Task<IEnumerable<TournamentInviteInfo>> GetTournamentInvitesAsync(
    TournamentIdentifier tournamentId, 
    UserIdentifier? filterByParticipant = null);

Task<TournamentInviteInfo> CreateInviteAsync(
    TournamentIdentifier tournamentId,
    string participantType,
    string participantId,
    UserIdentifier initiatorUserId);

Task UpdateInviteApprovalAsync(
    long inviteId,
    bool isTournamentManager,
    bool approved);

Task<IEnumerable<TournamentParticipantInfo>> GetTournamentParticipantsAsync(
    TournamentIdentifier tournamentId);

Task RemoveParticipantAsync(
    TournamentIdentifier tournamentId,
    TeamIdentifier teamId);
```

### 3. Implement Invite Endpoints

**a) GET `/api/v2/tournaments/{tournamentId}/invites`** - List invites

```csharp
[HttpGet("{tournamentId}/invites")]
[Tags("Tournament")]
[Authorize]
public async Task<IEnumerable<TournamentInviteViewModel>> GetTournamentInvites(
    [FromRoute] TournamentIdentifier tournamentId)
{
    var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
    
    var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
        .Any(r => r.Tournament.AppliesTo(tournamentId));
    
    // Managers see all invites, participants see only their own
    var filterByParticipant = isTournamentManager ? null : userContext.UserId;
    
    var invites = await this.tournamentContextProvider.GetTournamentInvitesAsync(
        tournamentId, filterByParticipant);
    
    return invites.Select(i => new TournamentInviteViewModel
    {
        // Map properties
    });
}
```

**Authorization:** Any authenticated user (but filtering applied based on role).

**b) POST `/api/v2/tournaments/{tournamentId}/invites`** - Create invite

```csharp
[HttpPost("{tournamentId}/invites")]
[Tags("Tournament")]
[Authorize]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<ActionResult<TournamentInviteViewModel>> CreateInvite(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromBody] CreateInviteModel model)
{
    var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Validate participant type
    if (model.ParticipantType != "team")
    {
        return BadRequest(new { error = "Only team participants supported" });
    }
    
    // Parse team ID
    if (!TeamIdentifier.TryParse(model.ParticipantId, out var teamId))
    {
        return BadRequest(new { error = "Invalid participant ID" });
    }
    
    // Check authorization: must be tournament manager OR team manager
    var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
        .Any(r => r.Tournament.AppliesTo(tournamentId));
    var isTeamManager = userContext.Roles.OfType<TeamManagerRole>()
        .Any(r => r.Team.AppliesTo(teamId));
    
    if (!isTournamentManager && !isTeamManager)
    {
        return Forbid();
    }
    
    // Check tournament not archived
    var tournament = await this.tournamentContextProvider
        .GetTournamentContextAsync(tournamentId);
    if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
    {
        return BadRequest(new { error = "Cannot modify archived tournament" });
    }
    
    // Check if pending invite already exists
    var existingInvites = await this.tournamentContextProvider
        .GetTournamentInvitesAsync(tournamentId);
    if (existingInvites.Any(i => i.ParticipantId == model.ParticipantId && 
                                  i.Status == InviteStatus.Pending))
    {
        return BadRequest(new { error = "Pending invite already exists" });
    }
    
    // Validate tournament-team type compatibility
    var team = await this.teamContextProvider.GetTeamContextAsync(teamId);
    
    if (!CanTeamJoinTournament(tournament.Type, team.GroupAffiliation))
    {
        return BadRequest(new { error = "Team type incompatible with tournament" });
    }
    
    // Create invite
    var invite = await this.tournamentContextProvider.CreateInviteAsync(
        tournamentId,
        model.ParticipantType,
        model.ParticipantId,
        userContext.UserId);
    
    // Send email notifications
    await this.emailNotificationService.SendInviteCreatedNotificationsAsync(
        tournamentId, teamId, invite);
    
    // Map to view model and return
    return CreatedAtAction(nameof(GetTournamentInvites), 
        new { tournamentId = tournamentId.ToString() },
        MapToViewModel(invite));
}
```

**c) POST `/api/v2/tournaments/{tournamentId}/invites/{participantId}`** - Respond to invite

```csharp
[HttpPost("{tournamentId}/invites/{participantId}")]
[Tags("Tournament")]
[Authorize]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> RespondToInvite(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromRoute] string participantId,
    [FromBody] InviteResponseModel response)
{
    var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
    
    // Get pending invite
    var invites = await this.tournamentContextProvider
        .GetTournamentInvitesAsync(tournamentId);
    var invite = invites.FirstOrDefault(i => 
        i.ParticipantId == participantId && 
        i.Status == InviteStatus.Pending);
    
    if (invite == null)
    {
        return NotFound(new { error = "No pending invite found" });
    }
    
    // Check tournament not archived
    var tournament = await this.tournamentContextProvider
        .GetTournamentContextAsync(tournamentId);
    if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
    {
        return BadRequest(new { error = "Cannot modify archived tournament" });
    }
    
    // Check authorization and determine which approval to update
    var isTournamentManager = userContext.Roles.OfType<TournamentManagerRole>()
        .Any(r => r.Tournament.AppliesTo(tournamentId));
    
    var isParticipant = false;
    if (invite.ParticipantType == "team")
    {
        if (TeamIdentifier.TryParse(invite.ParticipantId, out var teamId))
        {
            isParticipant = userContext.Roles.OfType<TeamManagerRole>()
                .Any(r => r.Team.AppliesTo(teamId));
        }
    }
    
    // Must be either tournament manager (with pending manager approval)
    // or participant (with pending participant approval)
    var canApproveasManager = isTournamentManager && 
        invite.TournamentManagerApprovalStatus == ApprovalStatus.Pending;
    var canApproveAsParticipant = isParticipant && 
        invite.ParticipantApprovalStatus == ApprovalStatus.Pending;
    
    if (!canApproveasManager && !canApproveAsParticipant)
    {
        return Forbid();
    }
    
    // Update approval
    await this.tournamentContextProvider.UpdateInviteApprovalAsync(
        invite.Id,
        isTournamentManager,
        response.Approved);
    
    // Reload to check if fully approved
    var updatedInvite = await this.tournamentContextProvider
        .GetInviteByIdAsync(invite.Id);
    
    // If both approved, create participant
    if (GetInviteStatus(updatedInvite) == InviteStatus.Approved)
    {
        var teamId = TeamIdentifier.Parse(invite.ParticipantId);
        await this.tournamentContextProvider.AddParticipantAsync(
            tournamentId, teamId);
    }
    
    // Send email notifications
    await this.emailNotificationService.SendInviteResponseNotificationsAsync(
        tournamentId, invite, response.Approved);
    
    return Ok();
}
```

### 4. Implement Participant Endpoints

**a) GET `/api/v2/tournaments/{tournamentId}/participants`** - List participants

```csharp
[HttpGet("{tournamentId}/participants")]
[Tags("Tournament")]
[Authorize]
public async Task<IEnumerable<TournamentParticipantViewModel>> GetParticipants(
    [FromRoute] TournamentIdentifier tournamentId)
{
    // Check access: public tournament or user is manager/participant
    await this.VerifyTournamentAccessAsync(tournamentId);
    
    var participants = await this.tournamentContextProvider
        .GetTournamentParticipantsAsync(tournamentId);
    
    return participants.Select(p => new TournamentParticipantViewModel
    {
        TeamId = p.TeamId.ToString(),
        TeamName = p.TeamName,
        Type = "team",
        // Roster from Phase 4
    });
}
```

**b) DELETE `/api/v2/tournaments/{tournamentId}/participants/{teamId}`** - Remove participant

```csharp
[HttpDelete("{tournamentId}/participants/{teamId}")]
[Tags("Tournament")]
[Authorize(AuthorizationPolicies.TournamentManagerPolicy)]
public async Task<IActionResult> RemoveParticipant(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromRoute] TeamIdentifier teamId)
{
    // Check tournament not archived
    var tournament = await this.tournamentContextProvider
        .GetTournamentContextAsync(tournamentId);
    if (tournament.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
    {
        return BadRequest(new { error = "Cannot modify archived tournament" });
    }
    
    await this.tournamentContextProvider.RemoveParticipantAsync(
        tournamentId, teamId);
    
    return Ok();
}
```

**Note:** Removing participant should allow them to be re-invited (delete participant record but not invites, or mark participant as inactive).

## Email Notifications

### 1. Create Email Templates
**Location:** `src/backend/ManagementHub.Mailers/Templates/`

Create templates for:
- `TournamentInviteCreated` - sent when invite created
- `TournamentInviteApproved` - sent when invite fully approved
- `TournamentInviteRejected` - sent when invite rejected

**Pattern reference:** Look at existing email templates in the Mailers project.

### 2. Create Email Notification Service
**Location:** `src/backend/ManagementHub.Service/Services/ITournamentEmailNotificationService.cs`

Methods:
```csharp
Task SendInviteCreatedNotificationsAsync(
    TournamentIdentifier tournamentId,
    TeamIdentifier teamId,
    InviteInfo invite);

Task SendInviteResponseNotificationsAsync(
    TournamentIdentifier tournamentId,
    InviteInfo invite,
    bool approved);
```

Implementation should:
- Get tournament manager emails
- Get team manager emails
- Send appropriate template to both parties
- Handle errors gracefully (log but don't fail request)

**Pattern reference:** Look for existing email service patterns in the codebase.

## Testing Checklist

1. **Team Type Validation:**
   - Club tournaments only accept University/Community teams
   - National tournaments only accept National teams
   - Youth tournaments only accept Youth teams
   - Fantasy tournaments accept any team

2. **Invite Creation:**
   - Tournament manager can invite team
   - Team manager can request to join
   - User with both roles auto-approves
   - Cannot create invite if pending invite exists
   - Email sent to both parties

3. **Invite Response:**
   - Manager can approve/reject when pending
   - Participant can approve/reject when pending
   - Cannot respond if already responded
   - Participant created when both approved
   - Emails sent on approval/rejection

4. **Participants:**
   - List shows all participants
   - Participant access control working
   - Manager can remove participant
   - Removed team can be re-invited

5. **Authorization:**
   - TeamManager role loaded correctly
   - Team manager policy working
   - Combined role scenarios work

## Design Decisions

1. **Team Manager Bootstrap:** NGB Admin should be able to add a team manager. This will be implemented in Phase 5.

2. **Invite Expiration:** No automatic expiration on invites. However, archived tournaments (past end date) should block all changes to invites.

3. **Invite History:** Keep all rejected invites in the database for audit trail.

4. **Re-invitation:** If a participant is removed, they can be re-invited. Removal doesn't block future invites.

5. **Multiple Invites:** Only one pending invite allowed per participant. Use unique index on (TournamentId, ParticipantType, ParticipantId) WHERE status != Rejected.

6. **Participant Limit:** No enforced maximum limit. Tournament managers handle capacity themselves by not approving once their limit is reached.

7. **Withdrawal:** No self-service withdrawal. Only tournament managers can remove participants.

8. **Archived Tournament:** Changes to invites and participants are blocked for archived tournaments (past end date). Tournament details can still be viewed but not modified.

9. **Email Opt-out:** No opt-out mechanism in Phase 3.

10. **Batch Invites:** No batch API. Frontend can call the single-invite endpoint multiple times if needed.
