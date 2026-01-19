# GitHub Copilot Instructions for IQA Management Hub

This document provides comprehensive guidance for coding agents working on the IQA Management Hub repository. These instructions help ensure consistency, quality, and alignment with project patterns and conventions.

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

The IQA Management Hub is a full-stack application with:
- **Backend**: .NET 8.0 / C# / ASP.NET Core / Entity Framework Core
- **Frontend**: React 18 / TypeScript / Redux Toolkit / Webpack 5
- **Database**: PostgreSQL (production), in-memory (development)
- **Infrastructure**: Docker, GitHub Actions CI/CD, AWS deployment

**Directory Structure:**
```
/home/runner/work/referee_hub/referee_hub/
├── src/
│   ├── backend/          # .NET solution
│   │   ├── ManagementHub.Service/      # Main API service
│   │   ├── ManagementHub.Models/       # Domain models
│   │   ├── ManagementHub.Storage/      # Data access & EF Core
│   │   ├── ManagementHub.Processing/   # Business logic
│   │   ├── ManagementHub.Mailers/      # Email services
│   │   ├── ManagementHub.UnitTests/    # Unit tests
│   │   └── ManagementHub.IntegrationTests/ # Integration tests
│   └── frontend/         # React application
│       ├── app/
│       │   ├── modules/  # Feature modules
│       │   ├── pages/    # Page components
│       │   ├── components/ # Reusable components
│       │   ├── apis/     # Generated API client (RTK Query)
│       │   └── store/    # Redux store
│       └── dist/         # Build output
├── docs/                 # Documentation
├── docker/               # Docker configurations
└── scripts/              # Build/utility scripts
```

### 2. Read Existing Documentation

**ALWAYS** review relevant documentation before making changes:
- `README.md` - Project overview, setup, and building instructions
- `docs/building.md` - Detailed build instructions
- `docs/testing.md` - Testing guidelines
- `docs/api-client.md` - API client generation (RTK Query)
- `docs/features/` - Feature-specific documentation
- `docs/features/tournaments/README.md` - Tournament implementation guide
- `docs/features/FeatureGates.md` - Feature flag system

### 3. Build and Restore Projects First

Before starting work, ensure the project builds successfully:

**Backend:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet restore
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false
```

**Frontend:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn install --immutable
yarn build:dev
```

**Why this matters:** Understanding existing build errors helps you avoid fixing unrelated issues. You're only responsible for issues related to your changes.

### 4. Understand the User Context

This project serves three main user groups:
- **Referees**: Take certification tests, manage profiles
- **NGB Admins**: Manage referees in their jurisdiction
- **IQA Admins**: System-wide administration

Always consider authorization and data visibility when implementing features.

---

## High-Level Guidance

### Core Principles

1. **Minimal Changes**: Make the smallest possible changes to achieve your goal. Don't refactor unrelated code.

2. **Follow Existing Patterns**: Look for similar implementations in the codebase and follow those patterns consistently.

3. **Type Safety**: Use strongly-typed identifiers (e.g., `TournamentIdentifier`, `UserIdentifier`) instead of raw strings or GUIDs.

4. **Security First**: Never log sensitive data (emails, passwords, personal information). Always validate authorization.

5. **Test-Driven**: Write or update tests for your changes. Integration tests are preferred for API endpoints.

6. **Documentation**: Update documentation when making significant changes, especially API changes or new features.

### Development Workflow

1. **Explore** → Understand the existing code and patterns
2. **Plan** → Outline minimal changes needed
3. **Implement** → Make focused changes following patterns
4. **Test** → Run relevant tests (don't run full suite initially)
5. **Lint/Format** → Run linters and formatters
6. **Build** → Ensure project builds successfully
7. **Validate** → Manually test if applicable
8. **Commit** → Use descriptive commit messages

---

## Technology Stack

### Backend (.NET 8.0)

- **Language**: C# 12
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 14+ (production), in-memory (dev)
- **Caching**: Redis (production), in-memory (dev)
- **Background Jobs**: Hangfire
- **Testing**: xUnit, FluentAssertions
- **API Docs**: Swashbuckle (Swagger/OpenAPI)
- **Payments**: Stripe.NET
- **Email**: Custom mailer service

### Frontend (React 18)

- **Language**: TypeScript 5.x
- **Framework**: React 18
- **State Management**: Redux Toolkit, RTK Query
- **Routing**: React Router 6
- **Styling**: Tailwind CSS 3.x, PostCSS
- **Build Tool**: Webpack 5
- **Testing**: Jest, React Testing Library
- **HTTP Client**: Axios (legacy), RTK Query (preferred)

---

## Coding Guidelines

### Backend (C#) Guidelines

#### 1. Naming Conventions

- **Classes/Interfaces**: PascalCase (`UserService`, `IUserRepository`)
- **Methods**: PascalCase (`GetUserAsync`, `CreateTournament`)
- **Variables/Parameters**: camelCase (`userId`, `tournamentData`)
- **Private Fields**: camelCase with underscore prefix (`_dbContext`, `_logger`)
- **Constants**: PascalCase (`MaxRetryCount`)

#### 2. Required Properties

Use the `required` keyword for required properties instead of `= null!`:

```csharp
// ✅ GOOD
public class TournamentData
{
    public required string Name { get; init; }
    public required TournamentType Type { get; init; }
}

// ❌ BAD
public class TournamentData
{
    public string Name { get; init; } = null!;
    public TournamentType Type { get; init; } = null!;
}
```

#### 3. Strongly-Typed Identifiers

Always use strongly-typed identifiers (record structs) instead of raw strings or GUIDs:

```csharp
// ✅ GOOD
public record struct TournamentIdentifier(Ulid UniqueId)
{
    private const string IdPrefix = "T_";
    public static TournamentIdentifier NewId() => new(Ulid.NewUlid());
    public override string ToString() => $"{IdPrefix}{UniqueId}";
}

// Usage
public async Task<Tournament> GetTournamentAsync(TournamentIdentifier id)
{
    // ...
}

// ❌ BAD
public async Task<Tournament> GetTournamentAsync(string id)
{
    // ...
}
```

#### 4. Entity Framework Query Patterns

**ALWAYS filter/order BEFORE projection** to ensure EF Core can translate to SQL:

```csharp
// ✅ GOOD - Filter then project
var tournaments = await dbContext.Tournaments
    .Where(t => t.IsPublic || managedTournamentIds.Contains(t.Id))
    .OrderBy(t => t.StartDate)
    .Select(t => new TournamentViewModel
    {
        Id = t.UniqueId.ToString(),
        Name = t.Name
    })
    .ToListAsync();

// ❌ BAD - Project then filter (won't translate to SQL)
var tournaments = await dbContext.Tournaments
    .Select(t => new TournamentViewModel
    {
        Id = t.UniqueId.ToString(),
        Name = t.Name,
        IsPublic = t.IsPublic
    })
    .Where(t => t.IsPublic) // This runs in memory!
    .ToListAsync();
```

#### 5. User Queries with WithIdentifier Pattern

**NEVER** compare `User.UniqueId` directly. Always use the `WithIdentifier` extension pattern:

```csharp
// ✅ GOOD - Use WithIdentifier
var userDbId = await WithIdentifier(dbContext.Users, userId)
    .Select(u => u.Id)
    .FirstOrDefaultAsync();

// ❌ BAD - Direct UniqueId comparison
var user = await dbContext.Users
    .Where(u => u.UniqueId == userId.UniqueId)
    .FirstOrDefaultAsync();
```

#### 6. Authorization

Use authorization policies and requirements:

```csharp
// Controller
[Authorize(AuthorizationPolicies.TournamentManager)]
public async Task<IActionResult> UpdateTournament(
    [FromRoute] TournamentIdentifier tournamentId,
    [FromBody] UpdateTournamentRequest request)
{
    // Authorization is enforced by policy
    // ...
}

// Policy definition (in Startup/Program.cs)
options.AddPolicy(
    AuthorizationPolicies.TournamentManager,
    policy => policy.Requirements.Add(new TournamentManagerRequirement()));
```

#### 7. Logging - NO SENSITIVE DATA

**NEVER** log sensitive information:

```csharp
// ✅ GOOD
_logger.LogInformation("User login attempt for user ID {UserId}", userId);

// ❌ BAD - Logs email address
_logger.LogInformation("User login attempt for email {Email}", email);

// ❌ BAD - Logs personal data
_logger.LogError("Failed to update user {UserData}", JsonSerializer.Serialize(userData));
```

#### 8. Async/Await

- Use `async`/`await` for I/O operations
- Suffix async methods with `Async`
- Use `ConfigureAwait(false)` in library code (not in controllers)

```csharp
// ✅ GOOD
public async Task<Tournament> GetTournamentAsync(TournamentIdentifier id)
{
    return await _dbContext.Tournaments
        .FirstOrDefaultAsync(t => t.UniqueId == id.UniqueId);
}
```

#### 9. Nullable Reference Types

The project uses nullable reference types. Be explicit:

```csharp
// ✅ GOOD
public string? OptionalField { get; set; }  // Nullable
public string RequiredField { get; set; }   // Non-nullable

// ❌ BAD - Ambiguous
public string SomeField { get; set; }  // Is this nullable?
```

### Frontend (TypeScript/React) Guidelines

#### 1. Component Structure

Prefer functional components with hooks:

```typescript
// ✅ GOOD
import { FC } from 'react';

interface TournamentCardProps {
  tournamentId: string;
  name: string;
}

export const TournamentCard: FC<TournamentCardProps> = ({ tournamentId, name }) => {
  return (
    <div className="tournament-card">
      <h3>{name}</h3>
    </div>
  );
};
```

#### 2. API Calls - Use RTK Query

**Prefer RTK Query** over legacy Redux or direct Axios calls:

```typescript
// ✅ GOOD - RTK Query
import { useGetTournamentsQuery } from '../store/serviceApi';

export const TournamentList: FC = () => {
  const { data: tournaments, isLoading, error } = useGetTournamentsQuery();
  
  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage error={error} />;
  
  return (
    <div>
      {tournaments?.map(t => <TournamentCard key={t.id} {...t} />)}
    </div>
  );
};

// ❌ BAD - Legacy Redux
// Don't use this pattern for new code
import { fetchTournaments } from '../store/tournaments/actions';
```

#### 3. Type Safety

Use TypeScript types generated from the backend API:

```typescript
// ✅ GOOD - Generated types from API
import { TournamentViewModel } from '../store/serviceApi';

interface TournamentListProps {
  tournaments: TournamentViewModel[];
}

// ❌ BAD - Manual any types
interface TournamentListProps {
  tournaments: any[];
}
```

#### 4. State Management

- Use RTK Query for server state (API data)
- Use Redux Toolkit slices for client state only
- Use React hooks (`useState`, `useContext`) for local component state

```typescript
// ✅ GOOD - Server state with RTK Query
const { data } = useGetUserQuery();

// ✅ GOOD - Local state with useState
const [isOpen, setIsOpen] = useState(false);

// ⚠️ Legacy - Avoid for new code
// Don't use legacy Redux actions/reducers
```

#### 5. Feature Flags

Use the `useFeatureGates` hook for feature gating:

```typescript
import { useFeatureGates } from '../hooks/useFeatureGates';

export const AdminPage: FC = () => {
  const { isTestFlag } = useFeatureGates();
  
  return (
    <div>
      <h1>Admin</h1>
      {isTestFlag && <NewFeature />}
    </div>
  );
};
```

Query parameter override: `?features=isTestFlag` or `?features=!isTestFlag`

#### 6. Styling with Tailwind

Use Tailwind CSS utility classes:

```typescript
// ✅ GOOD
<div className="flex items-center justify-between p-4 bg-white rounded-lg shadow">
  <h2 className="text-xl font-bold">Title</h2>
</div>

// Avoid inline styles unless dynamic
```

#### 7. Navigation

Use the custom `useNavigate` hook to preserve query parameters:

```typescript
import { useNavigate } from '../utils/navigationUtils';

export const SomeComponent: FC = () => {
  const navigate = useNavigate();
  
  const handleClick = () => {
    navigate('/tournaments'); // Preserves ?impersonate and ?features
  };
  
  return <button onClick={handleClick}>Go to Tournaments</button>;
};
```

---

## Building and Running

### Backend Build

**Standard build (in CI/CD):**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet build
```

**Local development build (recommended):**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false
```

**Why the properties?**
- `/p:DisableGitVersion=true` - Disables GitVersion (fails in shallow clones)
- `/p:BuildFrontend=false` - Skips frontend build during backend compilation

**Running the backend:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend/ManagementHub.Service
dotnet run
```

The service starts at `http://localhost:5000` with:
- In-memory database (auto-seeded)
- In-memory cache
- Debug email output to console
- Swagger UI at `/swagger`

### Frontend Build

**Development build:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn install --immutable
yarn build:dev
```

**Production build:**
```bash
yarn build:prod
```

**Development server with hot reload:**
```bash
# Terminal 1: Start backend
cd /home/runner/work/referee_hub/referee_hub/src/backend/ManagementHub.Service
dotnet run

# Terminal 2: Start frontend dev server
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn start:dev
```

Open `http://localhost:5000` - changes to TypeScript/React files auto-reload.

**Note:** CSS changes require manual rebuild with `yarn styles`.

### API Client Regeneration

After backend API changes, regenerate the frontend TypeScript client:

```bash
# Option 1: Use the script (recommended)
cd /home/runner/work/referee_hub/referee_hub
bash scripts/refresh_swagger.sh

# Option 2: Manual
# Start backend service first, then:
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn swaggergen
```

This generates RTK Query API definitions in `src/frontend/app/store/serviceApi.ts`.

### Docker Build

**Build Docker image:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```

Image tagged as: `iqasport/management-hub:latest`

---

## Testing

### Backend Tests

**Run all unit tests:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet test
```

**Run specific test project:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet test ManagementHub.UnitTests
```

**Run integration tests:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet test ManagementHub.IntegrationTests
```

**Integration tests** use TestContainers for PostgreSQL and run against a real database.

### Frontend Tests

**Run tests with coverage:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn test
```

**Test guidelines:**
- Focus on testing non-trivial logic
- Mock minimally - prefer testing real behavior
- For RTK Query: mock API responses using MSW or RTK Query test utilities
- Skip legacy Redux tests - that code is being deprecated

### Writing Good Tests

**Backend - Integration Test Example:**
```csharp
[Fact]
public async Task CreateTournament_WithValidData_ReturnsTournament()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = await GetAuthTokenAsync(UserRole.IqaAdmin);
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    
    var request = new CreateTournamentRequest
    {
        Name = "World Cup 2024",
        Type = TournamentType.WorldCup
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/tournaments", request);
    
    // Assert
    response.Should().BeSuccessful();
    var tournament = await response.Content.ReadFromJsonAsync<TournamentViewModel>();
    tournament.Should().NotBeNull();
    tournament!.Name.Should().Be("World Cup 2024");
}
```

**Frontend - Component Test Example:**
```typescript
import { render, screen } from '@testing-library/react';
import { TournamentCard } from './TournamentCard';

describe('TournamentCard', () => {
  it('renders tournament name', () => {
    render(<TournamentCard tournamentId="T_123" name="World Cup 2024" />);
    
    expect(screen.getByText('World Cup 2024')).toBeInTheDocument();
  });
});
```

---

## Database Migrations

### Creating Migrations

**Always run from the `ManagementHub.Service` directory:**

```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend/ManagementHub.Service
dotnet ef migrations add <MigrationName> --project ../ManagementHub.Storage
```

Examples:
```bash
dotnet ef migrations add AddTournamentTables --project ../ManagementHub.Storage
dotnet ef migrations add AddTournamentManagerRole --project ../ManagementHub.Storage
```

### Applying Migrations

**Development (in-memory database):**
Migrations are auto-applied on startup when using in-memory database.

**PostgreSQL:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend/ManagementHub.Service
dotnet ef database update
```

### Migration Guidelines

1. **One migration per logical change** - Don't bundle unrelated changes
2. **Descriptive names** - `AddTournamentTables`, not `Migration1`
3. **Test rollback** - Ensure migrations can be reverted
4. **Index foreign keys** - EF Core doesn't auto-index FKs
5. **Review generated SQL** - Check `Up()` and `Down()` methods

### Entity Configuration Pattern

Configure entities in `ManagementHub.Storage/Data/ManagementHubDbContext.cs`:

```csharp
modelBuilder.Entity<Tournament>(entity =>
{
    entity.ToTable("tournaments");
    entity.HasKey(e => e.Id);
    
    entity.Property(e => e.UniqueId)
        .IsRequired();
    
    entity.HasIndex(e => e.UniqueId)
        .IsUnique();
    
    entity.HasOne(d => d.Ngb)
        .WithMany(p => p.Tournaments)
        .HasForeignKey(d => d.NgbId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

**Common pitfalls:**
- Forgetting `.WithMany()` navigation property → causes EF shadow properties
- Not specifying `OnDelete` behavior → defaults may cause issues
- Missing indexes on foreign keys → poor query performance

---

## Before You Commit

### 1. Format Code

**Backend:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet format
```

Must exit cleanly (exit code 0).

**Frontend:**
```bash
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn lint
```

### 2. Run Tests

Run tests relevant to your changes (don't need to run full suite initially):

```bash
# Backend
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet test

# Frontend
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn test
```

### 3. Build Successfully

Ensure the project builds:

```bash
# Backend
cd /home/runner/work/referee_hub/referee_hub/src/backend
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false

# Frontend
cd /home/runner/work/referee_hub/referee_hub/src/frontend
yarn build:dev
```

### 4. Check for Sensitive Data

- Review logs for email addresses, passwords, personal data
- Check that authorization is properly enforced
- Ensure no secrets are committed (API keys, tokens)

### 5. Update Documentation

If you made significant changes:
- Update `README.md` if setup/build steps changed
- Update `docs/` if APIs or architecture changed
- Add comments for complex logic

### 6. Validate .gitignore

Ensure build artifacts aren't committed:
- `src/frontend/dist/` - excluded
- `src/backend/**/bin/`, `**/obj/` - excluded
- `node_modules/` - excluded

### 7. Commit Message Format

Use clear, descriptive commit messages:

```
Add tournament management API endpoints

- Implement GET /api/tournaments
- Implement POST /api/tournaments
- Add TournamentManager authorization policy
- Add integration tests for tournament CRUD

Resolves #451
```

---

## Specific Coding Patterns

### 1. Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentContextProvider _tournamentContext;
    private readonly ILogger<TournamentsController> _logger;
    
    public TournamentsController(
        ITournamentContextProvider tournamentContext,
        ILogger<TournamentsController> logger)
    {
        _tournamentContext = tournamentContext;
        _logger = logger;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TournamentViewModel>>> GetTournaments()
    {
        var tournaments = await _tournamentContext.QueryTournaments()
            .Select(t => new TournamentViewModel
            {
                Id = t.UniqueId.ToString(),
                Name = t.Name
            })
            .ToListAsync();
            
        return Ok(tournaments);
    }
    
    [HttpPost]
    [Authorize(AuthorizationPolicies.TournamentManager)]
    public async Task<ActionResult<TournamentViewModel>> CreateTournament(
        [FromBody] CreateTournamentRequest request)
    {
        var tournamentId = await _tournamentContext.CreateTournamentAsync(
            new TournamentData
            {
                Name = request.Name,
                Type = request.Type
            });
            
        var tournament = await _tournamentContext.GetTournamentAsync(tournamentId);
        
        return CreatedAtAction(
            nameof(GetTournament),
            new { id = tournamentId.ToString() },
            MapToViewModel(tournament));
    }
}
```

### 2. Context Provider Pattern

```csharp
public interface ITournamentContextProvider
{
    IQueryable<TournamentContext> QueryTournaments();
    Task<TournamentContext> GetTournamentAsync(TournamentIdentifier id);
    Task<TournamentIdentifier> CreateTournamentAsync(TournamentData data);
}

public class TournamentContextProvider : ITournamentContextProvider
{
    private readonly ManagementHubDbContext _dbContext;
    
    public IQueryable<TournamentContext> QueryTournaments()
    {
        return _dbContext.Tournaments
            .Select(t => new TournamentContext
            {
                Id = t.Id,
                UniqueId = new TournamentIdentifier(t.UniqueId),
                Name = t.Name,
                // ... other fields
            });
    }
}
```

### 3. Authorization Requirement Pattern

```csharp
// Requirement
public class TournamentManagerRequirement : IAuthorizationRequirement
{
    public TournamentIdentifier? TournamentId { get; init; }
}

// Handler
public class TournamentManagerHandler : AuthorizationHandler<TournamentManagerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TournamentManagerRequirement requirement)
    {
        var userId = context.User.GetUserId();
        if (userId == null)
        {
            context.Fail();
            return;
        }
        
        // Check if user is tournament manager
        var isTournamentManager = await CheckTournamentManagerAsync(
            userId.Value,
            requirement.TournamentId);
            
        if (isTournamentManager)
        {
            context.Succeed(requirement);
        }
    }
}
```

### 4. View Model Pattern

```csharp
// API view model (returned to client)
public class TournamentViewModel
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required TournamentType Type { get; init; }
    public DateTime StartDate { get; init; }
}

// Request model (received from client)
public class CreateTournamentRequest
{
    public required string Name { get; init; }
    public required TournamentType Type { get; init; }
    public DateTime? StartDate { get; init; }
}
```

### 5. Email Template Pattern

Email templates in `ManagementHub.Mailers/Templates/`:

```csharp
public class TournamentInviteMailer : IMailer
{
    private readonly IMailSender _mailSender;
    
    public async Task SendInviteAsync(
        string recipientEmail,
        string tournamentName,
        string inviteUrl)
    {
        var subject = $"Invitation to {tournamentName}";
        var body = $@"
            <h1>You've been invited!</h1>
            <p>You've been invited to participate in {tournamentName}.</p>
            <a href=""{inviteUrl}"">Accept Invitation</a>
        ";
        
        await _mailSender.SendEmailAsync(recipientEmail, subject, body);
    }
}
```

### 6. React Component with RTK Query Pattern

```typescript
import { FC } from 'react';
import { useGetTournamentQuery } from '../store/serviceApi';
import { Spinner } from '../components/Spinner';
import { ErrorMessage } from '../components/ErrorMessage';

interface TournamentDetailProps {
  tournamentId: string;
}

export const TournamentDetail: FC<TournamentDetailProps> = ({ tournamentId }) => {
  const { data: tournament, isLoading, error } = useGetTournamentQuery(tournamentId);
  
  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage error={error} />;
  if (!tournament) return <div>Tournament not found</div>;
  
  return (
    <div className="tournament-detail">
      <h1 className="text-2xl font-bold">{tournament.name}</h1>
      <p>Type: {tournament.type}</p>
      <p>Start Date: {new Date(tournament.startDate).toLocaleDateString()}</p>
    </div>
  );
};
```

---

## Common Pitfalls

### 1. EF Core Shadow Properties

**Problem:** Forgetting to specify `.WithMany()` causes EF Core to create shadow properties.

```csharp
// ❌ BAD - Creates shadow property UserId1
entity.HasOne(d => d.User)
    .WithMany()  // Missing navigation property!
    .HasForeignKey(d => d.UserId);

// ✅ GOOD - Explicit navigation
entity.HasOne(d => d.User)
    .WithMany(p => p.TournamentManagers)
    .HasForeignKey(d => d.UserId);
```

**Check logs for warnings:**
```
The foreign key property 'TournamentManager.UserId1' was created in shadow state
```

### 2. Logging Sensitive Data

**Problem:** Logging email addresses or personal data violates privacy policies.

```csharp
// ❌ BAD
_logger.LogInformation("User {Email} logged in", user.Email);

// ✅ GOOD
_logger.LogInformation("User {UserId} logged in", user.UniqueId);
```

### 3. Not Using Strongly-Typed IDs

**Problem:** Using strings or GUIDs directly instead of typed identifiers.

```csharp
// ❌ BAD
public async Task<IActionResult> GetTournament(string id)

// ✅ GOOD
public async Task<IActionResult> GetTournament(TournamentIdentifier id)
```

### 4. Projection Before Filtering

**Problem:** Projecting to ViewModels before filtering prevents EF Core SQL translation.

```csharp
// ❌ BAD - Runs in memory
var result = await dbContext.Tournaments
    .Select(t => new TournamentViewModel { Id = t.Id, Name = t.Name })
    .Where(vm => vm.Name.Contains("World"))  // Can't translate to SQL!
    .ToListAsync();

// ✅ GOOD - Filter in SQL
var result = await dbContext.Tournaments
    .Where(t => t.Name.Contains("World"))
    .Select(t => new TournamentViewModel { Id = t.Id, Name = t.Name })
    .ToListAsync();
```

### 5. Direct User.UniqueId Comparison

**Problem:** Querying users by UniqueId directly without the helper pattern.

```csharp
// ❌ BAD
var user = await dbContext.Users
    .Where(u => u.UniqueId == userId.UniqueId)
    .FirstOrDefaultAsync();

// ✅ GOOD
var userDbId = await WithIdentifier(dbContext.Users, userId)
    .Select(u => u.Id)
    .FirstOrDefaultAsync();
```

### 6. Missing Frontend Type Safety

**Problem:** Using `any` instead of generated API types.

```typescript
// ❌ BAD
const [tournament, setTournament] = useState<any>(null);

// ✅ GOOD
import { TournamentViewModel } from '../store/serviceApi';
const [tournament, setTournament] = useState<TournamentViewModel | null>(null);
```

### 7. Not Running Swagger Regeneration

**Problem:** Making backend API changes without updating frontend types.

```bash
# After backend API changes, ALWAYS run:
cd /home/runner/work/referee_hub/referee_hub
bash scripts/refresh_swagger.sh
```

### 8. Forgetting Build Flags

**Problem:** Build failures in local development due to GitVersion.

```bash
# ❌ BAD - May fail in local/shallow clone
dotnet build

# ✅ GOOD - Use build flags for local development
dotnet build /p:DisableGitVersion=true /p:BuildFrontend=false
```

### 9. Legacy Redux Usage

**Problem:** Using old Redux patterns instead of RTK Query for API calls.

```typescript
// ❌ BAD - Legacy Redux
import { fetchTournaments } from '../store/tournaments/actions';
dispatch(fetchTournaments());

// ✅ GOOD - RTK Query
import { useGetTournamentsQuery } from '../store/serviceApi';
const { data: tournaments } = useGetTournamentsQuery();
```

### 10. Not Preserving Query Parameters

**Problem:** Using React Router's `useNavigate` directly loses `?impersonate` and `?features`.

```typescript
// ❌ BAD
import { useNavigate } from 'react-router-dom';

// ✅ GOOD
import { useNavigate } from '../utils/navigationUtils';
```

---

## Additional Resources

- **Swagger UI (when running)**: http://localhost:5000/swagger
- **MailHog (Docker staging)**: http://localhost:8025
- **Docker Compose configs**: `/home/runner/work/referee_hub/referee_hub/docker/`
- **CI/CD Pipeline**: `.github/workflows/pipeline.yml`
- **Test users (dev mode)**: See README.md

---

## Questions or Issues?

If you encounter patterns not covered in this guide:
1. Look for similar existing code in the repository
2. Check the `docs/` directory for feature-specific documentation
3. Review recent PRs and issues for context
4. When in doubt, ask for clarification before proceeding

**Remember**: Make minimal changes, follow existing patterns, test thoroughly, and document your work.
