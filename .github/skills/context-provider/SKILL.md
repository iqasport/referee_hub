---
name: context-provider
description: Rules for context providers that encapsulate entity queries with NGB scoping. Use when adding new entity queries, context factories, or filtering logic.
---

# Context Providers

**Trigger**: Any new entity query, context factory, or data access layer that needs NGB scoping.

**Use when**: Adding a new domain entity, creating a context/interface pair, or writing queries that need NGB filtering.

---

## The Pattern

Context providers encapsulate all queries for a domain entity, applying `NgbConstraint` filtering automatically. They use **context factories** for lazy query building and **`CollectionFilteringContext`** for dynamic filtering.

### Interface pattern

Read existing context interfaces in `ManagementHub.Models/Abstraction/Contexts/Providers/`:

| Interface | Purpose |
|-----------|---------|
| `INgbContextProvider` | NGB queries with constraint |
| `IRefereeContextProvider` | Referee queries with constraint |
| `ITournamentContextProvider` | Tournament queries with constraint |
| `IUserContextProvider` | User queries |
| `ITeamContextProvider` | Team queries |
| `ITestContextProvider` | Test queries |


### Implementation pattern

Read existing implementations in `ManagementHub.Storage/Contexts/<Entity>/`:

| Provider | Location |
|----------|----------|
| `DbNgbContextProvider` | `ManagementHub.Storage/Contexts/Ngb/` |
| `DbRefereeContextProvider` | `ManagementHub.Storage/Contexts/Referee/` |
| `DbTournamentContextProvider` | `ManagementHub.Storage/Contexts/Tournament/` |
| `DbUserContextProvider` | `ManagementHub.Storage/Contexts/User/` |
| `DbTeamContextProvider` | `ManagementHub.Storage/Contexts/Team/` |
| `DbTestContextProvider` | `ManagementHub.Storage/Contexts/Test/` |

Each has a matching `Db<Entity>ContextFactory` in the same directory.

### Context Factory pattern

Context factories handle the heavy lifting:
- Apply `CollectionFilteringContext` for NGB scoping
- Apply `WithConstraint` for NGB filtering
- Map to interface types via `.Select()`
- Log query operations

### Caching

Read `Cached<Entity>ContextProvider` wrappers in `ManagementHub.Processing/Contexts/` for examples of caching layers.

---

## Rules

### ALWAYS:

1. **Inject `CollectionFilteringContext`** in context factories — It applies NGB scoping automatically
2. **Use `NgbConstraint` in all public query methods** — Never expose unscoped queries
3. **Provide all three retrieval modes**: single (`Task<T>`), queryable (`IQueryable<T>`), async enumerable (`IAsyncEnumerable<T>`)
4. **Map to interface types** — Return `I<Entity>Context`, not concrete types
5. **Use context factories** — Keep the provider thin (just delegation), put logic in factories
6. **Use `ILogger` in factories** — Log query operations for debugging

### Context types by entity:

| Entity | Context Interface | Stats Context | Provider |
|--------|-------------------|---------------|----------|
| NGB | `INgbContext` | `INgbStatsContext` | `DbNgbContextProvider` |
| Referee | `IRefereeViewContext` | — | `DbRefereeContextProvider` |
| Tournament | `ITournamentContext` | `ITournamentStatsContext` | `DbTournamentContextProvider` |
| User | `IUserContext` | `IUserStatsContext` | `DbUserContextProvider` |
| Test | `ITestContext` | — | `DbTestContextProvider` |
| Team | `ITeamContext` | — | `DbTeamContextProvider` |

### Registration:

Register in DI (usually in `ManagementHub.Service/Startup` or `Program.cs`):
```csharp
services.AddScoped<I<Entity>ContextProvider, Db<Entity>ContextProvider>();
```

---

## Why This Matters

- **NGB scoping** — All queries automatically respect NGB boundaries via `CollectionFilteringContext`
- **Single source of truth** — All queries for an entity go through one provider
- **Testability** — Interfaces can be mocked; factories are easy to unit test
- **Caching** — Cached wrappers can be layered on without changing the interface
- **Consistency** — All consumers use the same query patterns and filtering
