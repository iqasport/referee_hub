const path = require('path');
const Dotenv = require('dotenv-webpack');

module.exports = {
  entry: './app/index.tsx',
  mode: 'production',
  plugins: [
    new Dotenv({
      systemvars: true,
      path: './.env.prod',
    }),
  ],
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    filename: 'management_hub_app.js',
    path: path.resolve(__dirname, 'dist'),
    clean: true,
  },
};