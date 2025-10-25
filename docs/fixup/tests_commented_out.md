# Commented Out Tests

## Overview
Tests that have been completely commented out during the migration from Redux to RTK Query.

## Files Affected

### 1. `app/pages/Test/Details.test.tsx`
- **Status**: All tests commented out (34 lines)
- **Original Tests**:
  - "renders the details"
  - "with an associated language" - "renders the correct language"
- **Issue**: Tests were for test details page but components may have changed during RTK Query migration
- **Fix Strategy**: 
  - Review the `Details.tsx` component to understand current implementation
  - Rewrite tests to use RTK Query hooks if component was migrated
  - Ensure tests properly mock the new data fetching approach

### 2. `app/pages/RefereeProfile/RefereeTeam/RefereeTeam.test.tsx`
- **Status**: All tests commented out (38 lines)
- **Original Tests**:
  - "renders the component"
- **Issue**: Component signature changed during migration, tests need updating
- **Fix Strategy**:
  - Review `RefereeTeam.tsx` component implementation
  - Update test data structure to match new component props
  - Rewrite assertions to match new component behavior

### 3. `app/pages/RefereeProfile/RefereeHeader/RefereeHeader.test.tsx`
- **Status**: All tests commented out (192 lines) 
- **Original Tests**: 15 tests covering:
  - Name rendering (default and with names)
  - Pronoun display
  - Certification display  
  - Bio display
  - Edit button functionality
  - Edit mode behavior (save button, certifications hidden, form fields)
  - Change event handling
- **Issue**: Large test suite completely disabled, likely due to RTK Query migration
- **Fix Strategy**:
  - High priority due to comprehensive coverage
  - Review component to identify which parts still use Redux vs RTK Query
  - Update mock store configuration for hybrid approach
  - Rewrite tests progressively, validating each section

### 4. `app/components/Counter/Counter.test.tsx`
- **Status**: Partially commented (1 describe block with 3 tests)
- **Commented Tests**:
  - `getDisplayColor` utility function tests (3 tests)
- **Active Tests**: 2 tests passing
- **Issue**: Utility function tests commented for unknown reason
- **Fix Strategy**:
  - Uncomment and verify if `getDisplayColor` still exists
  - If function was removed, remove tests
  - If function exists, fix any test issues

## Priority Order

1. **High**: `RefereeHeader.test.tsx` - Comprehensive test suite (192 lines)
2. **Medium**: `Details.test.tsx` - Small but important feature
3. **Medium**: `RefereeTeam.test.tsx` - Component rendering validation
4. **Low**: `Counter.test.tsx` - Only utility function tests commented

## Implementation Notes

- All commented tests need to be reviewed against current component implementations
- Tests should be updated to work with the hybrid Redux + RTK Query architecture
- Mock data should be validated against actual API responses from backend
- Consider simplifying tests where appropriate while maintaining coverage
