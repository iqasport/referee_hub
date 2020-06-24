/* eslint-disable import/no-extraneous-dependencies */
process.env.NODE_ENV = process.env.NODE_ENV || 'development'

const path = require('path');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const environment = require('./environment')

environment.plugins.append(
  'ForkTsCheckerWebpackPlugin',
  new ForkTsCheckerWebpackPlugin({
    typescript: {
      tsconfig: path.resolve(__dirname, '../../tsconfig.json'),
    },
    async: false,
  })
);

module.exports = environment.toWebpackConfig()
