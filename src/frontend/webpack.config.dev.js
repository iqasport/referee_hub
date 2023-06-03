const path = require('path');

module.exports = {
  entry: './app/index.tsx',
  devtool: 'inline-source-map',
  mode: 'development',
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
    filename: 'management_hub.js',
    path: path.resolve(__dirname, 'dist'),
    clean: true,
  },
};