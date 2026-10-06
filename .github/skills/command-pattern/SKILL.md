---
name: command-pattern
description: Rules for the Command pattern in ManagementHub.Storage. Use when adding commands, database transactions, or CQRS-style operations.
---

# Command Pattern

**Trigger**: Any database write operation, multi-step transaction, or CQRS command.

**Use when**: Adding a new API endpoint that modifies data, creating background jobs, or writing any operation that touches the database.

---

## The Pattern

All database write operations go through the **Command pattern** in `ManagementHub.Storage/Commands/`. Commands:
1. Take typed identifiers and domain values as parameters
2. Use `IDatabaseTransactionProvider` for transactional safety
3. Return result DTOs (not void) for status reporting
4. Are organized by domain in subdirectories

### Directory structure

Read existing commands in `ManagementHub.Storage/Commands/`:

| Domain | Location |
|--------|----------|
| Payments | `Commands/Payments/` |
| NGB | `Commands/Ngb/` |
| Referee | `Commands/Referee/` |
| Tournament | `Commands/Tournament/` |
| Tests | `Commands/Tests/` |
| User | `Commands/User/` |

### Command interface

Read existing command interfaces in `ManagementHub.Models/Abstraction/Commands/`:

| Interface | Purpose |
|-----------|---------|
| `IUploadFileCommand` | File upload abstraction |
| `IAccessFileCommand` | Get access URI for uploaded file |
| `I<Entity>Command` | Domain-specific commands |

Commands follow the pattern:
- **Take typed identifiers** — All parameters use `*Identifier` record structs
- **Return result DTOs** — Not `void`. Use nested `enum Outcome` for status codes
- **Nested result types** — `public interface Result { enum Outcome { Success, NotFound, ... } }`

### Command implementation

Read existing implementations in `ManagementHub.Storage/Commands/` for patterns:
- `using var transaction = await this.databaseTransactionProvider.BeginAsync()` — Begin first
- `AsNoTracking()` for reads — Performance optimization
- `WithIdentifier` extension — Never compare IDs directly
- `transaction.CommitAsync()` — Commit last
- Result DTOs — `return new Result(Result.Outcome.Success)`

---

## Rules

### ALWAYS:

1. **Inject `IDatabaseTransactionProvider`** — Never use `dbContext.Database.BeginTransaction()` directly
2. **Use `using var transaction`** — Ensures disposal even on exceptions
3. **Call `transaction.CommitAsync()`** explicitly — The `using` only disposes, doesn't commit
4. **Return result DTOs** — Not `void`. Use nested `enum Outcome` for status codes
5. **Use `AsNoTracking()` for reads** — Performance optimization for queries that don't modify
6. **Use `WithIdentifier` extension** — Never compare `UniqueId` directly against string IDs
7. **Log at appropriate levels** — `Information` for normal flow, `Warning` for expected edge cases, `Error` for failures
8. **Use typed identifiers** — All parameters use `*Identifier` record structs

### NEVER:

1. **Skip transactions for multi-step writes** — Even 2 `SaveChangesAsync` calls need a transaction
2. **Log sensitive data** — Emails, passwords, PII (see `sensitive-data-checker`)
3. **Bypass `IDatabaseTransactionProvider`** — It handles retry logic and connection management
4. **Return raw exceptions from commands** — Wrap in result DTOs; let the caller handle exceptions
5. **Mix read and write in the same query** — Separate `AsNoTracking()` reads from writes

### Transaction pattern:

Read any command in `ManagementHub.Storage/Commands/` for the full pattern:
1. `using var transaction = await databaseTransactionProvider.BeginAsync()`
2. Read with `AsNoTracking()`
3. Write operations
4. `await transaction.CommitAsync()`
5. Return result DTO

### Registration:

Register in DI (usually in `ManagementHub.Service/Startup`):
```csharp
services.AddScoped<I<Entity>Command, <Entity>Command>();
```

### When to create a new command:

1. **API endpoint modifies data** — Every POST/PUT/PATCH/DELETE needs a command
2. **Background job** — Hangfire jobs should use commands, not direct DbContext access
3. **Multi-step operation** — If you need 2+ `SaveChangesAsync`, wrap in a command
4. **Complex business logic** — Eligibility checks, state transitions, calculations

### When NOT to create a command:

1. **Simple single-entity CRUD** — Inline in the controller is OK for trivial operations
2. **Read-only queries** — Use context providers, not commands
3. **Tests** — Use `_factory.CreateClient()` for integration tests

---

## Why This Matters

- **Transaction safety** — `IDatabaseTransactionProvider` handles retries and connection management
- **Consistency** — All writes go through the same pattern
- **Testability** — Commands are easy to mock; results are explicit
- **Status reporting** — Result DTOs provide clear return values without exceptions
- **Separation of concerns** — Controllers delegate to commands; commands delegate to DbContext
