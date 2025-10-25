/**
 * Helper utilities for mocking RTK Query hooks in tests
 * 
 * Usage:
 * 
 * // In your test file, before imports:
 * jest.mock('../../../store/serviceApi', () => ({
 *   ...jest.requireActual('../../../store/serviceApi'),
 *   useGetLanguagesQuery: jest.fn(),
 *   useGetAllTestsQuery: jest.fn(),
 * }));
 * 
 * // In your test:
 * import { useGetLanguagesQuery } from '../../../store/serviceApi';
 * 
 * (useGetLanguagesQuery as jest.Mock).mockReturnValue({
 *   data: mockLanguages,
 *   isLoading: false,
 *   isError: false,
 *   isSuccess: true,
 * });
 */

/**
 * Creates a successful RTK Query response
 */
export const createQuerySuccess = <T>(data: T) => ({
  data,
  isLoading: false,
  isError: false,
  isSuccess: true,
  isFetching: false,
  currentData: data,
  refetch: jest.fn(),
});

/**
 * Creates a loading RTK Query response
 */
export const createQueryLoading = () => ({
  data: undefined,
  isLoading: true,
  isError: false,
  isSuccess: false,
  isFetching: true,
  currentData: undefined,
  refetch: jest.fn(),
});

/**
 * Creates an error RTK Query response
 */
export const createQueryError = (error: any = { status: 500, data: 'Error' }) => ({
  data: undefined,
  isLoading: false,
  isError: true,
  isSuccess: false,
  isFetching: false,
  error,
  currentData: undefined,
  refetch: jest.fn(),
});

/**
 * Creates a successful RTK Query mutation response
 */
export const createMutationSuccess = () => {
  const mutationFn = jest.fn().mockResolvedValue({ data: {} });
  return [
    mutationFn,
    {
      isLoading: false,
      isError: false,
      isSuccess: true,
      reset: jest.fn(),
    },
  ] as const;
};

/**
 * Creates an RTK Query mutation response
 */
export const createMutation = () => {
  const mutationFn = jest.fn();
  return [
    mutationFn,
    {
      isLoading: false,
      isError: false,
      isSuccess: false,
      reset: jest.fn(),
    },
  ] as const;
};

