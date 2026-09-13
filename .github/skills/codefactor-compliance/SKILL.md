---
name: codefactor-compliance
description: Checklist for CodeFactor compliance: BOM removal, trailing whitespace, formatting. Use before committing any code change.
---

# CodeFactor Compliance Checker

**Trigger**: Any file creation or modification in `src/backend/` or `src/frontend/`.

**Use when**: Before committing any code change to ensure CodeFactor won't auto-fix it.

---

## Checklist

### Before committing:

1. **No BOM characters** — Files must NOT start with a byte order mark (BOM):
   ```bash
   # Check for BOM in C# files
   file src/backend/**/*.cs | grep -i "with BOM"
   # Remove BOM if found
   sed -i '' '1s/^\xEF\xBB\xBF//' src/backend/**/*.cs
   ```

2. **No trailing whitespace** — Remove trailing spaces on all lines.

3. **No excessive blank lines** — Maximum 2 consecutive blank lines.

4. **C# formatting** — Run before committing:
   ```bash
   cd src/backend
   dotnet format
   ```

5. **Frontend linting** — Run before committing:
   ```bash
   cd src/frontend
   yarn lint
   ```

### Common CodeFactor issues:

- **BOM characters** — Most frequent auto-fix; files saved with UTF-8-BOM encoding
- **Extra blank lines** — Multiple consecutive empty lines in methods/classes
- **Unused using statements** — Clean up imports after refactoring
- **Semicolon/style issues** — `dotnet format` handles most of these
- **JavaScript/TSX formatting** — Ensure Prettier/eslint configs are applied

### Why this matters:

CodeFactor auto-fixes have appeared in multiple PRs (e.g., #555, #373, #371). These create unnecessary noise in the diff and can obscure real changes. Catching them locally keeps PRs clean.
