import { renderHook } from '@testing-library/react';
import { useFeatureGates } from './featureGateUtils';
import { useGetCurrentUserFeatureGatesQuery } from '../store/serviceApi';

// Mock the RTK Query hook
jest.mock('../store/serviceApi', () => ({
  useGetCurrentUserFeatureGatesQuery: jest.fn(),
}));

const mockedUseGetCurrentUserFeatureGatesQuery = useGetCurrentUserFeatureGatesQuery as jest.MockedFunction<typeof useGetCurrentUserFeatureGatesQuery>;

describe('useFeatureGates', () => {
  beforeEach(() => {
    // Clear the mock before each test
    mockedUseGetCurrentUserFeatureGatesQuery.mockClear();
    
    // Reset window.location
    delete (window as any).location;
    window.location = { search: '' } as Location;
  });

  it('should return backend values when no query parameters', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: true },
      isLoading: false,
      error: undefined,
    } as any);

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.featureGates.isTestFlag).toBe(true);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeUndefined();
  });

  it('should override backend value with query parameter', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: false },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=testFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.featureGates.isTestFlag).toBe(true);
  });

  it('should set flag to false with ! prefix', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: true },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=!testFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.featureGates.isTestFlag).toBe(false);
  });

  it('should handle multiple flags in query parameter', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: { isTestFlag: false, isAnotherFlag: true },
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=testFlag,!anotherFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.featureGates.isTestFlag).toBe(true);
    expect(result.current.featureGates.isAnotherFlag).toBe(false);
  });

  it('should handle empty backend data', () => {
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
    } as any);

    window.location.search = '?features=testFlag';

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.featureGates.isTestFlag).toBe(true);
  });

  it('should pass through loading and error states', () => {
    const mockError = { message: 'Test error' };
    mockedUseGetCurrentUserFeatureGatesQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: mockError,
    } as any);

    const { result } = renderHook(() => useFeatureGates());

    expect(result.current.isLoading).toBe(true);
    expect(result.current.error).toEqual(mockError);
  });
});
