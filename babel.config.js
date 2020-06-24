/* eslint-disable global-require */
/* eslint-disable space-before-function-paren */
/* eslint-disable operator-linebreak */
/* eslint-disable prefer-template */
/* eslint-disable no-var */
/* eslint-disable func-names */
module.exports = function(api) {
  var validEnv = ['development', 'test', 'production']
  var currentEnv = api.env()
  var isDevelopmentEnv = api.env('development')
  var isProductionEnv = api.env('production')
  var isTestEnv = api.env('test')

  if (!validEnv.includes(currentEnv)) {
    throw new Error(
      'Please specify a valid `NODE_ENV` or ' +
        '`BABEL_ENV` environment variables. Valid values are "development", ' +
        '"test", and "production". Instead, received: ' +
        JSON.stringify(currentEnv) +
        '.'
    )
  }

  return {
    presets: [
      isTestEnv && [
        '@babel/preset-env',
        {
          targets: {
            node: 'current'
          },
          modules: 'commonjs'
        },
        '@babel/preset-react'
      ],
      (isProductionEnv || isDevelopmentEnv) && [
        '@babel/preset-env',
        {
          forceAllTransforms: true,
          useBuiltIns: 'entry',
          corejs: 2,
          modules: false,
          exclude: ['transform-typeof-symbol']
        }
      ],
      [
        '@babel/preset-react',
        {
          development: isDevelopmentEnv || isTestEnv,
          useBuiltIns: true
        }
      ],
      ['@babel/preset-typescript', {
        isTSX: true,
        allExtensions: true,
      }],
    ].filter(Boolean),
    plugins: [
      'babel-plugin-macros',
      '@babel/plugin-syntax-dynamic-import',
      isTestEnv && 'babel-plugin-dynamic-import-node',
      '@babel/plugin-transform-destructuring',
      '@babel/plugin-proposal-export-default-from',
      '@babel/plugin-transform-typescript',
      '@babel/plugin-proposal-nullish-coalescing-operator',
      '@babel/plugin-proposal-optional-chaining',
      [
        '@babel/plugin-proposal-class-properties',
        {
          loose: true
        }
      ],
      [
        '@babel/plugin-proposal-object-rest-spread',
        {
          useBuiltIns: true
        }
      ],
      [
        '@babel/plugin-transform-runtime',
        {
          helpers: false,
          regenerator: true,
          corejs: false
        }
      ],
      [
        '@babel/plugin-transform-regenerator',
        {
          async: false
        }
      ],
      isProductionEnv && [
        'babel-plugin-transform-react-remove-prop-types',
        {
          removeImport: true
        }
      ],
      [require('babel-plugin-module-resolver').default, {
        root: ['./app'],
        alias: {
          assets: './assets'
        }
      }]
    ].filter(Boolean)
  }
}
