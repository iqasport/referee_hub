---
name: collection-filtering
description: Rules for the collection filtering system: FilteringParameters, Filtered<T>, CollectionFilteringContext, and the IAsyncActionFilter. Use when adding collection/list endpoints.
---

# Collection Filtering

**Trigger**: Any API endpoint that returns a collection of entities (list, search, or paginated results).

**Use when**: Adding a GET endpoint that lists entities, implementing search/filter, or building paginated APIs.

---

## The Pattern

The filtering system uses a **context-based approach** with an action filter that injects `FilteringParameters` from query strings and attaches `FilteringMetadata` to `Filtered<T>` responses.

### Core types

| Type | Location | Purpose |
|------|----------|---------|
| `FilteringParameters` | `ManagementHub.Storage/Collections/FilteringParameters.cs` | Query parameter model |
| `Filtered<T>` + `AsFiltered()` | `ManagementHub.Service/Filtering/Filtered.cs` | Response wrapper |
| `FilteringMetadata` | `ManagementHub.Storage/Collections/FilteringMetadata.cs` | Pagination metadata (TotalCount) |
| `CollectionFilteringContext` | `ManagementHub.Storage/Collections/CollectionFilteringContext.cs` | DI-scoped context for filter state |
| `CollectionFilteringActionFilter` | `ManagementHub.Service/Filtering/CollectionFilteringActionFilter.cs` | IAsyncActionFilter that wires everything |

### Controller pattern

Read `NgbsController.cs` or `TournamentsController.cs` in `ManagementHub.Service/Areas/` for full examples of:

- Accepting `FilteringParameters` as `[FromQuery]`
- Applying filter → order → skip → take
- Wrapping results with `.AsFiltered()` and setting `FilteringMetadata`

### How filtering context applies to EF queries

The filtering system works through a **DI-scoped `CollectionFilteringContext`** that lives across the action filter and controller:

1. **Action filter runs first** — reads query string params → writes to `CollectionFilteringContext.FilteringParameters`
2. **Context provider reads the context** — injects `CollectionFilteringContext` and reads `filteringContext.FilteringParameters` to build the EF query
3. **Controller calls `.Page()`** — applies skip/take from the parameters
4. **Controller calls `.AsFiltered()`** — wraps results; action filter then attaches `FilteringMetadata`

#### In a context provider (query building):

The context provider injects `CollectionFilteringContext` and reads from it to build the EF query:

- `filteringContext.FilteringParameters.Filter` — the search string (check null/empty before using)
- `filteringContext.FilteringParameters.Page` / `PageSize` — for pagination
- `filteringContext.FilteringParameters.SkipPaging` — skip paging entirely (true for dropdowns)
- `filteringContext.FilteringParameters as CustomFilteringParams` — cast to custom types for extra params
- `filteringContext.FilteringMetadata.TotalCount = query.Count()` — set count before paging

The context provider applies filtering at the EF level — never in-memory. Use `EF.Functions.ILike` for case-insensitive search on PostgreSQL.

#### In a controller (wrapping results):

The controller receives `FilteringParameters` as `[FromQuery]` (the action filter already wrote them to the context). The controller:

- Passes the same parameters to `.Page(filtering)` on the query
- Calls `.AsFiltered()` on the final result list (not on the query — on the materialized list)
- The action filter then attaches `FilteringMetadata` to the `Filtered<T>` response

### Applying NGB constraints to EF queries

For entities that belong to an NGB, the context provider must apply `NgbConstraint` filtering to every query:

1. **Inject the context provider** — e.g. `IRefereeContextProvider`, `ITournamentContextProvider`
2. **Pass `NgbConstraint` to the provider's query method** — never `NgbConstraint.Any` for scoped endpoints
3. **The provider applies `WithConstraint` internally** — via `CollectionFilteringContext` in the context factory

#### When to use which constraint:

| Constraint | When to use | SQL effect |
|-----------|-------------|------------|
| `NgbConstraint.Any` | Public endpoints, admin views, background jobs | No WHERE clause — returns all rows |
| `NgbConstraint.Single` | User-scoped endpoints (referee profile, user dashboard) | `WHERE country_code = '<user_ngb>'` |
| `NgbConstraint.Set` | Multi-tenant admin (user manages multiple NGBs) | `WHERE country_code IN ('<ngb1>', '<ngb2>', ...)` |
| `NgbConstraint.Empty` | Error case / no NGB context | `WHERE 1 = 0` — returns nothing |

#### In a context provider (applying constraint):

The context factory uses `CollectionFilteringContext.FilteringContext` to apply the constraint:

- `filteringContext.Filter(this.dbContext.Entities).WithConstraint(ngbConstraint)` — scoped query
- `filteringContext.Filter(this.dbContext.Entities)` — no constraint (all rows)
- The constraint is applied BEFORE `Select()` projection for proper SQL translation

#### In a controller (passing constraint):

The controller determines the correct `NgbConstraint` based on the user's context:

- Admin endpoints → `NgbConstraint.Any`
- User-scoped → `NgbConstraint.Single(userContext.Ngb)` or `NgbConstraint.Set(userContext.Ngbs)`
- The constraint is passed to the context provider's query method, not applied manually

### Custom filtering parameters

For endpoints that need additional filter parameters, extend `FilteringParameters` — see `TournamentFilteringParameters` as an example.

The action filter looks for a parameter named `filtering` by type. For custom types, the parameter name must still be `filtering` and inherit from `FilteringParameters`.

---

## Rules

### ALWAYS:

1. **Accept `FilteringParameters` as `[FromQuery]`** — Never build pagination manually in controllers
2. **Return `Filtered<T>`** — Wrap all collection responses with `AsFiltered()`
3. **Set `FilteringMetadata.TotalCount`** — Count BEFORE paging, after filtering
4. **Apply filter → order → skip → take** — Always in this order for correct SQL translation
5. **Name the parameter `filtering`** — The action filter looks for this exact name
6. **Use `SkipPaging = true` for dropdowns** — When the client needs all items (no pagination)
7. **Default page size of 25** — Match the `FilteringParameters` default

### NEVER:

1. **Bypass `Filtered<T>`** — All collections should be wrapped
2. **Set TotalCount after paging** — Must be count of ALL matching items, not just the page
3. **Use `Take` without `Skip`** — Always pair them for correct pagination
4. **Compare strings in filter without null check** — `!string.IsNullOrEmpty(filtering.Filter)`
5. **Apply ordering after projection** — Order BEFORE `Select()` for SQL translation

### Action filter behavior:

The `CollectionFilteringActionFilter` (registered as `IAsyncActionFilter`):

1. **Before action**: Injects `FilteringParameters` from query → `CollectionFilteringContext`
2. **Default `SkipPaging`**: Sets to `false` if not provided (for API endpoints)
3. **After action**: Attaches `FilteringMetadata` to `Filtered` responses if present

### Registering the filter:

In `Program.cs` or `Startup.cs`:
- Register `CollectionFilteringActionFilter` and `CollectionFilteringContext` as scoped
- Add to MVC filters via `options.Filters.Add<CollectionFilteringActionFilter>()`

---

## Why This Matters

- **Consistency** — All collection endpoints use the same filtering pattern
- **NGB scoping** — Filtering context integrates with NGB constraint filtering
- **Pagination metadata** — `TotalCount` is attached automatically by the action filter
- **Extensibility** — Custom filtering parameters inherit from `FilteringParameters`
- **SQL translation** — Filtering/ordering before projection ensures proper SQL generation
