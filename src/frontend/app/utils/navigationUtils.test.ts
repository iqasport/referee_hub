import { renderHook } from '@testing-library/react';
import { useNavigate } from './navigationUtils';
import { useNavigate as routerUseNavigate } from 'react-router-dom';

// Mock react-router-dom
jest.mock('react-router-dom', () => ({
  useNavigate: jest.fn(),
  useParams: jest.fn(),
}));

const mockedRouterUseNavigate = routerUseNavigate as jest.MockedFunction<typeof routerUseNavigate>;

describe('useNavigate', () => {
  const originalLocation = window.location;
  let mockNavigate: jest.Mock;

  beforeEach(() => {
    // Create a fresh mock navigate function
    mockNavigate = jest.fn();
    mockedRouterUseNavigate.mockReturnValue(mockNavigate);
    
    // Reset window.location
    delete (window as any).location;
    window.location = { ...originalLocation, search: '' };
  });

  afterEach(() => {
    // Restore original location
    window.location = originalLocation;
    jest.clearAllMocks();
  });

  describe('impersonate parameter propagation', () => {
    it('should propagate impersonate param when navigating with string URL without query params', () => {
      window.location.search = '?impersonate=123';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test?impersonate=123', {});
    });

    it('should propagate impersonate param when navigating with string URL with existing query params', () => {
      window.location.search = '?impersonate=123';

      const { result } = renderHook(() => useNavigate());
      result.current('/test?foo=bar');

      expect(mockNavigate).toHaveBeenCalledWith('/test?foo=bar&impersonate=123', {});
    });

    it('should propagate impersonate param when navigating with object URL', () => {
      window.location.search = '?impersonate=123';

      const { result } = renderHook(() => useNavigate());
      result.current({ pathname: '/test' });

      expect(mockNavigate).toHaveBeenCalledWith(
        expect.objectContaining({
          pathname: '/test',
          search: '?impersonate=123',
        }),
        {}
      );
    });

    it('should not add impersonate param when not present in current URL', () => {
      window.location.search = '';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test', {});
    });

    it('should handle numeric navigation (back/forward)', () => {
      window.location.search = '?impersonate=123';

      const { result } = renderHook(() => useNavigate());
      result.current(-1);

      expect(mockNavigate).toHaveBeenCalledWith(-1, {});
    });
  });

  describe('features parameter propagation', () => {
    it('should propagate features param when navigating with string URL without query params', () => {
      window.location.search = '?features=isTestFlag';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test?features=isTestFlag', {});
    });

    it('should propagate features param when navigating with string URL with existing query params', () => {
      window.location.search = '?features=isTestFlag,!isOtherFlag';

      const { result } = renderHook(() => useNavigate());
      result.current('/test?foo=bar');

      expect(mockNavigate).toHaveBeenCalledWith('/test?foo=bar&features=isTestFlag,!isOtherFlag', {});
    });

    it('should propagate features param when navigating with object URL', () => {
      window.location.search = '?features=isTestFlag';

      const { result } = renderHook(() => useNavigate());
      result.current({ pathname: '/test' });

      expect(mockNavigate).toHaveBeenCalledWith(
        expect.objectContaining({
          pathname: '/test',
          search: '?features=isTestFlag',
        }),
        {}
      );
    });

    it('should not add features param when not present in current URL', () => {
      window.location.search = '';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test', {});
    });
  });

  describe('both impersonate and features parameter propagation', () => {
    it('should propagate both impersonate and features params with string URL', () => {
      window.location.search = '?impersonate=123&features=isTestFlag';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test?impersonate=123&features=isTestFlag', {});
    });

    it('should propagate both params when navigating with string URL with existing query params', () => {
      window.location.search = '?impersonate=123&features=isTestFlag';

      const { result } = renderHook(() => useNavigate());
      result.current('/test?foo=bar');

      expect(mockNavigate).toHaveBeenCalledWith('/test?foo=bar&impersonate=123&features=isTestFlag', {});
    });

    it('should propagate both params when navigating with object URL', () => {
      window.location.search = '?impersonate=123&features=isTestFlag';

      const { result } = renderHook(() => useNavigate());
      result.current({ pathname: '/test' });

      expect(mockNavigate).toHaveBeenCalledWith(
        expect.objectContaining({
          pathname: '/test',
          search: '?impersonate=123&features=isTestFlag',
        }),
        {}
      );
    });

    it('should preserve order: impersonate first, then features', () => {
      window.location.search = '?features=isTestFlag&impersonate=123';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      // Impersonate should come before features
      expect(mockNavigate).toHaveBeenCalledWith('/test?impersonate=123&features=isTestFlag', {});
    });
  });

  describe('edge cases', () => {
    it('should handle object URL with existing search params', () => {
      window.location.search = '?impersonate=123&features=isTestFlag';

      const { result } = renderHook(() => useNavigate());
      result.current({ pathname: '/test', search: '?foo=bar' });

      expect(mockNavigate).toHaveBeenCalledWith(
        expect.objectContaining({
          pathname: '/test',
          search: '?foo=bar&impersonate=123&features=isTestFlag',
        }),
        {}
      );
    });

    it('should handle NavigateOptions parameter', () => {
      window.location.search = '?impersonate=123';

      const { result } = renderHook(() => useNavigate());
      result.current('/test', { replace: true });

      expect(mockNavigate).toHaveBeenCalledWith('/test?impersonate=123', { replace: true });
    });

    it('should handle empty string search params', () => {
      window.location.search = '';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test', {});
    });

    it('should handle only impersonate param in current URL', () => {
      window.location.search = '?impersonate=456';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test?impersonate=456', {});
    });

    it('should handle only features param in current URL', () => {
      window.location.search = '?features=isFeatureA,isFeatureB';

      const { result } = renderHook(() => useNavigate());
      result.current('/test');

      expect(mockNavigate).toHaveBeenCalledWith('/test?features=isFeatureA,isFeatureB', {});
    });
  });
});
