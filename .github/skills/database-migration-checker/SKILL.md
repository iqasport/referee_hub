---
name: database-migration-checker
description: Checklist for database migrations and EF Core entity configuration. Use when modifying entities, adding tables or relationships.
---

# Database Migration Checker

**Trigger**: Any change that modifies domain models, EF Core entities, or adds new tables/relationships.

**Use when**: The PR touches `ManagementHub.Models/`, `ManagementHub.Storage/`, or adds new entity classes.

---

## Checklist

### Before committing:

1. **Migration exists** — If you added/modified entities, create a migration:
   ```bash
   cd src/backend/ManagementHub.Service
   dotnet ef migrations add <DescriptiveName> --project ../ManagementHub.Storage
   ```

2. **Migration is clean** — Review the generated migration file:
   - No unexpected table drops or renames
   - Foreign keys have proper indexes
   - Column types match the entity (e.g., `bool` → `bit`, `string` → `nvarchar`)
   - Seed data (if any) is correct

3. **Entity configuration** — Verify in `ManagementHub.Storage/ManagementHubDbContext.cs`:
   - All relationships have `.WithMany()` or `.WithOne()` (avoids shadow properties)
   - Foreign keys are indexed
   - Delete behavior is intentional (`Restrict` vs `Cascade`)

4. **Database context abstraction** — If adding new queries, check if a new context interface is needed (`INgbStatsContext` pattern)

### ⚠️ Never modify committed migrations

**Once a migration is merged into `main`, never edit it.** Modifying applied migrations corrupts the migration history — other developers' databases will diverge.

- **Current WIP branch only** — You may edit migrations you just created in your feature branch, before merging to `main`.
- **Migration already on main?** — Create a *new* migration to fix it instead.
- **Need to drop a migration?** — Use `dotnet ef migrations remove` only if it hasn't been applied to any database yet.

### Common pitfalls:

- **Missing `.WithMany()`** — Creates invisible shadow properties that cause runtime errors
- **Missing indexes** — Foreign key columns should be indexed for query performance
- **Wrong delete behavior** — `Restrict` prevents accidental cascade deletes across NGB/team boundaries
- **Migration not applied** — Always verify the migration applies cleanly in a test database
