module.exports = {
  roots: ['<rootDir>'],
  testPathIgnorePatterns: ['<rootDir>/config'],
  moduleDirectories: [
    'node_modules',
    './',
    './utils',
    './app/javascript/MainApp'
  ],
  moduleFileExtensions: ['js', 'jsx', 'json', 'node', 'ts', 'tsx'],
  testRegex: '(/__tests__/.*|(\\.|/)(test))\\.[jt]sx?$',
  testEnvironment: 'jsdom',
  collectCoverage: true,
  coverageReporters: ['json'],
  reporters: ['default', 'jest-junit'],
  transform: {
    '^.+\\.(ts|tsx)$': 'babel-jest',
  },
  setupFilesAfterEnv: ['./jest.setup.ts'],
  // coverageThreshold: {
  //   global: {
  //     lines: 90,
  //     statements: 90
  //   }
  // }
}
