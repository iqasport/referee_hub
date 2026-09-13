# GitHub Copilot Instructions for IQA Management Hub

Comprehensive guidance for coding agents working on this repository.

---

## Before You Begin

### Project Structure

Full-stack application:
- **Backend**: .NET 8.0, ASP.NET Core, Entity Framework Core, PostgreSQL
- **Frontend**: React 18, TypeScript, Redux Toolkit, RTK Query, Webpack 5

**Key directories:**
```
src/backend/
  ├── ManagementHub.Service/      # Main API
  ├── ManagementHub.Models/       # Domain models
  ├── ManagementHub.Storage/      # Data access & EF Core
  └── ManagementHub.IntegrationTests/
src/frontend/app/
  ├── modules/  # Feature modules
  ├── pages/    # Page components
  └── store/    # Redux store & RTK Query API client
```

### Documentation

Review before making changes:
- `README.md` - Setup and building
- `docs/` - Building, testing, API client, features

### Build First

Build projects to understand existing errors before starting work:

```bash
# Backend (use these flags for local dev to avoid GitVersion/frontend build issues)
cd src/backend
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false

# Frontend
cd src/frontend
yarn install --immutable
yarn build:dev
```

### User Roles

- **Referees**: Take tests, manage profiles
- **NGB Admins**: Manage referees in jurisdiction
- **IQA Admins**: System-wide administration

---

## Core Principles

1. **Minimal Changes** - Make smallest changes to achieve goal
2. **Follow Patterns** - Look for similar code and follow consistently
3. **Type Safety** - Use strongly-typed identifiers, not raw strings
4. **Security** - Never log sensitive data (emails, passwords, PII); validate authorization
5. **Test-Driven** - Write/update tests for changes
6. **Document** - Update docs when making significant changes

**Workflow**: Explore → Plan → Implement → Test → Lint → Build → Commit

---

## Skills

The following skills provide focused checklists for common review patterns. Read the relevant skill when triggered.

| Skill | Trigger |
|-------|---------|
| `.github/skills/database-migration-checker/` | Modifying entities, adding tables/relationships |
| `.github/skills/test-coverage-checker/` | New features, API endpoints, business logic |
| `.github/skills/codefactor-compliance/` | Before committing any code change |
| `.github/skills/sensitive-data-checker/` | Adding logging, error handling |
| `.github/skills/typed-identifier/` | Creating new entity types, API contracts |
| `.github/skills/context-provider/` | Adding query methods, context factories |
| `.github/skills/command-pattern/` | Adding commands, CQRS handlers |
| `.github/skills/authorization-policy/` | Adding endpoints, role checks |
| `.github/skills/collection-filtering/` | Adding collection/list endpoints |
| `.github/skills/swagger-to-rtk/` | API changes, frontend API client updates |
| `.github/skills/multipart-upload/` | File upload endpoints |
| `.github/skills/query-extension/` | Adding EF queries, NGB-scoped filtering |

---

## Technology Stack

**Backend**: .NET 8.0, C# 12, ASP.NET Core, Entity Framework Core 8, PostgreSQL, Redis, Hangfire, xUnit

**Frontend**: React 18, TypeScript 5.x, Redux Toolkit, RTK Query, React Router 6, Tailwind CSS, Webpack 5, Jest

---

## Coding Guidelines

### Backend (C#)

**Naming Conventions:**
- Classes/Interfaces: PascalCase (`UserService`, `IUserRepository`)
- Methods: PascalCase (`GetUserAsync`, `CreateTournament`)
- Variables/Parameters/Private Fields: camelCase (`userId`, `dbContext`, `logger`)
- Constants: PascalCase (`MaxRetryCount`)

**Required Properties** - Use `required` keyword:
```csharp
public class TournamentData
{
    public required string Name { get; init; }
    public required TournamentType Type { get; init; }
}
```

**Strongly-Typed Identifiers** - Use record structs with ULID:
```csharp
public record struct TournamentIdentifier(Ulid UniqueId)
{
    private const string IdPrefix = "TR_";
    public static TournamentIdentifier NewId() => new(Ulid.NewUlid());
    public override string ToString() => $"{IdPrefix}{UniqueId}";
}
```

**EF Core Queries** - Filter/order BEFORE projection for SQL translation:
```csharp
var tournaments = await dbContext.Tournaments
    .Where(t => t.IsPublic || managedIds.Contains(t.Id))
    .OrderBy(t => t.StartDate)
    .Select(t => new TournamentViewModel { Id = t.UniqueId.ToString(), Name = t.Name })
    .ToListAsync();
```

**User Queries** - Use `WithIdentifier` pattern (NEVER compare `User.UniqueId` directly):
```csharp
var userDbId = await WithIdentifier(dbContext.Users, userId)
    .Select(u => u.Id)
    .FirstOrDefaultAsync();
```

**Authorization** - Use policies:
```csharp
[Authorize(AuthorizationPolicies.TournamentManager)]
public async Task<IActionResult> UpdateTournament(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromBody] UpdateTournamentRequest request) { }
```

**Logging** - NO sensitive data (emails, passwords, PII). Pass typed identifiers directly — they have safe `ToString()`. Full rules → `.github/skills/sensitive-data-checker/SKILL.md`

**Entity Configuration** - Always specify `.WithMany()` to avoid EF shadow properties:
```csharp
modelBuilder.Entity<Tournament>(entity =>
{
    entity.HasOne(d => d.Ngb)
        .WithMany(p => p.Tournaments)  // Required to avoid shadow properties
        .HasForeignKey(d => d.NgbId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

### Frontend (TypeScript/React)

**Components** - Functional with hooks:
```typescript
interface TournamentCardProps {
  tournamentId: string;
  name: string;
}

export const TournamentCard: FC<TournamentCardProps> = ({ tournamentId, name }) => {
  return <div className="tournament-card"><h3>{name}</h3></div>;
};
```

**API Calls** - Use RTK Query (not legacy Redux):
```typescript
import { useGetTournamentsQuery } from '../store/serviceApi';

export const TournamentList: FC = () => {
  const { data: tournaments, isLoading, error } = useGetTournamentsQuery();
  
  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage error={error} />;
  
  return <div>{tournaments?.map(t => <TournamentCard key={t.id} {...t} />)}</div>;
};
```

**Type Safety** - Use generated types:
```typescript
import { TournamentViewModel } from '../store/serviceApi';
```

**Feature Flags** - Query override: `?features=isTestFlag` or `?features=!isTestFlag`
```typescript
import { useFeatureGates } from '../hooks/useFeatureGates';

const { isTestFlag } = useFeatureGates();
```

**Navigation** - Use custom hook to preserve query parameters (`?impersonate`, `?features`):
```typescript
import { useNavigate } from '../utils/navigationUtils';
```

---

## Testing

**Backend:**
```bash
cd src/backend
dotnet test                              # All tests
dotnet test ManagementHub.UnitTests      # Unit tests only
dotnet test ManagementHub.IntegrationTests  # Integration tests (use TestContainers)
```

Integration test example - test with auth, validate responses:
```csharp
[Fact]
public async Task CreateTournament_WithValidData_ReturnsTournament()
{
    var client = _factory.CreateClient();
    var token = await GetAuthTokenAsync(UserRole.IqaAdmin);
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    
    var request = new CreateTournamentRequest { Name = "World Cup 2024" };
    var response = await client.PostAsJsonAsync("/api/tournaments", request);
    
    response.Should().BeSuccessful();
    var tournament = await response.Content.ReadFromJsonAsync<TournamentViewModel>();
    tournament!.Name.Should().Be("World Cup 2024");
}
```

**Frontend:**
```bash
cd src/frontend
yarn test
```

Test non-trivial logic, minimize mocking. Skip legacy Redux tests.

---

## Before You Commit

1. **Format & Lint:**
   ```bash
   cd src/backend && dotnet format
   cd src/frontend && yarn lint
   ```

2. **Check for BOM characters** (CodeFactor auto-fixer flagging these):
   ```bash
   file src/backend/**/*.cs | grep -i "with BOM" || echo "No BOM found"
   ```

3. **Run relevant tests**

4. **Check for sensitive data** in logs → `.github/skills/sensitive-data-checker/SKILL.md`
5. **Verify database migrations** if entities changed → `.github/skills/database-migration-checker/SKILL.md`
6. **Verify test coverage** for new features/APIs → `.github/skills/test-coverage-checker/SKILL.md`

7. **Update documentation** if needed

8. **Regenerate API client** after backend API changes:
   ```bash
   bash scripts/refresh_swagger.sh
   ```

---

**Remember**: Make minimal changes, follow existing patterns, test thoroughly.
