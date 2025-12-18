# Tournament Management Implementation - Overview

This directory contains detailed implementation guidelines for the tournament management feature, broken down into five deliverable phases.

## Phase Documents

1. **[Phase 1: Get and Create Tournament, New Tournament Manager Role](./phase-1-implementation.md)**
   - Foundation: Tournament entities, IDs, database tables
   - TournamentManager role and authorization
   - Basic CRUD operations (list, get, create)
   - Banner image upload

2. **[Phase 2: Manage Tournament Managers](./phase-2-implementation.md)**
   - Add/remove tournament managers
   - Manager listing with email addresses
   - Multi-manager support

3. **[Phase 3: Manage Invites and Create Participants](./phase-3-implementation.md)**
   - Tournament invitation/join request system
   - Team-tournament type compatibility validation
   - Approval workflow (manager & participant)
   - TeamManager role and authorization
   - Participant management
   - Email notifications
   - Extended TeamGroupAffiliation enum

4. **[Phase 4: Manage Team Rosters](./phase-4-implementation.md)**
   - Roster management (players, coaches, staff)
   - Player-specific data (jersey numbers)
   - Sensitive gender data handling
   - Privacy controls and scheduled deletion
   - Archived tournament restrictions

5. **[Phase 5: Team Manager Bootstrap (NGB Admin Access)](./phase-5-implementation.md)**
   - NGB Admins can add/remove team managers
   - Solves bootstrap problem for initial team setup
   - Team manager listing endpoints
   - NGB jurisdiction validation
   - Combined authorization policy for TeamManager or NgbAdmin

6. **[Phase 6: List Team Members](./phase-6-implementation.md)**
   - Team members endpoint for roster selection
   - Search by name and pagination support
   - Returns users associated with team (players/coaches)
   - Team manager and NGB admin access

## How to Use These Guidelines

Each phase document includes:

- **Overview**: What the phase accomplishes
- **Prerequisites**: What must be complete before starting
- **Database Changes**: Entity creation, migrations, relationships
- **Authorization**: Roles, policies, requirements
- **API Implementation**: Endpoints, view models, business logic
- **Code References**: Existing patterns to follow
- **Testing Checklist**: What to verify after implementation
- **Open Questions**: Decisions needed or clarifications required

### For Implementers

1. **Read the spec**: Review [requirements.md](./requirements.md) to understand overall requirements
2. **Follow sequentially**: Complete Phase 1 before 2, Phase 2 before 3, etc.
3. **Reference patterns**: Use the file paths and pattern references provided
4. **Keep code snippets minimal**: The guidelines show patterns but expect you to write full implementations
5. **Address open questions**: Discuss unclear items with stakeholders before proceeding
6. **Test thoroughly**: Use the testing checklists to verify completeness

### Code Organization

The implementation touches these main areas:

```
src/backend/
├── ManagementHub.Models/
│   ├── Domain/
│   │   ├── Tournament/        # TournamentIdentifier, TournamentData, TournamentConstraint
│   │   ├── Team/              # TeamConstraint
│   │   └── User/Roles/        # TournamentManagerRole, TeamManagerRole
│   ├── Enums/                 # TournamentType, ApprovalStatus, RosterRole
│   └── Abstraction/           # Interfaces for services and contexts
├── ManagementHub.Storage/
│   ├── Data/                  # Entity classes (Tournament, TournamentManager, etc.)
│   ├── Contexts/              # Context providers
│   ├── Commands/              # Command implementations
│   ├── Services/              # Services (gender data, etc.)
│   └── Migrations/            # EF migrations
├── ManagementHub.Service/
│   ├── Areas/Tournaments/     # Controller and view models
│   └── Authorization/         # Authorization requirements and policies
└── ManagementHub.Mailers/
    └── Templates/             # Email templates
```

## Key Design Decisions

### Extensibility
- **ParticipantType**: Designed as string to support future individual participants
- **TournamentConstraint/TeamConstraint**: Support single, multiple, or all entities
- **Roles**: Parameterized by constraints for flexibility

### Privacy
- **Gender Data**: Separate table with audit trail for scheduled deletion
- **Access Control**: Gender visible only to authorized users (tournament managers, team managers, self)
- **Private Tournaments**: Visibility restricted to managers and participants

### Authorization
- **TournamentManagerRole**: Required for tournament management operations
- **TeamManagerRole**: Required for team roster management
- **Combined Roles**: Auto-approve invites when user has both roles

### Data Integrity
- **Unique Constraints**: Prevent duplicate managers, participants, roster entries
- **Foreign Keys**: Ensure referential integrity
- **Archived Tournaments**: Restrict modifications based on end date

## Migration Strategy

### Database Migrations
Each phase includes one or more EF migrations. Apply them sequentially:

```powershell
cd src/backend/ManagementHub.Service
dotnet ef migrations add <MigrationName> --project ../ManagementHub.Storage
dotnet ef database update
```

### Rollback Considerations
- Phase 1: Can rollback independently
- Phase 2: Depends on Phase 1 tables
- Phase 3: Depends on Phase 1 & 2, adds invite system
- Phase 4: Depends on Phase 3, adds roster tables

Plan rollback strategy for each phase before deployment.

## Common Patterns Used

### Entity Pattern
```csharp
public class EntityName
{
    public long Id { get; set; }           // Auto-increment PK
    public Ulid UniqueId { get; set; }     // If using ULID
    // ... properties
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual RelatedEntity Related { get; set; }
}
```

### Identifier Pattern
```csharp
public record struct EntityIdentifier(Ulid UniqueId)
{
    private const string IdPrefix = "XX_";
    public static EntityIdentifier NewId() => new(Ulid.NewUlid());
    public static bool TryParse(string value, out EntityIdentifier result) { ... }
    public override string ToString() => $"{IdPrefix}{UniqueId}";
}
```

### Authorization Pattern
```csharp
[Authorize(AuthorizationPolicies.SpecificPolicy)]
public async Task<IActionResult> ProtectedEndpoint([FromRoute] EntityIdentifier id)
{
    // Authorization handled by policy
    // Additional checks if needed
}
```

### Context Provider Pattern
```csharp
public interface IEntityContextProvider
{
    IQueryable<EntityContext> QueryEntities();
    Task<EntityContext> GetEntityContextAsync(EntityIdentifier id);
    Task<EntityIdentifier> CreateEntityAsync(EntityData data);
}
```

## Testing Strategy

### Unit Tests
Each phase should include unit tests for:
- Business logic (validation, status computation)
- Authorization requirements
- Context provider methods

### Integration Tests
Test API endpoints with:
- Happy path scenarios
- Error cases (validation, authorization)
- Edge cases (concurrent updates, boundary conditions)

### Security Tests
Verify:
- Authorization policies enforced
- Private data access controlled
- Gender data properly secured

## Performance Considerations

### Database Indexes
Ensure indexes on:
- Foreign keys for joins
- UniqueId fields for lookups
- Composite unique constraints
- Query filter fields (TournamentId, TeamId, UserId)

### Query Optimization
- Use `Include()` for eager loading navigation properties
- Batch gender data queries (don't query per player)
- Consider caching for frequently accessed data
- Use pagination for list endpoints

### Concurrent Access
- Consider optimistic concurrency for updates
- Handle race conditions in invite approval
- Test concurrent roster updates

## Deployment Checklist

Before deploying each phase:

- [ ] Database migrations reviewed and tested
- [ ] All endpoints tested (manual or automated)
- [ ] Authorization policies working correctly
- [ ] Email templates created and tested (Phase 3+)
- [ ] Open questions resolved
- [ ] Documentation updated
- [ ] Performance tested with realistic data volumes
- [ ] Security review completed
- [ ] Rollback plan prepared

## Support and Questions

When implementing, if you encounter:
- **Pattern Confusion**: Reference the existing code files mentioned
- **Unclear Requirements**: Review open questions in each phase doc
- **Design Decisions**: Consult with team lead or architect
- **Technical Issues**: Check existing similar implementations in codebase

## Next Steps

1. Review all phase documents
2. Resolve open questions with stakeholders
3. Set up development environment
4. Begin Phase 1 implementation
5. Follow testing checklist for each phase
6. Deploy phases incrementally

Good luck with the implementation!
