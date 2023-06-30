const Dotenv = require('dotenv-webpack');
const { merge } = require('webpack-merge');
const common = require('./webpack.config.common.js');

module.exports = merge(common, {
  entry: './app/index.tsx',
  mode: 'production',
  plugins: [
    new Dotenv({
      systemvars: true,
      path: './.env.prod',
    }),
  ],
});