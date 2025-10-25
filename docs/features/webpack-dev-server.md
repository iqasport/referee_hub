# Webpack Dev Server Design Document

## Overview

This document outlines the design and implementation of a development server for the IQA Management Hub frontend that enables automatic rebuilding and hot reloading of changes without requiring manual page refreshes.

## Current State

### Existing Build Process

The current development workflow requires:
1. Running `yarn build:dev` to perform a full rebuild
2. Manually refreshing the browser to see changes
3. Waiting for the complete rebuild process each time

### Technology Stack

- **Webpack 5.94.0**: Module bundler
- **webpack-dev-server 5.2.1**: Already installed as a dev dependency
- **React 18.2**: Frontend framework
- **TypeScript 5.1.3**: Type-safe JavaScript
- **ASP.NET Core**: Backend server that serves static files

### Backend Integration

The backend (.NET) serves frontend assets from the `src/frontend/dist/` directory:
- In development mode, the WebRoot is configured as `../../frontend/dist` (relative to the backend project)
- The backend uses `app.UseStaticFiles()` to serve static files
- The backend has a fallback route `endpoints.MapFallbackToFile("index.html")` for SPA routing

## Problem Statement

Developers need a faster feedback loop when making frontend changes. The current workflow is inefficient because:
1. Full rebuilds take significant time
2. Manual page refreshes are required
3. Application state is lost on each refresh
4. No incremental compilation

## Design Goals

1. **Automatic Rebuilds**: Detect file changes and rebuild only what's necessary
2. **Hot Module Replacement (HMR)**: Update modules in the browser without full page reload
3. **Backend Integration**: Backend must continue serving the frontend (no separate port for frontend)
4. **State Preservation**: Maintain React component state where possible during updates
5. **Developer Experience**: Simple commands to start the dev environment

## Solution Architecture

### Option Analysis

#### Option 1: Webpack Dev Server with writeToDisk (RECOMMENDED)

**Approach**: Run webpack-dev-server in watch mode with `devServer.devMiddleware.writeToDisk: true`

**Pros**:
- Writes compiled files to disk so backend can serve them
- Supports Hot Module Replacement (HMR)
- Fast incremental rebuilds
- Simple integration with existing backend
- No CORS issues

**Cons**:
- Slightly more disk I/O than pure in-memory approach
- Need to handle file watching carefully

**Implementation**:
```javascript
// webpack.config.dev.js
module.exports = {
  devServer: {
    devMiddleware: {
      writeToDisk: true,  // Write files to disk for backend to serve
    },
    hot: true,            // Enable HMR
    liveReload: false,    // Disable full page reload (HMR handles updates)
    client: {
      overlay: true,      // Show compilation errors in browser
    },
    static: {
      directory: path.join(__dirname, 'dist'),
      watch: true,
    },
  },
};
```

#### Option 2: Webpack Watch Mode Only

**Approach**: Use `webpack --watch` without dev-server

**Pros**:
- Simple setup
- Always writes to disk
- No additional server needed

**Cons**:
- No Hot Module Replacement
- Full page reload required for each change
- Slower than HMR for preserving application state
- No built-in error overlay

#### Option 3: Proxy Setup with Separate Dev Server Port

**Approach**: Run webpack-dev-server on a different port (e.g., 3000) and proxy API requests to backend

**Pros**:
- Standard webpack-dev-server setup
- Full HMR support

**Cons**:
- Violates requirement that backend serves frontend
- CORS complexity
- Need to configure proxy for all API routes
- Different URLs in development vs. production

### Selected Solution: Option 1 (Webpack Dev Server with writeToDisk)

This option best meets our requirements because:
1. Backend continues to serve frontend files (meets the key requirement)
2. Provides HMR for fast updates
3. Maintains simple architecture
4. Leverages existing webpack-dev-server dependency

## Implementation Details

### Webpack Configuration Changes

#### webpack.config.dev.js Updates

```javascript
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
    // Watch options
    watchFiles: {
      paths: ['app/**/*', 'assets/**/*'],
      options: {
        usePolling: false, // Use native file system events
      },
    },
    // Client configuration
    client: {
      overlay: {
        errors: true,
        warnings: false,
      },
      // WebSocket URL configuration
      // Explicitly set to webpack dev server port (8080) so browser connects to the right WebSocket
      webSocketURL: {
        hostname: 'localhost',
        pathname: '/ws',
        port: 8080,
        protocol: 'ws',
      },
    },
    // Disable host check for easier development
    allowedHosts: 'all',
    // Compression
    compress: true,
    // Headers
    headers: {
      'Access-Control-Allow-Origin': '*',
    },
  },
});
```

### React Hot Module Replacement Setup

To enable HMR for React components, we need to update the entry point:

#### app/index.tsx Updates

```typescript
// Add at the end of the file
if (module.hot) {
  module.hot.accept('./routes', () => {
    // Re-render the app when routes change
    const NextRoutes = require('./routes').default;
    root.render(
      <Provider store={store}>
        <ErrorBoundary>
          <NextRoutes />
        </ErrorBoundary>
      </Provider>
    );
  });
}
```

### Package.json Scripts

Add a new script for running the dev server:

```json
{
  "scripts": {
    "start:dev": "webpack serve --config webpack.config.dev.js",
    "build:dev": "yarn build:app:dev && yarn styles && yarn images:copy",
    "build:app:dev": "webpack --config webpack.config.dev.js"
  }
}
```

### Development Workflow

#### Starting the Development Environment

1. **Terminal 1 - Start Backend**:
   ```bash
   cd src/backend/ManagementHub.Service
   dotnet run
   ```
   Backend runs on http://localhost:5000

2. **Terminal 2 - Start Frontend Dev Server**:
   ```bash
   cd src/frontend
   yarn start:dev
   ```
   Dev server watches files and rebuilds on changes

3. **Browser**:
   Open http://localhost:5000
   Changes will hot reload automatically

#### How It Works

1. Developer edits a React component or TypeScript file
2. Webpack dev server detects the change
3. Webpack recompiles only the changed modules (incremental build)
4. New files are written to `dist/` directory
5. HMR client in browser receives update notification via WebSocket
6. HMR runtime applies the update without full page reload
7. React component updates in place, preserving state where possible

**Important Note on WebSocket Configuration:**

Since the browser accesses the application through the backend at `http://localhost:5000`, but webpack dev server runs on `http://localhost:8080`, we must explicitly configure the WebSocket URL. The `client.webSocketURL` setting in webpack config ensures the HMR client connects to `ws://localhost:8080/ws` (where webpack dev server is listening) rather than trying to connect to `ws://localhost:5000/ws` (which would fail). This is critical for HMR to work correctly in this architecture where the backend serves the frontend files.

### File Watching Considerations

- **Ignore Patterns**: The dev server will ignore `node_modules` by default
- **Polling vs. Native**: Use native file system events for better performance
- **Large Codebases**: If watching becomes slow, can adjust watch options

### CSS and Asset Handling

For styles and images, we have two options:

1. **Keep Separate Build** (Simpler):
   - Run `yarn styles` and `yarn images:copy` once at startup
   - Manually rebuild if CSS/images change
   - Pros: Simple, matches current workflow
   - Cons: No auto-reload for CSS changes

2. **Integrate into Webpack** (More Complete):
   - Configure webpack loaders for CSS and assets
   - Include in watch/HMR cycle
   - Pros: Full auto-reload experience
   - Cons: More configuration changes

**Recommendation**: Start with Option 1 (keep separate build), can enhance later if needed.

## Validation Plan

### Manual Testing Steps

1. **Initial Setup**:
   - Install dependencies: `yarn install`
   - Build styles and images once: `yarn styles && yarn images:copy`

2. **Start Development Environment**:
   - Terminal 1: `cd src/backend/ManagementHub.Service && dotnet run`
   - Terminal 2: `cd src/frontend && yarn start:dev`
   - Wait for both to be ready

3. **Test Hot Reload**:
   - Open browser to http://localhost:5000
   - Navigate to a page with visible UI (e.g., referees list)
   - Open `app/pages/Referees/index.tsx` (or similar)
   - Make a visible change (e.g., change button text)
   - Save file
   - Observe browser updates without manual refresh

4. **Test Error Handling**:
   - Introduce a syntax error in a TypeScript file
   - Save file
   - Verify error overlay appears in browser
   - Fix the error
   - Verify error overlay disappears and app recovers

5. **Test State Preservation**:
   - Navigate to a page with form inputs
   - Fill in some form fields
   - Make a change to the component code
   - Save file
   - Verify form state is preserved after HMR update (when possible)

### Expected Outcomes

- **Build Time**: Initial build ~10-30 seconds, incremental rebuilds ~1-5 seconds
- **Reload Time**: HMR updates visible in <1 second after save
- **State Preservation**: React component state preserved for non-root component changes
- **Error Feedback**: Syntax and build errors shown immediately in browser overlay

## Documentation Updates

### README.md Additions

Add a new section "Frontend Development Workflow":

```markdown
### Frontend Development Workflow

For rapid frontend development with automatic reloading:

1. Start the backend server:
   ```bash
   cd src/backend/ManagementHub.Service
   dotnet run
   ```

2. In a separate terminal, start the frontend dev server:
   ```bash
   cd src/frontend
   yarn start:dev
   ```

3. Open your browser to http://localhost:5000

Changes to React components and TypeScript files will automatically rebuild and hot reload in the browser without losing application state.

**Note**: Changes to CSS or image assets require running `yarn styles` or `yarn images:copy` manually.

**First Time Setup**: Run `yarn styles && yarn images:copy` before starting the dev server.
```

## Future Enhancements

Potential improvements for future iterations:

1. **Full Asset Integration**: Integrate CSS and image processing into webpack
2. **Redux DevTools Integration**: Enhanced debugging with state inspector
3. **React Refresh**: Upgrade to React Refresh for even better HMR experience
4. **Source Maps**: Optimize source maps for better debugging
5. **Bundle Analysis**: Integrate webpack-bundle-analyzer for optimization
6. **Code Splitting**: Optimize chunk splitting for faster initial loads

## Security Considerations

- **Dev Server Access**: Dev server should only run in development environment
- **CORS Headers**: Wide-open CORS in dev is acceptable; production should be strict
- **File Watching**: Ensure dev server doesn't watch sensitive files
- **Error Overlay**: Error details shown in browser are acceptable in dev, should be disabled in production

## Performance Considerations

- **Memory Usage**: Dev server keeps build in memory; acceptable for development
- **File System**: Writing to disk adds I/O but necessary for backend integration
- **Incremental Builds**: Webpack caching makes rebuilds very fast
- **Watch Limits**: On Linux, may need to increase `fs.inotify.max_user_watches` for large projects

## Rollback Plan

If issues arise:
1. Keep `yarn build:dev` as fallback command
2. Dev server is opt-in via `yarn start:dev`
3. Existing build process remains unchanged
4. No impact on production builds

## References

- [Webpack Dev Server Documentation](https://webpack.js.org/configuration/dev-server/)
- [Hot Module Replacement](https://webpack.js.org/concepts/hot-module-replacement/)
- [React Hot Loader](https://gaearon.github.io/react-hot-loader/)
- [Webpack Watch and WatchOptions](https://webpack.js.org/configuration/watch/)
