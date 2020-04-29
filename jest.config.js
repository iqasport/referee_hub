const { defaults: tsjPreset } = require('ts-jest/presets')

module.exports = {
  testPathIgnorePatterns: ['<rootDir>/config'],
  moduleDirectories: ['node_modules', './'],
  moduleFileExtensions: ['js', 'jsx', 'json', 'node', 'ts', 'tsx'],
  testRegex: '(/__tests__/.*|(\\.|/)(test))\\.[jt]sx?$',
  testEnvironment: 'jest-environment-jsdom-fourteen',
  collectCoverage: true,
  coverageReporters: ['json'],
  transform: {
    ...tsjPreset.transform,
  },
  reporters: ['default', 'jest-junit'],
}
