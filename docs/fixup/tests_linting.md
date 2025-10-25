# Linting Issues

## Overview
ESLint errors and warnings found in test files and related production code.

## Test File Issues

### 1. `app/components/Avatar/Avatar.test.tsx`
- **Warning**: `jest/expect-expect` - Test has no assertions (line 17)
- **Issue**: Test calls `screen.getByText("QR")` but doesn't assert anything
- **Fix**: Add explicit expect statement:
  ```tsx
  expect(screen.getByText("QR")).toBeInTheDocument();
  ```

### 2. `app/components/Counter/Counter.test.tsx`
- **Warning**: `@typescript-eslint/no-unused-vars` - 'getDisplayColor' imported but never used (line 5)
- **Error**: `jest-dom/prefer-in-document` - Use `.toBeInTheDocument()` instead of `.toBeDefined()` (lines 25, 35)
- **Warning**: `jest/no-commented-out-tests` - Some tests seem to be commented (line 58)
- **Fixes**:
  - Remove unused import or use it
  - Replace `.toBeDefined()` with `.toBeInTheDocument()`:
    ```tsx
    // Before
    expect(screen.getByText("00:00")).toBeDefined();
    
    // After
    expect(screen.getByText("00:00")).toBeInTheDocument();
    ```
  - Address commented out tests (covered in tests_commented_out.md)

## Production Code Issues Affecting Tests

### ESLint Errors in API Files

Multiple API files have unnecessary try/catch wrappers that should be removed:

#### `app/apis/certification.ts` (3 errors)
- Lines: 28, 44, 63
- Pattern: Try/catch that just re-throws

#### `app/apis/checkout.ts` (2 errors)  
- Lines: 20, 34

#### `app/apis/job.ts` (4 errors)
- Lines: 23, 42, 60, 74

#### `app/apis/language.ts` (1 error)
- Line: 12

#### `app/apis/nationalGoverningBody.ts` (5 errors)
- Lines: 80, 96, 107, 124, 143

#### `app/apis/question.ts` (3 errors)
- Lines: 26, 45, 59

#### `app/apis/referee.ts` (3 errors)
- Lines: 140, 161, 178

#### `app/apis/single_test.ts` (8 errors)
- Lines: 31, 50, 72, 91, 122, 142, 157, 176

#### `app/apis/team.ts` (7 errors)
- Lines: 48, 64, 87, 107, 127, 144, 164

#### `app/apis/user.ts` (4 errors)
- Lines: 49, 60, 71, 85

**Pattern Example**:
```tsx
// Bad
try {
  return await apiCall();
} catch (error) {
  throw error;
}

// Good  
return await apiCall();
```

### Other Production Code Issues

#### `app/apis/utils.ts`
- **Warning**: Line 7 - `@typescript-eslint/no-explicit-any` - Unexpected any type
- **Fix**: Define proper type for the parameter

#### `app/components/Avatar/Avatar.tsx`
- **Warning**: Line 1 - 'axios' imported but never used
- **Warning**: Line 68 - 'invite' assigned but never used
- **Fixes**: Remove unused imports/variables

#### `app/components/Counter/Counter.tsx`
- **Warning**: Lines 46, 48 - `@typescript-eslint/no-explicit-any` - Unexpected any types
- **Fix**: Define proper types for setTimeout/clearTimeout

#### `app/components/DragNDrop/DragNDrop.tsx`
- **Error**: Line 32 (2x) - `react/no-unescaped-entities` - Escape apostrophes
- **Fix**: Use `&apos;` or `&#39;` for apostrophes in JSX

#### `app/components/modals/ExportModal/ExportModal.tsx`
- **Error**: Lines 44, 45 - `react/no-unescaped-entities` - Escape apostrophes

#### `app/components/modals/NgbAdminsModal/NgbAdminsModal.tsx`
- **Error**: Line 32 - `react/no-unescaped-entities` - Escape apostrophes

#### `app/components/modals/NgbEditModal/NgbEditModal.tsx`
- **Error**: Line 236 (2x) - `react/no-unescaped-entities` - Escape apostrophes

## Priority

### Critical (blocking tests)
- None - linting issues don't block test execution, but should be fixed for code quality

### High (test quality)
1. Fix test assertions in Counter.test.tsx (prefer-in-document)
2. Add assertion to Avatar.test.tsx

### Medium (code quality)
1. Remove unnecessary try/catch wrappers in API files
2. Fix TypeScript any types
3. Remove unused imports/variables

### Low (style)
1. Fix unescaped entities in JSX

## Implementation Strategy

1. Fix test-specific linting issues first (Avatar, Counter tests)
2. Run tests to ensure they still pass
3. Fix production code linting issues in batches:
   - Remove unnecessary try/catch wrappers
   - Fix TypeScript types
   - Fix JSX entities
   - Remove unused code
4. Run full lint + test suite to verify

## Automation Opportunity

Many of these issues can be auto-fixed:
```bash
yarn lint --fix
```

This will automatically fix:
- Unused imports/variables
- Some TypeScript issues
- Some formatting issues

Manual fixes needed for:
- Unnecessary try/catch blocks
- TypeScript any types
- Test assertions
