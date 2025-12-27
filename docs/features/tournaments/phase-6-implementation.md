# Phase 6: List Team Members

## Overview
This phase enables team managers to view a list of users associated with their team:
- Implementing endpoint to list team members (referees with player or coach association)
- Supporting search by name and pagination
- Using existing RefereeTeam table as data source
- Preparing for tournament roster selection in the UI

This addresses the need for team managers to select tournament participants from existing team members.

## Prerequisites
- Phase 5 must be complete:
  - TeamManagerOrNgbAdminPolicy exists
  - Team manager and NGB admin authorization working
  - Team-based authorization functional

## Database Changes

No database changes required. This phase uses the existing `RefereeTeam` table:
- `RefereeTeam.TeamId` - links to team
- `RefereeTeam.RefereeId` - links to user
- `RefereeTeam.AssociationType` - "player" or "coach" (currently ignored)

## API Implementation

### 1. Create View Model
**Location:** `src/backend/ManagementHub.Service/Areas/Teams/` or within Ngbs area

**TeamMemberViewModel.cs:**
```csharp
public class TeamMemberViewModel
{
    public string UserId { get; set; }
    public string Name { get; set; }
}
```

### 2. Extend Team Context Provider
**Location:** Team context provider implementation

Add method to interface:
```csharp
IQueryable<TeamMemberInfo> QueryTeamMembers(TeamIdentifier teamId);
```

**TeamMemberInfo class** - Define in appropriate location:
```csharp
public class TeamMemberInfo
{
    public UserIdentifier UserId { get; set; }
    public string Name { get; set; }  // First + Last name
}
```

**Implementation:**
```csharp
public IQueryable<TeamMemberInfo> QueryTeamMembers(TeamIdentifier teamId)
{
    var teamDbId = context.Teams
        .Where(t => t.TeamId == teamId.ToString())
        .Select(t => t.Id)
        .FirstOrDefault();
    
    if (teamDbId == 0)
    {
        return Enumerable.Empty<TeamMemberInfo>().AsQueryable();
    }
    
    var query = context.RefereeTeams
        .Where(rt => rt.TeamId == teamDbId)
        .Where(rt => rt.RefereeId != null)  // Exclude any null associations
        .Join(
            context.Users,
            rt => rt.RefereeId,
            u => u.Id,
            (rt, u) => new TeamMemberInfo
            {
                UserId = u.UniqueId != null 
                    ? UserIdentifier.Parse(u.UniqueId) 
                    : UserIdentifier.FromLegacyUserId(u.Id),
                Name = $"{u.FirstName} {u.LastName}"
            });
    
    // Apply filtering if present
    var filter = this.filteringContext.FilteringParameters.Filter;
    filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";
    if (!string.IsNullOrEmpty(filter))
    {
        if (context.Database.IsNpgsql())
        {
            query = query.Where(m => EF.Functions.ILike(m.Name, filter));
        }
        else
        {
            query = query.Where(m => EF.Functions.Like(m.Name, filter));
        }
    }
    
    return query.Distinct();  // In case user appears multiple times with different associations
}
```

**Note:** This returns an `IQueryable` to support filtering and pagination via the `Filtered<>` pattern.

### 3. Add Endpoint to Controller
**Location:** `src/backend/ManagementHub.Service/Areas/Ngbs/NgbsController.cs` or separate Teams controller

Add endpoint following NGB subresource pattern:

**GET `/api/v2/Ngbs/{ngb}/teams/{teamId}/members`** - List team members (Team Manager or NGB Admin)

```csharp
/// <summary>
/// List members (referees) associated with a team.
/// </summary>
[HttpGet("{ngb}/teams/{teamId}/members")]
[Tags("Team")]
[Authorize(AuthorizationPolicies.TeamManagerOrNgbAdminPolicy)]
public async Task<Filtered<TeamMemberViewModel>> GetTeamMembers(
    [FromRoute] NgbIdentifier ngb,
    [FromRoute] TeamIdentifier teamId,
    [FromQuery] FilteringParameters filtering)
{
    // Verify team belongs to NGB
    var team = await this.teamContextProvider.GetTeamAsync(teamId);
    if (team == null || !team.NgbId.Equals(ngb))
    {
        return Enumerable.Empty<TeamMemberViewModel>().AsQueryable().AsFiltered();
    }
    
    // Get members as queryable - filtering is applied within QueryTeamMembers
    var membersQuery = this.teamContextProvider.QueryTeamMembers(teamId);
    
    // Convert to view model and apply pagination
    return membersQuery
        .Select(m => new TeamMemberViewModel
        {
            UserId = m.UserId.ToString(),
            Name = m.Name
        })
        .AsFiltered();
}
```

**Pattern reference:** See `GetNgbTeams` in `NgbsController.cs` for the `Filtered<>` pattern usage.

**Authorization note:** The `TeamManagerOrNgbAdminPolicy` handles authorization checks via route parameters:
- Team managers must have TeamManager role for this specific `teamId`
- NGB admins must have NgbAdmin role for this specific `ngb`

## Design Decisions

1. **Association Type Ignored:** For simplicity, the endpoint returns all team members regardless of whether they're players or coaches. This can be extended in the future if needed.

2. **No Email Exposure:** Email addresses are not included in the response for privacy reasons. Only UserIdentifier and name are exposed.

3. **Distinct Members:** If a user appears multiple times in RefereeTeam table with different association types, they appear only once in the results.

4. **Search by Name:** The `filtering.Filter` parameter enables search/filter by user name. Uses PostgreSQL's `ILike` for case-insensitive pattern matching with wildcard support.

5. **NGB Validation:** Endpoint validates that the requested team belongs to the specified NGB before returning data.

6. **Pagination Support:** Uses the standard `Filtered<>` pattern with `FilteringParameters` for consistent pagination across the API.

7. **Empty Results:** Returns empty filtered collection if team doesn't exist or doesn't belong to NGB, rather than 404.

## Testing Checklist

After implementing:

1. **List Team Members:**
   - Team manager can list members for their own team
   - NGB admin can list members for teams in their NGB
   - Returns correct user IDs and names
   - Results include both players and coaches

2. **Search Functionality:**
   - Search by partial name works (case-insensitive)
   - Empty search returns all members
   - Search with no matches returns empty list

3. **Pagination:**
   - Pagination parameters work correctly
   - Page size limits are respected
   - Metadata includes total count

4. **Authorization:**
   - Non-team-managers cannot access the endpoint
   - Team managers can only see their own team's members
   - NGB admin from different NGB cannot access
   - NGB admin can access all teams in their NGB

5. **Validation:**
   - Invalid team ID returns empty results
   - Team from different NGB returns empty results
   - Teams with no members return empty list

6. **Data Integrity:**
   - Duplicate user associations appear only once
   - Users with null RefereeId are excluded
   - Name formatting is consistent (First Last)

## Implementation Notes

1. **Controller Location:** Can be added to existing `NgbsController.cs` since teams are already managed as subresources of NGBs.

2. **Queryable Pattern:** Using `IQueryable` allows the filtering infrastructure to work efficiently with database queries rather than loading all members into memory.

3. **AsFiltered Extension:** The `.AsFiltered()` extension method (from existing codebase) handles pagination and metadata generation automatically.

4. **Search Implementation:** The name search is applied in `QueryTeamMembers` using `EF.Functions.ILike` (PostgreSQL) or `EF.Functions.Like` for database-level filtering. The `FilteringParameters.Filter` value is wrapped with `%` wildcards for partial matching.

5. **Distinct:** The `Distinct()` call uses default equality comparison on `TeamMemberInfo`. Since it's a reference type, this might not deduplicate correctly. Consider implementing `IEquatable<TeamMemberInfo>` or using `DistinctBy(m => m.UserId)` if using .NET 6+.

## Future Enhancements

1. **Association Type Filter:** Add optional query parameter to filter by player/coach association type.

2. **Additional Fields:** Include avatar URL, certification levels, or other relevant user information.

3. **Roster Status:** Indicate which members are already on rosters for specific tournaments.

4. **Sorting:** Support sorting by name or other fields.

5. **Extended User Roles:** When non-referee team members are added in the future, extend the query to include additional user-team relationship tables.

## Open Questions

1. **Performance:** For teams with very large member lists (100+ users), should we add database indexes on RefereeTeam(TeamId, RefereeId)?

2. **Real-time Updates:** Should this endpoint reflect real-time changes to team membership, or is eventual consistency acceptable?

3. **Inactive Users:** Should the endpoint include inactive or deactivated users who are still associated with the team?
