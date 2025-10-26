# Remaining Test Work

## Summary of Progress

### ✅ Completed
- Counter tests: Fully working with proper logic validation (11 tests)
- Avatar tests: Working (1 test)
- Settings tests: Working with async handling (7 tests, 2 skipped legacy)
- TestsTable tests: Working with minimal mocking (5 tests)
- TestEditModal tests: Working with comprehensive validation (7 tests, 1 skipped)
- Details tests: Restored and working (3 tests)

**Total: 32 passing tests, 4 skipped**

### ⚠️ Needs Work: RefereeHeader and RefereeTeam

Both components have been restored from commented state but need RTK Query implementation:

#### RefereeHeader.test.tsx
**Component Uses:**
- `useGetUserDataQuery({ userId })`
- `useGetUserAvatarQuery({ userId })`  
- `useUpdateCurrentUserDataMutation()`

**Tests to Implement:**
1. Renders referee name from user data
2. Renders default name if names not present
3. Renders pronouns when showPronouns is true
4. Doesn't render pronouns when showPronouns is false
5. Renders certifications list
6. Renders edit form when isEditing is true
7. Handles form submission

**Implementation Approach:**
```typescript
const mockUseGetUserDataQuery = jest.fn();
const mockUseGetUserAvatarQuery = jest.fn();
const mockUseUpdateCurrentUserDataMutation = jest.fn();

jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetUserDataQuery: () => mockUseGetUserDataQuery(),
  useGetUserAvatarQuery: () => mockUseGetUserAvatarQuery(),
  useUpdateCurrentUserDataMutation: () => mockUseUpdateCurrentUserDataMutation(),
}));

// In tests:
mockUseGetUserDataQuery.mockReturnValue({
  data: { firstName: "John", lastName: "Doe", /* ... */ },
  isLoading: false,
  error: null,
});
```

#### RefereeTeam.test.tsx
**Component Uses:**
- `useGetNgbTeamsQuery({ ngb, skipPaging })`

**Tests to Implement:**
1. Renders component with no teams selected
2. Renders selected teams when provided
3. Displays teams from primary NGB
4. Displays teams from secondary NGB
5. Handles team selection

**Implementation Approach:**
Similar to RefereeHeader - mock the RTK Query hooks and provide test data.

## Recommendations

### Option 1: Complete Now
Implement RTK Query mocks for both components following the pattern established in TestsTable and TestEditModal tests.

**Pros:**
- Complete test coverage
- Consistent mocking approach
- All tests passing

**Cons:**
- More complex than other components
- Requires understanding component behavior
- Time investment

### Option 2: Document and Defer
Leave tests in current state (restored but failing) with clear documentation of what needs to be done.

**Pros:**
- Quick closure
- Clear path forward documented
- Can be picked up later with fresh context

**Cons:**
- Test suite shows failures
- Incomplete coverage

## Current Test Metrics

**Before this PR:**
- 8 passing, 22 failing (26.7% pass rate)

**After current work:**
- 32 passing, 3 skipped, 2 files failing
- Improvement: +300% in passing tests
- All working tests use minimal mocking
- Tests validate behavior, not implementation

## Quality Improvements Made

1. **Fixed overlapping assertions** - No more redundant text checks
2. **Removed brittle selectors** - Using behavior-based queries
3. **Consolidated duplicate tests** - More focused, less redundant
4. **Added mutation validation** - Tests verify actual data being sent
5. **Improved callback verification** - Exact value checks, not vague ranges
6. **Focus on behavior** - Tests validate user-visible behavior
