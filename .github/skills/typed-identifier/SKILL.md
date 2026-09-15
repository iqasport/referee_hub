---
name: typed-identifier
description: Rules for using strongly-typed identifiers (*Identifier record structs) instead of raw strings or Guid. Use when adding new entities, API parameters, or domain models.
---

# Typed Identifiers

**Trigger**: Any new entity, API parameter, or domain model that needs an identifier.

**Use when**: Defining a new domain object, adding API endpoints, or writing queries that reference entities.

---

## The Pattern

Every domain entity MUST use a typed identifier — never raw `string`, `Guid`, or `long` for entity references.

### Record struct template

Read existing typed identifiers in `ManagementHub.Models/Domain/<Entity>/`:

| Identifier | Location |
|------------|----------|
| `TournamentIdentifier` | `Domain/Tournament/TournamentIdentifier.cs` |
| `UserIdentifier` | `Domain/User/UserIdentifier.cs` |
| `TeamIdentifier` | `Domain/Team/TeamIdentifier.cs` |
| `NgbIdentifier` | `Domain/Ngb/NgbIdentifier.cs` |
| `TestIdentifier` | `Domain/Test/TestIdentifier.cs` |

Each follows the pattern:
- **Record struct** with an underlying type (`Ulid`, `Guid`, `long`, or `string`)
- **`ToString()`** — Returns prefixed string (e.g., `TR_<ulid>`)
- **`TryParse(string, out)`** — Validates prefix and format
- **`Parse(string)`** — Throws on invalid input
- **`New<Entity>Id()`** — Factory method for creation

### Underlying types by entity

Read each identifier file for the exact underlying type and prefix:

| Entity | Underlying Type | Prefix | Notes |
|--------|----------------|--------|-------|
| `TournamentIdentifier` | `Ulid` | `TR_` | Time-sortable IDs |
| `UserIdentifier` | `Guid` | `U_` | Has `ToLegacyUserId()` for migration |
| `TeamIdentifier` | `long` | `TM_` | Implements `IIdentifiable` |
| `NgbIdentifier` | `string` (3-char code) | — | `TryParse` rejects `ANY` and non-3-char codes |
| `TestIdentifier` | `Ulid` | `TS_` | |
| `TestAttemptIdentifier` | `Ulid` | `TA_` | |
| `NotificationIdentifier` | `Ulid` | `NT_` | |
| `TeamInvitationIdentifier` | `Ulid` | `TI_` | |

### JSON serialization

Read existing converters in `ManagementHub.Serialization/Identifiers/`:

| Converter | Location |
|-----------|----------|
| `TournamentIdentifierJsonConverter` | `Identifiers/TournamentIdentifierJsonConverter.cs` |
| `UserIdentifierJsonConverter` | `Identifiers/UserIdentifierJsonConverter.cs` |
| `TeamIdentifierJsonConverter` | `Identifiers/TeamIdentifierJsonConverter.cs` |
| `NgbIdentifierJsonConverter` | `Identifiers/NgbIdentifierJsonConverter.cs` |

Each converter:
- **Read**: `identifier.Parse(reader.GetString()!)`
- **Write**: `writer.WriteStringValue(value.ToString())`
- **Register**: Add to `DbContext` or startup via `options.Converters.Add()`

---

## Rules

### ALWAYS:

1. **Use typed identifiers in API parameters** — `[FromRoute] TournamentIdentifier tournamentId`
2. **Use typed identifiers in domain methods** — `Tournament.UpdateName(TournamentIdentifier id, string name)`
3. **Log typed identifiers directly** — They have safe `ToString()` (prefix + value, never PII)
4. **Use `TryParse` in controllers** — Convert string route params to typed identifiers
5. **Use `New<Entity>Id()` for creation** — Never call `new()` directly

### NEVER:

1. **Use `Guid` or `string` for entity IDs** — Always wrap in a typed identifier
2. **Compare identifiers with `==`** — Use `.Equals()` or `==` on the underlying value
3. **Store raw IDs in domain models** — Domain models hold typed identifiers, not strings
4. **Bypass `TryParse` in public APIs** — Always validate the prefix and format

### When creating a new identifier:

1. Create the record struct in `ManagementHub.Models/Domain/<Entity>/`
2. Create the JSON converter in `ManagementHub.Serialization/Identifiers/`
3. Register the converter in `DbContext` configuration
4. Update API models to use the typed identifier
5. Update EF Core mapping if the underlying type differs from the column type

---

## Why This Matters

- **Type safety** — Compiler catches wrong entity references at compile time
- **Format validation** — `TryParse` enforces prefix + format at the boundary
- **Safe logging** — `ToString()` returns a non-sensitive, human-readable format
- **Migration path** — `UserIdentifier` shows how to handle legacy ID migration (`ToLegacyUserId`)
- **Self-documenting** — `TournamentIdentifier` is clearer than `string tournamentId`
