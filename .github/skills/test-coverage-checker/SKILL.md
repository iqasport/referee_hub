---
name: test-coverage-checker
description: Checklist for unit and integration test coverage. Use when adding new features, API endpoints, or business logic.
---

# Test Coverage Checker

**Trigger**: Any new feature, API endpoint, or business logic change.

**Use when**: Adding a new controller action, service method, eligibility rule, or integration point.

---

## Checklist

### Before committing:

1. **Unit tests** — Only for genuinely non-trivial methods that integration tests won't cover:
   - Complex eligibility rules, state machines, calculations
   - Methods with multiple branching paths
   - Pure logic with no DB/HTTP dependencies
   - Skip unit tests for simple CRUD, mappers, or thin service wrappers — integration tests cover those
   - When you do write unit tests: test happy path, edge cases (null, empty, boundary values), and error paths

2. **Integration tests for API endpoints** — New endpoints need tests in `ManagementHub.IntegrationTests/`:
   - Authenticated success case
   - Unauthorized access returns 401/403
   - Invalid input returns 400
   - Use `TestWebApplicationFactory` with `AuthenticationHelper`

3. **Eligibility tests** — When adding new test types (e.g., FlagRunner), update `RefereeEligibilityUnitTests`:
   - Verify the new type appears in available tests
   - Verify prerequisite eligibility rules
   - Verify recertification eligibility rules

4. **Frontend tests** — For non-trivial UI logic:
   - Test custom hooks (`useFeatureGates`, `useNavigate`)
   - Test API client integration
   - Skip legacy Redux tests; focus on RTK Query

### Common pitfalls:

- **Missing auth tests** — Always test both authorized and unauthorized access
- **Missing integration tests** — API changes need integration tests, not just unit tests
- **Skipping eligibility tests** — New test types MUST have eligibility coverage
- **Over-mocking** — Minimize mocking; test real behavior where possible
- **Integration test cleanup** — Use `IAsyncLifetime` and clean up test data
