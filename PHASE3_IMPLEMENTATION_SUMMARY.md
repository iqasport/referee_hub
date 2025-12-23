# Phase 3 Implementation - Progress Summary

## Overview
This document summarizes the work completed for Phase 3 of the Tournament Management feature and outlines the remaining implementation tasks.

## Completed Database Schema Changes

### New Tables Created

1. **team_managers**
   - Tracks team manager assignments
   - Columns: id, team_id, user_id, created_at, updated_at
   - Unique constraint on (team_id, user_id)
   - Foreign keys to teams and users tables

2. **tournament_invites**
   - Manages invitation workflow between tournaments and teams
   - Columns: id, tournament_id, participant_type, participant_id, initiator_user_id, created_at
   - Approval tracking: tournament_manager_approval, tournament_manager_approval_date, participant_approval, participant_approval_date
   - Unique constraint on (tournament_id, participant_type, participant_id) WHERE both approvals != rejected
   - Foreign keys to tournaments and users tables

3. **tournament_team_participants**
   - Records tournament participation
   - Columns: id, tournament_id, team_id, team_name, created_at, updated_at
   - Unique constraint on (tournament_id, team_id)
   - Foreign keys to tournaments and teams tables
   - team_name is denormalized for historical records

### Enum Extensions

1. **TeamGroupAffiliation** - Added `National = 4` value
2. **ApprovalStatus** (new) - Pending, Approved, Rejected

## Completed Code Changes

### Models & Domain Objects

1. **TeamConstraint** - Similar to TournamentConstraint, supports Any, Single, or Set of teams
2. **ITeamUserRole** - Interface for team-scoped user roles
3. **TeamManagerRole** - Role implementation with TeamConstraint
4. **TournamentInvite** entity - Database entity for invites
5. **TournamentTeamParticipant** entity - Database entity for participants
6. **TeamManager** entity - Database entity for team managers

### Authorization

1. **TeamUserRoleAuthorizationRequirement** - Validates team permissions from route parameters
2. **TeamManagerPolicy** - Authorization policy for team manager endpoints
3. **Role Loading** - TeamManager roles now loaded in DbUserContext

### View Models

1. **TournamentInviteViewModel** - For returning invite information
2. **ApprovalStatusViewModel** - For approval status details
3. **CreateInviteModel** - For creating new invites
4. **InviteResponseModel** - For approving/rejecting invites
5. **TournamentParticipantViewModel** - For participant lists

### Database Migration

- Migration file: `20251223122500_AddTournamentInvitesAndParticipants.cs`
- Creates all three new tables with proper indexes and constraints

## Remaining Implementation Tasks

### 1. Context Provider Extensions

The `ITournamentContextProvider` and `DbTournamentContextProvider` need these new methods:

```csharp
// Invite management
Task<IEnumerable<InviteInfo>> GetTournamentInvitesAsync(TournamentIdentifier tournamentId, UserIdentifier? filterByParticipant = null);
Task<InviteInfo> CreateInviteAsync(TournamentIdentifier tournamentId, string participantType, string participantId, UserIdentifier initiatorUserId);
Task<InviteInfo> GetInviteByIdAsync(long inviteId);
Task UpdateInviteApprovalAsync(long inviteId, bool isTournamentManager, bool approved);

// Participant management
Task<IEnumerable<ParticipantInfo>> GetTournamentParticipantsAsync(TournamentIdentifier tournamentId);
Task AddParticipantAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId);
Task RemoveParticipantAsync(TournamentIdentifier tournamentId, TeamIdentifier teamId);
```

### 2. Controller Endpoints

Add to `TournamentsController`:

- `GET /api/v2/tournaments/{tournamentId}/invites` - List invites
- `POST /api/v2/tournaments/{tournamentId}/invites` - Create invite
- `POST /api/v2/tournaments/{tournamentId}/invites/{participantId}` - Respond to invite
- `GET /api/v2/tournaments/{tournamentId}/participants` - List participants
- `DELETE /api/v2/tournaments/{tournamentId}/participants/{teamId}` - Remove participant

### 3. Business Logic

Implement:
- **Tournament-Team Type Validation**: Match tournament type with team affiliation
- **Invite Status Computation**: Derive status from both approval fields
- **Auto-Approval Logic**: Auto-approve when user has both tournament manager and team manager roles
- **Archived Tournament Check**: Prevent modifications to archived tournaments

### 4. QueryTournaments Extension

Update `DbTournamentContextProvider.QueryTournaments()` to:
- Include tournaments where user is a team manager of a participating team
- Extend `GetUserInvolvedTournamentIdsAsync` to check team manager participation

### 5. Email Notifications

Create email templates and service for:
- Invite created notification
- Invite approved notification
- Invite rejected notification

### 6. Integration Tests

Add tests for:
- Creating invites (by tournament manager and team manager)
- Approval workflow (both approvals required)
- Auto-approval when user has both roles
- Type validation (club/national/youth/fantasy)
- Participant creation after approval
- Private tournament access for participants
- Removing participants

## Testing Status

### Current Status
✅ All existing tournament tests pass (7/7)
✅ Project builds successfully
✅ Code passes dotnet format checks

### Test Scenarios Needed

1. **Invite Creation**
   - Tournament manager invites team
   - Team manager requests to join
   - Both roles auto-approves
   - Duplicate invite prevention
   - Type compatibility validation

2. **Invite Response**
   - Tournament manager approval
   - Participant approval
   - Rejection by either party
   - Participant creation on double approval

3. **Participants**
   - List participants
   - Remove participant
   - Re-invitation after removal

4. **Authorization**
   - TeamManager policy enforcement
   - Route parameter validation
   - Private tournament access

## Migration Notes

### Running the Migration

```bash
cd src/backend/ManagementHub.Service
dotnet ef database update --project ../ManagementHub.Storage
```

### Rollback

```bash
dotnet ef database update <PreviousMigrationName> --project ../ManagementHub.Storage
```

## Next Steps

1. Implement the context provider methods for invite/participant management
2. Add controller endpoints for invites and participants
3. Implement business logic for validation and auto-approval
4. Update QueryTournaments to include team manager filtering
5. Create email notification templates and service
6. Write comprehensive integration tests
7. Run swagger refresh to update frontend types
8. Manual testing of the complete workflow

## Dependencies

The following Phase 3 items depend on code not yet implemented:

- Email notifications require email service infrastructure
- Some validation logic may require access to Team context provider
- Frontend integration will require swagger refresh after API completion

## Architecture Notes

### Why TeamManager Uses Numeric TeamIdentifier

Unlike TournamentIdentifier which uses ULID, TeamIdentifier uses a numeric long Id. This is because the Team table doesn't have a unique_id column - it uses the database-assigned Id as the primary identifier. The TeamIdentifier pattern wraps this numeric Id with the "TM_" prefix for API serialization.

### Invite Status Derivation

The invite status is computed rather than stored:
- **Rejected**: If either approval is Rejected
- **Approved**: If both approvals are Approved
- **Pending**: Otherwise

This allows re-invitation after rejection while maintaining audit trail.

### Denormalized Team Name

The tournament_team_participants table stores team_name as a denormalized field because team names can change over time, but tournament historical records should preserve the name at the time of participation.

## References

- Phase 3 Specification: `docs/features/tournaments/phase-3-implementation.md`
- TournamentConstraint pattern: `ManagementHub.Models/Domain/Tournament/TournamentConstraint.cs`
- TournamentManager pattern: `ManagementHub.Storage/Data/TournamentManager.cs`
- Authorization patterns: `ManagementHub.Service/Authorization/`
