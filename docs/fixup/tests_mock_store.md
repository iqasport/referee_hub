# Mock and Store Configuration Issues

## Overview
Tests that fail due to improper mock store setup or missing data in the mocked state.

## Core Issue

The test utility (`app/utils/test-utils.tsx`) uses `redux-mock-store` which is designed for the old Redux pattern. However, the application is migrating to RTK Query, which requires:
1. Real Redux store with RTK Query reducer
2. Proper middleware setup
3. API endpoint mocking instead of state mocking

## Current Test Utility Limitations

```tsx
// Current setup - redux-mock-store
export const mockedStore = configureMockStore([thunk]);

// Creates a mock store that:
// - Only tracks dispatched actions
// - Doesn't actually update state
// - Doesn't handle RTK Query middleware
```

## Hybrid Architecture Challenges

The application currently uses BOTH:
- **Old Redux pattern**: `modules/` directory with slices for languages, tests, certifications, etc.
- **New RTK Query**: `store/serviceApi.ts` with auto-generated endpoints

Tests need to support both patterns during the migration period.

## Affected Tests

### 1. Settings Component Tests
- **Issue**: Mock store doesn't properly initialize nested state
- **Current Setup**:
  ```tsx
  const defaultStore = {
    currentUser: {
      currentUser: { ...currentUser, enabledFeatures: ["i18n"] },
    },
    languages: { languages },
  };
  ```
- **Problem**: Mock store doesn't return state via `getState()`, only tracks actions
- **Fix**: Need real store or better mock configuration

### 2. TestsTable Tests  
- **Issue**: Similar to Settings - expects state but mock doesn't provide it
- **Current Setup**: Expects `state.languages.languages` and `state.tests.tests`
- **Problem**: useSelector returns undefined

### 3. TestEditModal Tests
- **Issue**: RTK Query hooks return undefined
- **Current Setup**: No RTK Query setup in mock store
- **Problem**: `useGetAllTestsQuery()` and `useGetLanguagesQuery()` return undefined

## Solution Approaches

### Approach 1: Enhanced Test Utilities (Recommended)

Create a new test utility that supports both patterns:

```tsx
import { configureStore } from '@reduxjs/toolkit';
import { serviceApi } from '../store/serviceApi';
import rootReducer from '../rootReducer';

export const createTestStore = (preloadedState = {}) => {
  return configureStore({
    reducer: rootReducer,
    preloadedState,
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware().concat(serviceApi.middleware),
  });
};

export const renderWithProviders = (
  ui: ReactElement,
  {
    preloadedState = {},
    store = createTestStore(preloadedState),
    ...renderOptions
  } = {}
) => {
  const Wrapper = ({ children }) => (
    <Provider store={store}>
      <MemoryRouter>{children}</MemoryRouter>
    </Provider>
  );
  return { store, ...render(ui, { wrapper: Wrapper, ...renderOptions }) };
};
```

**Pros**: 
- Supports both Redux and RTK Query
- Real state updates
- Can test actual reducer logic

**Cons**:
- More setup per test
- Slower than mock store

### Approach 2: Mock RTK Query Hooks

Mock the hooks directly in tests:

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

**Pros**:
- Quick to implement
- Focused on component behavior

**Cons**:
- Lots of boilerplate
- Need to mock each hook per test file
- Can't test integration with RTK Query

### Approach 3: MSW (Mock Service Worker)

Mock API endpoints at the network level:

```tsx
import { rest } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer(
  rest.get('/api/languages', (req, res, ctx) => {
    return res(ctx.json(mockLanguages));
  }),
  rest.get('/api/tests', (req, res, ctx) => {
    return res(ctx.json(mockTests));
  }),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

**Pros**:
- Most realistic - tests actual HTTP layer
- Reusable across tests
- Good for integration testing

**Cons**:
- More setup complexity
- Need to know actual API endpoints and response formats

## Recommended Strategy

**Phase 1 - Quick Wins**: Mock RTK Query hooks for components that fully migrated
- TestEditModal
- Any other components using only RTK Query

**Phase 2 - Enhanced Utilities**: Create `renderWithProviders` for hybrid components
- Settings
- TestsTable  
- Components using both old Redux and RTK Query

**Phase 3 - MSW Integration**: For comprehensive API testing
- Add MSW for integration tests
- Use for testing error states, loading states, etc.

## Implementation Checklist

- [ ] Create `createTestStore` utility function
- [ ] Create `renderWithProviders` utility function
- [ ] Update test-utils.tsx to export both old and new helpers
- [ ] Identify which tests need which approach
- [ ] Update tests progressively
- [ ] Add MSW setup for future integration tests

## Store State Structure

For reference, here's the expected state structure:

```tsx
{
  // Old Redux slices
  certifications: { certifications: [] },
  languages: { languages: [] },
  tests: { tests: [] },
  currentUser: {
    currentUser: { id, enabledFeatures, ... },
    language: { ... }
  },
  
  // RTK Query cache
  serviceApi: {
    queries: {
      'getLanguages(undefined)': { data: [], status: 'fulfilled', ... },
      'getAllTests(undefined)': { data: [], status: 'fulfilled', ... },
    },
    mutations: {},
  }
}
```

## Next Steps

1. Review actual component implementations to confirm which pattern they use
2. Choose approach per test file based on component needs
3. Implement test utilities
4. Update tests incrementally
5. Validate with actual API responses from backend
