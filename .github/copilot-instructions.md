# GitHub Copilot Instructions for IQA Management Hub

This document provides comprehensive guidance for coding agents working on the IQA Management Hub repository.

---

## Table of Contents

1. [Before You Begin](#before-you-begin)
2. [High-Level Guidance](#high-level-guidance)
3. [Technology Stack](#technology-stack)
4. [Coding Guidelines](#coding-guidelines)
5. [Building and Running](#building-and-running)
6. [Testing](#testing)
7. [Database Migrations](#database-migrations)
8. [Before You Commit](#before-you-commit)
9. [Specific Coding Patterns](#specific-coding-patterns)
10. [Common Pitfalls](#common-pitfalls)

---

## Before You Begin

### 1. Understand the Project Structure

Full-stack application:
- **Backend**: .NET 8.0, ASP.NET Core, Entity Framework Core
- **Frontend**: React 18, TypeScript, Redux Toolkit, Webpack 5
- **Database**: PostgreSQL (production), in-memory (dev)

**Key directories:**
```
src/backend/
  ├── ManagementHub.Service/      # Main API
  ├── ManagementHub.Models/       # Domain models
  ├── ManagementHub.Storage/      # Data access
  └── ManagementHub.IntegrationTests/
src/frontend/app/
  ├── modules/  # Feature modules
  ├── pages/    # Page components
  └── store/    # Redux store & API client
```

### 2. Read Documentation First

Review before making changes:
- `README.md` - Setup and building
- `docs/building.md`, `docs/testing.md`, `docs/api-client.md`
- `docs/features/` - Feature-specific guides

### 3. Build Projects Before Starting

**Backend:**
```bash
cd src/backend
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false
```

**Frontend:**
```bash
cd src/frontend
yarn install --immutable
yarn build:dev
```

Understanding existing build errors helps avoid fixing unrelated issues.

### 4. User Context

- **Referees**: Take tests, manage profiles
- **NGB Admins**: Manage referees in jurisdiction
- **IQA Admins**: System-wide administration

---

## High-Level Guidance

### Core Principles

1. **Minimal Changes** - Make smallest changes to achieve goal
2. **Follow Patterns** - Look for similar code and follow consistently
3. **Type Safety** - Use strongly-typed identifiers, not raw strings
4. **Security** - Never log sensitive data; validate authorization
5. **Test-Driven** - Write/update tests for changes
6. **Document** - Update docs when making significant changes

### Workflow

Explore → Plan → Implement → Test → Lint → Build → Commit

---

## Technology Stack

**Backend**: .NET 8.0, C# 12, ASP.NET Core, Entity Framework Core 8, PostgreSQL 14+, Redis, Hangfire, xUnit, Swashbuckle

**Frontend**: React 18, TypeScript 5.x, Redux Toolkit, RTK Query, React Router 6, Tailwind CSS 3.x, Webpack 5, Jest

---

## Coding Guidelines

### Backend (C#)

#### Naming Conventions
- **Classes/Interfaces**: PascalCase (`UserService`, `IUserRepository`)
- **Methods**: PascalCase (`GetUserAsync`, `CreateTournament`)
- **Variables/Parameters/Private Fields**: camelCase (`userId`, `dbContext`, `logger`)
- **Constants**: PascalCase (`MaxRetryCount`)

#### Required Properties

Use `required` keyword instead of `= null!`:

```csharp
public class TournamentData
{
    public required string Name { get; init; }
    public required TournamentType Type { get; init; }
}
```

#### Strongly-Typed Identifiers

Use record structs instead of raw strings:

```csharp
public record struct TournamentIdentifier(Ulid UniqueId)
{
    private const string IdPrefix = "TR_";
    public static TournamentIdentifier NewId() => new(Ulid.NewUlid());
    public override string ToString() => $"{IdPrefix}{UniqueId}";
}

// Usage
public async Task<Tournament> GetTournamentAsync(TournamentIdentifier id) { }
```

#### EF Core Query Pattern

**ALWAYS filter/order BEFORE projection** for SQL translation:

```csharp
var tournaments = await dbContext.Tournaments
    .Where(t => t.IsPublic || managedIds.Contains(t.Id))
    .OrderBy(t => t.StartDate)
    .Select(t => new TournamentViewModel { Id = t.UniqueId.ToString(), Name = t.Name })
    .ToListAsync();
```

#### User Queries

**NEVER** compare `User.UniqueId` directly. Use `WithIdentifier`:

```csharp
var userDbId = await WithIdentifier(dbContext.Users, userId)
    .Select(u => u.Id)
    .FirstOrDefaultAsync();
```

#### Authorization

Use policies and requirements:

```csharp
[Authorize(AuthorizationPolicies.TournamentManager)]
public async Task<IActionResult> UpdateTournament(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromBody] UpdateTournamentRequest request) { }
```

#### Logging - NO SENSITIVE DATA

```csharp
// ✅ GOOD
_logger.LogInformation("User login attempt for user ID {UserId}", userId);

// ❌ BAD - logs email
_logger.LogInformation("User login attempt for email {Email}", email);
```

### Frontend (TypeScript/React)

#### Component Structure

Functional components with hooks:

```typescript
interface TournamentCardProps {
  tournamentId: string;
  name: string;
}

export const TournamentCard: FC<TournamentCardProps> = ({ tournamentId, name }) => {
  return <div className="tournament-card"><h3>{name}</h3></div>;
};
```

#### API Calls

Use RTK Query (not legacy Redux):

```typescript
import { useGetTournamentsQuery } from '../store/serviceApi';

export const TournamentList: FC = () => {
  const { data: tournaments, isLoading, error } = useGetTournamentsQuery();
  
  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage error={error} />;
  
  return <div>{tournaments?.map(t => <TournamentCard key={t.id} {...t} />)}</div>;
};
```

#### Type Safety

Use generated types from backend API:

```typescript
import { TournamentViewModel } from '../store/serviceApi';

interface TournamentListProps {
  tournaments: TournamentViewModel[];
}
```

#### Feature Flags

```typescript
import { useFeatureGates } from '../hooks/useFeatureGates';

export const AdminPage: FC = () => {
  const { isTestFlag } = useFeatureGates();
  return <div>{isTestFlag && <NewFeature />}</div>;
};
```

Query override: `?features=isTestFlag` or `?features=!isTestFlag`

#### Navigation

Use custom hook to preserve query parameters:

```typescript
import { useNavigate } from '../utils/navigationUtils';

const navigate = useNavigate();
navigate('/tournaments'); // Preserves ?impersonate and ?features
```

---

## Building and Running

### Backend

**Local development:**
```bash
cd src/backend
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false
```

Properties: `/p:DisableGitVersion=true` (avoids GitVersion in shallow clones), `/p:BuildFrontend=false` (skips frontend)

**Run service:**
```bash
cd src/backend/ManagementHub.Service
dotnet run  # Starts at http://localhost:5000 with Swagger at /swagger
```

### Frontend

**Development build:**
```bash
cd src/frontend
yarn install --immutable
yarn build:dev
```

**Dev server with hot reload:**
```bash
# Terminal 1: Backend
cd src/backend/ManagementHub.Service
dotnet run

# Terminal 2: Frontend
cd src/frontend
yarn start:dev  # Changes auto-reload at http://localhost:5000
```

Note: CSS changes need `yarn styles`

### API Client Regeneration

After backend API changes:
```bash
bash scripts/refresh_swagger.sh
```

Generates RTK Query types in `src/frontend/app/store/serviceApi.ts`

### Docker

```bash
cd src/backend
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```

---

## Testing

### Backend

```bash
cd src/backend
dotnet test                              # All tests
dotnet test ManagementHub.UnitTests      # Unit tests only
dotnet test ManagementHub.IntegrationTests  # Integration tests (use TestContainers)
```

**Integration test example:**
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

### Frontend

```bash
cd src/frontend
yarn test
```

Test non-trivial logic, minimize mocking. Skip legacy Redux tests.

---

## Database Migrations

### Creating Migrations

Run from `ManagementHub.Service` directory:

```bash
cd src/backend/ManagementHub.Service
dotnet ef migrations add AddTournamentTables --project ../ManagementHub.Storage
```

### Applying Migrations

Development (in-memory): Auto-applied on startup

PostgreSQL:
```bash
cd src/backend/ManagementHub.Service
dotnet ef database update
```

### Entity Configuration

Configure in `ManagementHub.Storage/Data/ManagementHubDbContext.cs`:

```csharp
modelBuilder.Entity<Tournament>(entity =>
{
    entity.ToTable("tournaments");
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.UniqueId).IsUnique();
    
    entity.HasOne(d => d.Ngb)
        .WithMany(p => p.Tournaments)  // MUST specify to avoid shadow properties
        .HasForeignKey(d => d.NgbId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

**Key points:**
- Always specify `.WithMany(p => p.NavigationProperty)` to avoid EF shadow properties
- Index foreign keys explicitly
- Review generated `Up()` and `Down()` methods

---

## Before You Commit

1. **Format code:**
   ```bash
   cd src/backend && dotnet format  # Must exit cleanly
   cd src/frontend && yarn lint
   ```

2. **Run tests** (relevant to your changes)

3. **Build successfully:**
   ```bash
   cd src/backend && dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false
   cd src/frontend && yarn build:dev
   ```

4. **Check for sensitive data** in logs (no emails, passwords, PII)

5. **Update documentation** if needed

6. **Validate .gitignore** (exclude `dist/`, `bin/`, `obj/`, `node_modules/`)

7. **Commit message format:**
   ```
   Add tournament management API endpoints
   
   - Implement GET /api/tournaments
   - Add TournamentManager authorization policy
   - Add integration tests
   
   Resolves #451
   ```

---

## Specific Coding Patterns

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentContextProvider tournamentContext;
    private readonly ILogger<TournamentsController> logger;
    
    public TournamentsController(
        ITournamentContextProvider tournamentContext,
        ILogger<TournamentsController> logger)
    {
        this.tournamentContext = tournamentContext;
        this.logger = logger;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TournamentViewModel>>> GetTournaments()
    {
        var tournaments = await tournamentContext.QueryTournaments()
            .Select(t => new TournamentViewModel { Id = t.UniqueId.ToString(), Name = t.Name })
            .ToListAsync();
        return Ok(tournaments);
    }
}
```

### Authorization Requirement

```csharp
public class TournamentManagerRequirement : IAuthorizationRequirement
{
    public TournamentIdentifier? TournamentId { get; init; }
}

public class TournamentManagerHandler : AuthorizationHandler<TournamentManagerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TournamentManagerRequirement requirement)
    {
        var userId = context.User.GetUserId();
        if (userId == null) { context.Fail(); return; }
        
        var isTournamentManager = await CheckTournamentManagerAsync(userId.Value, requirement.TournamentId);
        if (isTournamentManager) context.Succeed(requirement);
    }
}
```

### View Models

```csharp
// Returned to client
public class TournamentViewModel
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}

// Received from client
public class CreateTournamentRequest
{
    public required string Name { get; init; }
    public required TournamentType Type { get; init; }
}
```

---

## Common Pitfalls

### 1. EF Core Shadow Properties

Forgetting `.WithMany()` creates shadow properties:

```csharp
// ❌ BAD - Creates shadow property UserId1
entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId);

// ✅ GOOD
entity.HasOne(d => d.User).WithMany(p => p.TournamentManagers).HasForeignKey(d => d.UserId);
```

### 2. Projection Before Filtering

Projects to ViewModels before filtering runs in-memory:

```csharp
// ✅ GOOD - Filter in SQL
var result = await dbContext.Tournaments
    .Where(t => t.Name.Contains("World"))
    .Select(t => new TournamentViewModel { Name = t.Name })
    .ToListAsync();
```

### 3. Direct User.UniqueId Comparison

Use `WithIdentifier` pattern instead.

### 4. Missing Swagger Regeneration

After backend API changes:
```bash
bash scripts/refresh_swagger.sh
```

### 5. Legacy Redux Usage

Use RTK Query for new code, not legacy Redux actions/reducers.

### 6. Not Preserving Query Parameters

Use custom `useNavigate` from `../utils/navigationUtils`, not `react-router-dom`.

### 7. Forgetting Build Flags

Use `/p:DisableGitVersion=true /p:BuildFrontend=false` for local development.

### 8. Logging Sensitive Data

Never log emails, passwords, or PII.

---

## Additional Resources

- **Swagger UI**: http://localhost:5000/swagger (when running)
- **Documentation**: `docs/` directory
- **Test users (dev)**: See README.md

---

**Remember**: Make minimal changes, follow existing patterns, test thoroughly.
