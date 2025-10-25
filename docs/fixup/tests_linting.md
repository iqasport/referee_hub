# Test Linting Issues - Improvement Plan

## Overview
Address linting issues in test files only, as requested. The codebase has 837 total linting problems, but we'll focus only on test files.

## Test File Linting Issues

### Counter.test.tsx
```
Line 5:   'getDisplayColor' is defined but never used (unused import)
Line 25:  Prefer .toBeInTheDocument() for asserting DOM node existence
Line 35:  Prefer .toBeInTheDocument() for asserting DOM node existence
Line 58:  Some tests seem to be commented
```

**Fix Strategy**:
1. Will be resolved when uncommenting getDisplayColor tests
2. Replace `.toBeDefined()` with `.toBeInTheDocument()`
3. Replace `.toBeDefined()` with `.toBeInTheDocument()`
4. Will be resolved when uncommenting tests

### Avatar.test.tsx
```
Line 17:  Test has no assertions (jest/expect-expect)
```

**Fix Strategy**:
1. Add explicit `expect()` assertion

### RefereeHeader.test.tsx
```
Line 1:  'React' is defined but never used
Line 2:  'fireEvent' is defined but never used
Line 4:  'UpdateRefereeRequest' is defined but never used
Line 5:  'DataAttributes' is defined but never used
Line 5:  'IncludedAttributes' is defined but never used
Line 8:  Some tests seem to be commented
```

**Fix Strategy**:
1. These are in commented-out code, will be addressed when rewriting tests
2. Remove unused imports after uncommenting and rewriting tests

### RefereeTeam.test.tsx
```
Line 1:  'React' is defined but never used
Line 6:  'AssociationData' is defined but never used
Line 8:  Some tests seem to be commented
```

**Fix Strategy**:
1. Will be addressed when rewriting tests
2. Remove unused imports after uncommenting

### Details.test.tsx
```
Line 1:  'React' is defined but never used
Line 7:  'DetailsProps' is defined but never used
Line 9:  Some tests seem to be commented
```

**Fix Strategy**:
1. Will be addressed when rewriting tests

### TestsTable.test.tsx
```
Line 37:  Test has no assertions (jest/expect-expect)
Line 45:  Test has no assertions (jest/expect-expect)
```

**Fix Strategy**:
1. These tests use implicit assertions via `screen.getAll*` which throws on failure
2. Add explicit `expect()` assertions for clarity

## Implementation Plan

### Phase 1: Working Tests (Quick Wins)
1. **Counter.test.tsx**
   - Uncomment getDisplayColor tests (fixes unused import warning)
   - Replace `.toBeDefined()` with `.toBeInTheDocument()` in 2 places

2. **Avatar.test.tsx**
   - Add explicit `expect(...).toBeInTheDocument()` assertion

### Phase 2: Legacy Redux Tests
1. **TestsTable.test.tsx**
   - Add explicit assertions to tests (when rewriting for RTK Query)
   - Remove or fix tests as part of migration

### Phase 3: Commented Out Tests
1. **RefereeHeader.test.tsx**
   - Remove unused imports when uncommenting and rewriting
   - Add proper imports for rewritten tests

2. **RefereeTeam.test.tsx**
   - Remove unused imports when uncommenting and rewriting

3. **Details.test.tsx**
   - Remove unused imports when uncommenting and rewriting

## Linting Rules to Follow

### jest/expect-expect
- Every test must have an explicit `expect()` call
- Using `getBy*` queries is not sufficient
- Add `expect(screen.getByText(...)).toBeInTheDocument()`

### jest-dom/prefer-in-document
- Use `.toBeInTheDocument()` instead of `.toBeDefined()` for DOM elements
- More specific and clearer intent

### jest/no-commented-out-tests
- Don't leave tests commented out
- Either delete them or uncomment and fix them

### @typescript-eslint/no-unused-vars
- Remove unused imports
- Clean up after refactoring

## Expected Outcome
- Zero linting errors in test files
- Zero linting warnings in test files
- All tests follow best practices
- Clear, explicit assertions in all tests

## Notes
- Some linting issues will auto-resolve as part of test rewrites
- Focus on explicit assertions over implicit ones
- Keep test code clean and maintainable
- Skip linting issues in legacy redux store files (as requested)
