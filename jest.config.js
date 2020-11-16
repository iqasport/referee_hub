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
  coverageReporters: ['json', 'text'],
  coveragePathIgnorePatterns: [
    '/node_modules/',
    '/app/javascript/MainApp/schemas/',
    '/app/javascript/MainApp/factories/',
    '/app/javascript/MainApp/rootReducer',
    '/app/javascript/MainApp/store'
  ],
  reporters: ['default'],
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
