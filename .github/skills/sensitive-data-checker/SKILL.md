---
name: sensitive-data-checker
description: Rules for safe logging: never log emails, passwords, or PII; use typed identifiers which have safe ToString(). Use when adding logging or error handling.
---

# Sensitive Data in Logs Checker

**Trigger**: Any code that writes to logs (`ILogger`, `console.log`, Bugsnag, error handlers).

**Use when**: Adding logging, error handling, or data export functionality.

---

## Rules

### NEVER log:

- **Emails** — User emails, invite addresses, team emails
- **Passwords** — Or password hashes, verification codes, tokens
- **PII** — Names, phone numbers, addresses, national IDs
- **API keys/secrets** — Any credential or token value
- **Payment data** — Credit card numbers, bank details

### ALWAYS log:

- **Typed identifiers** — They have safe `ToString()` implementations:
  ```csharp
  // ✅ GOOD — typed identifier is safe to log directly
  _logger.LogInformation("Processing team {TeamId}", teamId);
  _logger.LogError(ex, "Failed for referee {RefereeId}", refereeId);
  ```

- **Non-sensitive context** — Action names, entity types, counts, statuses

### Common patterns:

```csharp
// ❌ BAD — logs email
_logger.LogInformation("User login for {Email}", email);

// ❌ BAD — logs password
_logger.LogWarning("Password attempt: {Password}", password);

// ✅ GOOD — logs typed identifier
_logger.LogInformation("User login for user {UserId}", userId);

// ❌ BAD — unnecessary sanitization (typed identifiers are already safe)
_logger.LogError(ex, "Failed for team {TeamId}", SanitizeForLog(teamId.ToString()));

// ✅ GOOD — log the exception with context
_logger.LogError(ex, "Failed to send notification to team {TeamId}", teamId);
```

### Frontend:

- Don't log auth tokens, session data, or user credentials to `console.log`
- Bugsnag error reports should strip PII before capturing
- Feature gate values are safe to log

### Why this matters:

Logging PII is a security compliance risk. Typed identifiers (`*Identifier` record structs) are designed to be safe for logging — they expose only the public prefix + ULID, never personal data.
