---
name: query-extension
description: Rules for query filtering patterns: WithConstraint, WithIdentifier, WithIdentifiers, and entity-specific extension methods. Use when writing repository queries, context queries, or filtering entity collections.
---

# Query Extensions

**Trigger**: Any repository or context query that filters entities by identifier or constraint.

**Use when**: Writing `IQueryable` filters, querying through context providers, or building scoped database queries.

---

## The Pattern

Entity collections are filtered via **extension methods** on `IQueryable<T>`:

| Extension | Purpose |
|-----------|---------|
| `WithIdentifier(id)` | Filter to a single entity by its typed identifier |
| `WithIdentifiers(ids)` | Filter to multiple entities by typed identifiers |
| `WithConstraint(constraint)` | Filter by `NgbConstraint` (ANY, single, or set) |

### Location

All extension methods live in `ManagementHub.Storage/Extensions/`:

| File | Entity |
|------|--------|
| `NgbCollectionExtensions.cs` | `NationalGoverningBody` |
| `TeamCollectionExtensions.cs` | `Team` |
| `UserCollectionExtensions.cs` | `User` |
| `TestCollectionExtensions.cs` | `Test` |

Read the relevant file for the exact implementation — signatures and legacy-fallback logic may change.

### NgbConstraint

Located in `ManagementHub.Models/Domain/Ngb/NgbConstraint.cs`:

| Member | Purpose |
|--------|---------|
| `NgbConstraint.Any` | Unscoped — matches all NGBs |
| `NgbConstraint.Single(id)` | Single NGB |
| `NgbConstraint.Set(ids)` | Multiple NGBs |
| `NgbConstraint.Empty()` | None — matches no NGBs |
| `constraint.AppliesToAny` | Check before filtering (early return) |
| `constraint.AppliesTo(id)` | Check if a specific NGB is included |
| `NgbConstraint.TryParse/Parse` | String → constraint conversion |

---

## Usage in Context Providers

Context providers use these extensions to build scoped queries. Read `DbNgbContextProvider.cs`, `DbRefereeContextProvider.cs`, and `DbTeamContextProvider.cs` for full examples of:

- `QueryNgbs(NgbConstraint)` — NGB-scoped query building
- `QueryReferees(NgbConstraint)` — Referee queries with constraint
- `QueryTeams(NgbConstraint)` — Team queries with constraint

---

## Rules

### ALWAYS:

1. **Use `WithIdentifier` for single-entity lookups** — Not `.Where(x => x.Id == id)`
2. **Use `WithConstraint(constraint)` for NGB-scoped queries** — Never filter NGBs manually
3. **Check `constraint.AppliesToAny` first** — Return early to avoid unnecessary filtering
4. **Use `WithIdentifiers` for bulk lookups** — Not multiple `.Where()` calls
5. **Include legacy fallbacks for User/Test** — `user.UniqueId == null` paths for migration
6. **Pass `NgbConstraint.Any` for unscoped queries** — Not `null` (which would be ambiguous)
7. **Use `.Contains()` in LINQ** — For `WithIdentifiers`, ensure it translates to SQL `IN`

### NEVER:

1. **Write manual `.Where()` for identifier filtering** — Always use the extension method
2. **Compare `NgbConstraint` directly** — Use `AppliesToAny` or `AppliesTo(id)`
3. **Pass `null` as a constraint** — Use `NgbConstraint.Any` instead
4. **Skip `WithConstraint` for NGB-scoped queries** — This is the core NGB multi-tenancy mechanism
5. **Use `.Any()` instead of `.Contains()` for collection filtering** — `.Contains()` translates to SQL `IN`

### When to create a new extension:

| Scenario | Action |
|----------|--------|
| New entity type | Add `*CollectionExtensions.cs` with `WithIdentifier` |
| Entity-specific filter | Add a new extension (e.g., `WithEmail`) |
| NGB-scoped query | Use `WithConstraint` from `NgbCollectionExtensions` |
| Legacy ID fallback | Include `UniqueId == null` paths in the extension |

### Adding a new extension method:

1. **Create/extend** `*CollectionExtensions.cs` in `ManagementHub.Storage/Extensions/`
2. **Use `this IQueryable<T>`** as the first parameter
3. **Return `IQueryable<T>`** — Never `IEnumerable<T>` (must remain translatable)
4. **Use only EF Core-translatable operations** — `.Where()`, `.Contains()`, `.Select()`, etc.
5. **Handle null constraints** — Check for `null` or `AppliesToAny` before filtering

---

## Why This Matters

- **NGB multi-tenancy** — `WithConstraint` is the core mechanism for NGB-scoped queries
- **Type safety** — `WithIdentifier` uses typed identifiers, not raw IDs
- **Legacy migration** — `UserIdentifier` and `TestIdentifier` support both new UUID and legacy int IDs
- **Query composability** — Extensions chain naturally: `.WithConstraint().Include().Select()`
- **Single source of truth** — All filtering logic lives in extension methods, not scattered in queries
- **Performance** — `NgbConstraint.Any` returns the unfiltered query, avoiding unnecessary SQL
