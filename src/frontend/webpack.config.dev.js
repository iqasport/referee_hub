const path = require('path');
const Dotenv = require('dotenv-webpack');
const { merge } = require('webpack-merge');
const common = require('./webpack.config.common.js');

module.exports = merge(common, {
  devtool: 'inline-source-map',
  mode: 'development',
  plugins: [
    new Dotenv(),
  ],
  devServer: {
    // Write files to disk so backend can serve them
    devMiddleware: {
      writeToDisk: true,
    },
    // Enable Hot Module Replacement
    hot: true,
    // Disable live reload (HMR will handle updates)
    liveReload: false,
    // Watch files for changes
    watchFiles: {
      paths: ['app/**/*', 'assets/**/*'],
      options: {
        usePolling: false, // Use native file system events for better performance
      },
    },
    // Client configuration for better error reporting
    client: {
      overlay: {
        errors: true,
        warnings: false,
      },
      // WebSocket URL configuration - explicitly point to webpack dev server port
      webSocketURL: {
        hostname: 'localhost',
        pathname: '/ws',
        port: 8080,
        protocol: 'ws',
      },
    },
    // Allow access from any host (useful for development)
    allowedHosts: 'all',
    // Enable compression
    compress: true,
    // Headers for CORS during development
    headers: {
      'Access-Control-Allow-Origin': '*',
    },
    // Static files configuration
    static: {
      directory: path.join(__dirname, 'dist'),
      watch: true,
    },
  },
});