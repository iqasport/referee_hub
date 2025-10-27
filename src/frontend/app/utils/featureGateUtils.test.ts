import { renderHook } from '@testing-library/react';
import { useFeatureGates } from './featureGateUtils';
import { useGetCurrentUserFeatureGatesQuery } from '../store/serviceApi';

// Mock the RTK Query hook
jest.mock('../store/serviceApi', () => ({
  useGetCurrentUserFeatureGatesQuery: jest.fn(),
}));

const mockedUseGetCurrentUserFeatureGatesQuery = useGetCurrentUserFeatureGatesQuery as jest.MockedFunction<typeof useGetCurrentUserFeatureGatesQuery>;

describe('useFeatureGates', () => {
  const originalLocation = window.location;

  beforeEach(() => {
    // Clear the mock before each test
    mockedUseGetCurrentUserFeatureGatesQuery.mockClear();
    
    // Reset window.location using Object.defineProperty
    delete (window as any).location;
    window.location = { ...originalLocation, search: '' };
  });

  afterEach(() => {
    // Restore original location
    window.location = originalLocation;
  });

  it('should return backend values when no query parameters', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: true },
      isLoading: false,
      error: undefined,
    } as any);

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isTestFlag).toBe(true);
  });

  it('should override backend value with query parameter', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: false },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=isTestFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isTestFlag).toBe(true);
  });

  it('should set flag to false with ! prefix', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: true },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=!isTestFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isTestFlag).toBe(false);
  });

  it('should handle multiple flags in query parameter', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: false, isAnotherFlag: true },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=isTestFlag,!isAnotherFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isTestFlag).toBe(true);
    expect(result.current.isAnotherFlag).toBe(false);
  });

  it('should handle empty backend data', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=isTestFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isTestFlag).toBe(true);
  });

  it('should be case insensitive for query parameters', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: false },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=ISTESTFLAG';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isTestFlag).toBe(true);
  });

  it('should handle backend errors gracefully', () => {
    const mockError = { message: 'Test error' };
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: mockError,
    } as any);

    const { result } = renderHook(() => useFeatureGates());

    // Should return empty object on error (properties won't exist unless in query param)
    expect(result.current.isTestFlag).toBeUndefined();
  });
});
