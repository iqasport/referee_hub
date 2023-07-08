/* eslint-env node */
module.exports = {
  extends: [
    'eslint:recommended',
    "plugin:react/recommended",
    "plugin:jest/recommended",
    "plugin:jest/style",
    "plugin:jest-dom/recommended",
    "plugin:testing-library/react",
    'plugin:@typescript-eslint/recommended'
  ],
  parser: '@typescript-eslint/parser',
  plugins: ['@typescript-eslint'],
  root: true,
};