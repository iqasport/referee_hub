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
  "ignorePatterns": ["*.js"],
  "settings": {
    "react": {
      "version": "detect"
    }
  },
  "overrides": [
    {
      "files": ["app/schemas/**/*.ts"],
      "rules": {
        "@typescript-eslint/no-explicit-any": "off"
      }
    },
    {
      "files": ["app/store/serviceApi.ts"],
      "rules": {
        "@typescript-eslint/no-explicit-any": "off",
        "@typescript-eslint/ban-types": "off"
      }
    },
    {
      "files": ["**/*.test.ts", "**/*.test.tsx"],
      "rules": {
        "testing-library/no-node-access": "warn",
        "testing-library/no-container": "warn",
        "testing-library/no-wait-for-multiple-assertions": "warn"
      }
    }
  ]
};