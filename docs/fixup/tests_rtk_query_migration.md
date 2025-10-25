# RTK Query Migration Issues

## Overview
Tests that fail due to the migration from manual Redux state management to RTK Query hooks.

## Architecture Changes

### Before (Redux Sagas/Thunks)
- Manual state management with slices
- Actions dispatched: `getLanguagesStart`, `getTestsStart`, `updateUserStart`, etc.
- State stored in Redux slices (e.g., `state.languages.languages`, `state.tests.tests`)
- Tests used `mockedStore` to verify actions dispatched

### After (RTK Query)
- Automatic state management via RTK Query hooks
- Hooks: `useGetLanguagesQuery()`, `useGetAllTestsQuery()`, etc.
- State managed by RTK Query cache (e.g., `state.serviceApi`)
- Tests need to mock API responses, not verify actions

## Files Affected

### 1. `app/components/modals/TestEditModal/TestEditModal.test.tsx`
- **Failing Tests**:
  - "renders an empty form" - TypeError: Cannot read properties of undefined (reading 'filter')
  - "renders the languages"
  - "dispatches getCertifications call" 
  - "dispatches getLanguages call"
  - "renders the edit form"
  - "handles input changes"
  - "handles dropdown changes"
  - "enables the submit button after a change"
  - "shows an error when a required field is empty"
  - "dispatches an update on submit"
  - "dispatches to get the test"

- **Root Cause**: Component now uses RTK Query hooks:
  ```tsx
  const { data: tests } = useGetAllTestsQuery();
  const { data: languages } = useGetLanguagesQuery();
  ```
  - Tests fail on line 78: `tests.filter(t => t.testId === testId)?.[0]`
  - `tests` is undefined because RTK Query hooks aren't properly mocked

- **Fix Strategy**:
  - Mock RTK Query hooks using MSW (Mock Service Worker) or by wrapping with RTK Query Provider
  - Provide test data via mocked API responses rather than Redux state
  - Update assertions from checking dispatched actions to checking rendered results
  - Example:
    ```tsx
    // Instead of checking actions
    expect(mockStore.getActions()).toEqual([...])
    
    // Check rendered output or API calls
    await waitFor(() => screen.getByText('Expected Result'))
    ```

### 2. `app/pages/Settings/Settings.test.tsx`
- **Failing Tests**:
  - "shows the language dropdown" - Unable to find element with text
  - "allows for language selection"
  - "is saveable"
  - "fetches languages"

- **Root Cause**: Component still uses old Redux pattern with `useSelector` and `dispatch`
  - Uses `getLanguages()` action and `state.languages` slice
  - Tests expect old action pattern but component may have partial RTK Query integration

- **Fix Strategy**:
  - Verify if component is still using old Redux or has been migrated
  - If still using Redux: Fix mock store setup to include proper initial state
  - If migrated to RTK Query: Update tests similar to TestEditModal approach
  - Update test assertions to match actual component behavior

### 3. `app/components/tables/TestsTable/TestsTable.test.tsx`
- **Failing Tests**:
  - "renders all of the tests" - Unable to find elements
  - "renders the expected text rows"
  - "goes to the test view on row click"
  - "dispatches getLanguages call"
  - "dispatches getTests call"

- **Root Cause**: Component likely migrated to RTK Query but tests still expect Redux actions
  - Tests expect `getLanguagesStart` and `getTestsStart` actions
  - Component probably now uses `useGetLanguagesQuery()` and `useGetAllTestsQuery()`

- **Fix Strategy**:
  - Review actual component implementation
  - Update to mock RTK Query responses
  - Change assertions from action verification to rendered content verification

### 4. `app/components/Counter/Counter.test.tsx`
- **Failing Tests**:
  - "it handles a second tick" - Unable to find element with text "00:01"

- **Root Cause**: Timer/timing issue, may be unrelated to RTK Query
  
- **Fix Strategy**:
  - Review timer implementation
  - Ensure proper use of `act()` and fake timers
  - May need to add proper cleanup

## Mock Setup Patterns

### Pattern 1: Mock RTK Query Hooks Directly
```tsx
jest.mock('../../../store/serviceApi', () => ({
  useGetLanguagesQuery: () => ({
    data: mockLanguages,
    isLoading: false,
    isError: false,
  }),
  useGetAllTestsQuery: () => ({
    data: mockTests,
    isLoading: false,
    isError: false,
  }),
}));
```

### Pattern 2: Use MSW (Mock Service Worker)
```tsx
import { rest } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer(
  rest.get('/api/languages', (req, res, ctx) => {
    return res(ctx.json(mockLanguages));
  }),
);
```

### Pattern 3: RTK Query Test Store
```tsx
import { configureStore } from '@reduxjs/toolkit';
import { serviceApi } from '../../../store/serviceApi';

const createTestStore = () => {
  return configureStore({
    reducer: {
      [serviceApi.reducerPath]: serviceApi.reducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware().concat(serviceApi.middleware),
  });
};
```

## Implementation Priority

1. **High**: TestEditModal - Critical functionality, many failing tests
2. **High**: TestsTable - Admin functionality affected
3. **Medium**: Settings - User functionality but less critical
4. **Low**: Counter - Minor timer issue

## General Migration Steps

1. Identify if component uses RTK Query hooks
2. Update test utilities to support RTK Query
3. Mock API endpoints or hook responses
4. Replace action verification with output verification
5. Ensure proper async handling with `waitFor` and `act`
6. Validate against actual backend API responses
