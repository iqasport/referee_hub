const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
  entry: './app/index.tsx',
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
  plugins: [
    new HtmlWebpackPlugin({
      title: 'IQA Management Hub',
      template: 'app/index.html',
      inject: false,
    })
  ],
  output: {
    chunkFilename: "[name]-[contenthash:12].js",
    filename: '[name]-[contenthash:6].js',
    path: path.resolve(__dirname, 'dist'),
    clean: true,
  },
  optimization: {
    splitChunks: {
      cacheGroups: {
        app: {
          test: /[\\/]app[\\/]/,
          name: 'management_hub_app',
        },
        reactVendor: {
          test: /[\\/]node_modules[\\/](react[^\\/]*)[\\/]/,
          name: 'react',
          chunks: 'all',
        },
        reduxVendor: {
          test: /[\\/]node_modules[\\/](\@reduxjs[^\\/]*|redux[^\\/])[\\/]/,
          name: 'redux',
          chunks: 'all',
        },
        fontawesomeVendor: {
          test: /[\\/]node_modules[\\/](\@fortawesome[^\\/]*)[\\/]/,
          name: 'awesome',
          chunks: 'all',
        },
      },
    },
  },
};