import '@testing-library/jest-dom'
import '@testing-library/jest-dom/extend-expect'

jest.setTimeout(10000)

// Mock react-markdown since it's ESM-only and causes issues with Jest
jest.mock('react-markdown', () => {
  return {
    __esModule: true,
    default: ({ children }: { children: string }) => {
      // Simple mock that returns children directly for testing
      return children;
    },
  };
});

// Mock remark-gfm plugin
jest.mock('remark-gfm', () => {
  return {
    __esModule: true,
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    default: () => {},
  };
});
