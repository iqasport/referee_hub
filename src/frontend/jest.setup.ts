import '@testing-library/jest-dom'
import '@testing-library/jest-dom/extend-expect'

jest.setTimeout(10000)

// Mock react-markdown since it's ESM-only and causes issues with Jest
jest.mock('react-markdown', () => {
  return {
    __esModule: true,
    default: ({ children }: { children: string }) => {
      // Simple mock that renders children in a div to simulate markdown rendering
      return children;
    },
  };
});
