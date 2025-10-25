# Legacy Redux Tests - Improvement Plan

## Overview
These tests are failing because they expect the old redux store structure with manual action dispatching, but the components have been partially or fully migrated to RTK Query.

## Test Files
1. `app/pages/Settings/Settings.test.tsx` (4 passing, 4 failing)
2. `app/components/tables/TestsTable/TestsTable.test.tsx` (0 passing, 6 failing)
3. `app/components/modals/TestEditModal/TestEditModal.test.tsx` (0 passing, 12 failing)

## Current State

### Settings.test.tsx
**Component Migration Status**: Partially migrated
- Still uses legacy redux for `updateUser` and `getLanguages` actions
- Uses `useSelector` for `currentUser` and `languages` state
- Component file: `app/pages/Settings/Settings.tsx`

**Passing Tests (4)**:
- ✅ renders the settings page
- ✅ shows cta to change language when user has no language
- ✅ shows the current language (with a language)
- ✅ does not render the settings page (without feature flag)

**Failing Tests (4)**:
- ❌ shows the language dropdown (can't find "Don't see your desired language?")
- ❌ allows for language selection (can't find placeholder)
- ❌ is cancelable (can't find text)
- ❌ is saveable (can't find placeholder)
- ❌ fetches languages (expects action dispatch - Legacy)

**Issues**:
1. Component renders but dropdown/editing UI elements not showing up
2. Tests expect old action types like `currentUser/updateUserStart` and `languages/getLanguagesStart`
3. Component logic might have changed during migration

### TestsTable.test.tsx
**Component Migration Status**: Fully migrated to RTK Query
- Uses `useGetAllTestsQuery` for fetching tests
- Uses `useSetTestActiveMutation` for updates
- Component file: `app/components/tables/TestsTable/TestsTable.tsx`

**Failing Tests (6)**:
- ❌ renders all of the tests (expects old store structure)
- ❌ renders the expected text rows (expects old store structure)
- ❌ goes to the test view on row click (expects old store structure)
- ❌ dispatches getLanguages call (Legacy - expects action)
- ❌ dispatches getTests call (Legacy - expects action)

**Issues**:
1. Tests expect old store shape: `{ tests: { tests: [], certifications: [], isLoading: false } }`
2. Component now uses RTK Query which has different state shape
3. Tests check for action dispatches which don't exist in RTK Query
4. Need to mock RTK Query hooks instead of store state

### TestEditModal.test.tsx
**Component Migration Status**: Needs investigation
- Test expects legacy store structure
- Component file: `app/components/modals/TestEditModal/TestEditModal.tsx`

**Failing Tests (12)**:
- ❌ All tests failing due to runtime errors
- Errors show: `Cannot read properties of undefined (reading 'filter')`
- Component rendering crashes, preventing any tests from running

**Issues**:
1. Component likely has undefined data access issues
2. Tests expect old store structure with `certifications`, `languages`, `test` slices
3. Need to investigate component implementation to understand RTK Query migration status
4. May need complete rewrite if component is migrated

## Root Cause Analysis

### Settings Component
Looking at `Settings.tsx`:
- Uses `useDispatch` with `updateUser()` and `getLanguages()` - these are legacy thunk actions
- Uses `useSelector` to access `state.currentUser` and `state.languages`
- **This component has NOT been migrated to RTK Query**

The tests fail because:
1. The dropdown is probably not rendering due to async rendering issues
2. Tests need to use `waitFor` or `findBy*` queries for async content
3. Action dispatch assertions work but UI elements need async queries

### TestsTable Component
Looking at `TestsTable.tsx`:
- Uses `useGetAllTestsQuery()` - fully migrated to RTK Query
- No longer dispatches old redux actions
- **This component HAS been migrated to RTK Query**

The tests fail because:
1. Tests still mock old redux store structure
2. Need to mock RTK Query instead
3. Tests checking for action dispatches are no longer valid

### TestEditModal Component
Need to check implementation to determine migration status.

## Improvement Strategy

### For Non-Migrated Components (Settings)
**Approach**: Keep using redux-mock-store but fix async rendering issues

1. Use `waitFor`, `findBy*` queries for elements that appear after state updates
2. Keep action dispatch assertions as-is (they're valid)
3. Update mock store structure if needed
4. Add better async handling for edit mode

**Note**: Per issue requirements, we should skip tests for legacy redux code. However, Settings.test.tsx is partially working and the component isn't fully legacy - it's just not migrated yet. We should:
- Fix the async rendering issues
- Skip the action dispatch assertions (mark as legacy)
- Focus on UI behavior tests

### For Migrated Components (TestsTable, TestEditModal)
**Approach**: Rewrite to use RTK Query mocking

1. Remove redux-mock-store usage
2. Use RTK Query test utilities or MSW (Mock Service Worker)
3. Mock API responses instead of store state
4. Remove action dispatch assertions (no longer applicable)
5. Add tests for loading/error states

### Implementation Phases

#### Phase 1: Settings Tests (Partial Fix)
```typescript
// Fix async rendering issues
test("shows the language dropdown", async () => {
  render(<Settings />, mockStore);
  
  const editButton = screen.getByText("Edit");
  await userEvent.click(editButton);
  
  // Use findBy for async element
  await screen.findByText("Don't see your desired language?", { exact: false });
});

// Skip legacy action dispatch tests
test.skip("fetches languages", () => {
  // Legacy redux - skip
});
```

#### Phase 2: TestsTable Tests (Complete Rewrite)
```typescript
// Setup RTK Query mock
import { setupApiStore } from '../utils/rtk-test-utils';

test("renders all of the tests", async () => {
  const tests = factories.test.buildList(5);
  
  // Mock RTK Query endpoint
  const { store } = setupApiStore({
    getAllTests: { data: tests }
  });
  
  render(<TestsTable />, { store });
  
  // Tests should now render
  for (const test of tests) {
    expect(await screen.findByText(test.title)).toBeInTheDocument();
  }
});
```

#### Phase 3: TestEditModal Tests (Investigate & Rewrite)
1. First, check component implementation
2. Determine migration status
3. Fix runtime errors
4. Rewrite tests following RTK Query patterns

## Testing Approach

### For Settings (Non-Migrated)
- **Keep Simple**: Use existing redux-mock-store
- **Fix Async**: Add proper async waits
- **Skip Legacy**: Mark action dispatch tests as skipped with comment
- **Focus on UI**: Test user interactions and UI state

### For Migrated Components
- **Mock APIs**: Use RTK Query test utilities
- **Real Queries**: Let RTK Query machinery run with mocked endpoints
- **Test Behavior**: Verify data flows through to UI correctly
- **Error/Loading**: Test loading and error UI states

## Expected Outcome
- Settings: 4 UI tests passing, 1 test skipped (legacy redux action)
- TestsTable: 3-4 tests passing (skip legacy action dispatch tests)
- TestEditModal: All tests passing or properly skipped based on component status
- Clear separation between legacy and migrated code
- Tests validate actual UI behavior, not just redux actions
