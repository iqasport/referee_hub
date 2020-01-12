module.exports = {
  testPathIgnorePatterns: ['<rootDir>/config'],
  moduleDirectories: ['node_modules', './'],
  moduleFileExtensions: ['js', 'jsx', 'json', 'node'],
  testRegex: '(/__tests__/.*|(\\.|/)(test))\\.(jsx?)$',
  testEnvironment: 'jest-environment-jsdom-fourteen',
  collectCoverage: true,
  coverageReporters: ['json']
}
