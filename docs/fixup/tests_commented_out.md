# Commented Out Tests - Improvement Plan

## Overview
These test files have been completely commented out with TODO markers indicating they need to be rewritten after component migration to RTK Query.

## Test Files
1. `app/pages/RefereeProfile/RefereeHeader/RefereeHeader.test.tsx`
2. `app/pages/RefereeProfile/RefereeTeam/RefereeTeam.test.tsx`
3. `app/pages/Test/Details.test.tsx`

## Current State

### RefereeHeader.test.tsx
- **Status**: Fully commented out with `/** TODO: rewrite tests */`
- **Component Status**: Migrated to RTK Query (uses `useGetUserDataQuery`, `useGetUserAvatarQuery`, `useUpdateCurrentUserDataMutation`)
- **Test Count**: ~24 test cases commented out
- **Test Coverage**: 
  - Rendering referee name, pronouns, certifications, bio
  - Edit mode functionality
  - Form field handling
  - Toggle controls

### RefereeTeam.test.tsx
- **Status**: Fully commented out with `/** TODO: rewrite tests */`
- **Component Status**: Needs investigation to determine if migrated to RTK Query
- **Test Count**: 1 test case commented out
- **Test Coverage**: Basic component rendering with team selection

### Details.test.tsx (Test Details Page)
- **Status**: Fully commented out
- **Component Status**: Needs investigation
- **Test Count**: 2 test cases commented out
- **Test Coverage**: Rendering test details and language associations

## Issues to Address

### Common Issues
1. **Old Test Structure**: Tests were written for pre-migration component props structure
2. **Missing RTK Query Mocks**: No mocking setup for RTK Query hooks
3. **Store Dependencies**: Tests relied on old redux store structure

### Specific Challenges
1. **RefereeHeader**: Component now fetches data via RTK Query instead of props
   - Old: Props passed referee data directly
   - New: Uses `useGetUserDataQuery` hook with refereeId from route params
   
2. **Route Parameter Mocking**: Components use `useNavigationParams` to get route params
   - Need to mock `useNavigationParams` or `useParams` from react-router

3. **API Response Mocking**: Need to properly mock RTK Query responses

## Improvement Strategy

### Phase 1: Setup Test Infrastructure
1. Create RTK Query test utilities for mocking API responses
2. Setup proper provider wrappers with RTK Query store
3. Create mock factories for API response shapes

### Phase 2: Rewrite Tests
For each component:
1. Analyze current component implementation
2. Identify RTK Query hooks being used
3. Rewrite tests to mock RTK Query responses instead of props
4. Update assertions to match new component structure
5. Add new tests for loading and error states

### Phase 3: Validation
1. Ensure all tests pass
2. Verify test coverage meets minimum threshold
3. Check that tests actually validate component logic (not just mocks)

## Implementation Plan

### RefereeHeader Tests
```typescript
// Key changes needed:
// 1. Mock RTK Query hooks
// 2. Mock route params (refereeId)
// 3. Test loading/error states from API
// 4. Test mutations on save

describe("RefereeHeader", () => {
  // Mock useNavigationParams to return refereeId
  // Mock useGetUserDataQuery to return user data
  // Mock useGetUserAvatarQuery for avatar
  // Test component renders with mocked data
  // Test edit mode with updateUser mutation
});
```

### RefereeTeam Tests
```typescript
// Key changes needed:
// Investigate if component uses RTK Query
// If not, can keep simpler test structure
// If yes, follow same pattern as RefereeHeader
```

### Details Tests
```typescript
// Key changes needed:
// Check if component uses RTK Query for test data
// Mock language data appropriately
// Test basic rendering functionality
```

## Testing Approach
- **Minimize Mocking**: Only mock RTK Query API calls, not component logic
- **Test Real Behavior**: Verify user interactions trigger correct mutations
- **Loading States**: Test how components handle loading states
- **Error States**: Test error handling from API calls
- **Integration Level**: Test components with real RTK Query setup where possible

## Expected Outcome
- All commented-out tests uncommented and passing
- Tests validate actual component behavior
- Good coverage of happy path and error scenarios
- Tests are maintainable and clear
