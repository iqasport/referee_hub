module.exports = {
  testPathIgnorePatterns: ['<rootDir>/config'],
  moduleDirectories: ['node_modules', './', './utils'],
  moduleFileExtensions: ['js', 'jsx', 'json', 'node', 'ts', 'tsx'],
  testRegex: '(/__tests__/.*|(\\.|/)(test))\\.[jt]sx?$',
  testEnvironment: 'jest-environment-jsdom-fourteen',
  collectCoverage: true,
  coverageReporters: ['json'],
  reporters: ['default', 'jest-junit'],
  transform: {
    '^.+\\.(js|jsx|ts|tsx)$': 'ts-jest'
  },
  preset: 'ts-jest/presets/js-with-babel',
}
