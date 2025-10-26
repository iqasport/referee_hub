# Working Tests - Improvement Plan

## Overview
These tests are currently passing but have minor issues that should be addressed for code quality and maintainability.

## Test Files
1. `app/components/Counter/Counter.test.tsx` (2 passing, 1 commented)
2. `app/components/Avatar/Avatar.test.tsx` (1 passing)

## Current State

### Counter.test.tsx
**Status**: 2 tests passing, 1 test block commented out

**Passing Tests**:
- ✅ it renders a counter
- ✅ it handles a second tick

**Commented Out Tests**:
- `describe("getDisplayColor")` - commented out block testing utility function

**Linting Issues**:
1. Line 5: `'getDisplayColor' is defined but never used` - imported but not tested
2. Line 25: `Prefer .toBeInTheDocument() for asserting DOM node existence` 
3. Line 35: `Prefer .toBeInTheDocument() for asserting DOM node existence`
4. Line 58: `Some tests seem to be commented`

**Component Status**: Simple utility component, no RTK Query dependencies

### Avatar.test.tsx
**Status**: 1 test passing

**Passing Test**:
- ✅ it is rendered

**Linting Issues**:
1. Line 17: `Test has no assertions` - The test calls `screen.getByText("QR")` but doesn't assert anything

**Component Status**: Simple presentation component, no RTK Query dependencies

## Issues to Address

### Counter Tests
1. **Commented Code**: Tests for `getDisplayColor` utility function are commented out
2. **Linting**: Using `.toBeDefined()` instead of `.toBeInTheDocument()`
3. **Unused Import**: `getDisplayColor` imported but not used (because tests are commented)

### Avatar Tests
1. **Missing Assertion**: Test doesn't have explicit assertion
2. **Minimal Coverage**: Only one basic test exists

## Improvement Strategy

### Counter.test.tsx

#### Issue 1: Commented Out Tests
**Decision**: Uncomment and fix the tests
- The `getDisplayColor` function appears to be a legitimate utility that should be tested
- Tests are simple and straightforward
- No reason to skip them

**Action**:
```typescript
describe("getDisplayColor", () => {
  const limit = 10;

  test("it returns grey when in the goodLimit", () => {
    const minutes = 5;
    const expectedColor = "grey";
    const actual = getDisplayColor(limit, minutes);

    expect(actual).toEqual(expectedColor);
  });

  test("it returns yellow when in the warning limit", () => {
    const minutes = 8;
    const expectedColor = "yellow";
    const actual = getDisplayColor(limit, minutes);

    expect(actual).toEqual(expectedColor);
  });

  test("it returns red when at the limit", () => {
    const minutes = 10;
    const expectedColor = "red";
    const actual = getDisplayColor(limit, minutes);

    expect(actual).toEqual(expectedColor);
  });
});
```

#### Issue 2: Linting Issues
**Action**: Replace `.toBeDefined()` with `.toBeInTheDocument()`
```typescript
// Before
expect(screen.getByText("00:00")).toBeDefined();

// After
expect(screen.getByText("00:00")).toBeInTheDocument();
```

### Avatar.test.tsx

#### Issue 1: Missing Assertion
**Decision**: Add explicit assertion

The current test:
```typescript
test("it is rendered", () => {
  render(<Avatar {...defaultProps} />);
  screen.getByText("QR");
});
```

`screen.getByText()` throws if element not found, so test is technically valid but implicit.

**Action**: Make assertion explicit
```typescript
test("it is rendered", () => {
  render(<Avatar {...defaultProps} />);
  expect(screen.getByText("QR")).toBeInTheDocument();
});
```

#### Issue 2: Minimal Coverage
**Decision**: Add more test cases if component has meaningful logic

Need to review Avatar component to see if there's more to test:
- Different prop combinations
- Edge cases
- Conditional rendering

## Implementation Plan

### Phase 1: Fix Linting Issues
1. Counter: Replace `.toBeDefined()` with `.toBeInTheDocument()`
2. Avatar: Add explicit assertion with `.toBeInTheDocument()`

### Phase 2: Uncomment Counter Tests
1. Verify `getDisplayColor` function exists and is exported
2. Uncomment test block
3. Fix any issues with the tests
4. Ensure all assertions pass

### Phase 3: Review Component Logic
1. Check Avatar component for additional test scenarios
2. Check Counter component for edge cases
3. Add tests for any uncovered logic

### Phase 4: Run and Validate
1. Run tests to ensure all pass
2. Run linter to verify issues are resolved
3. Check code coverage to ensure good coverage of non-trivial logic

## Expected Outcome

### Counter.test.tsx
- All tests passing (5 total)
- No commented code
- No linting issues
- Good coverage of utility functions

### Avatar.test.tsx
- All tests passing (1-3 tests depending on component logic)
- Explicit assertions
- No linting issues
- Coverage of key rendering scenarios

## Notes
These tests are the easiest to fix and should be completed first to build momentum. They require no RTK Query knowledge or complex mocking.
