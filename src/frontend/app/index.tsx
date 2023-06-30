/* eslint-disable */
import Bugsnag from "@bugsnag/js";
import BugsnagPluginReact from "@bugsnag/plugin-react";
import React from "react";
import { createRoot } from 'react-dom/client';
import { Provider } from "react-redux";

import Routes from "./routes";
import store from "./store";

document.addEventListener("DOMContentLoaded", () => {
  let ErrorBoundary: React.ComponentType<{children?: React.ReactNode}> = React.Fragment;
  const bugsnugApiKey = process.env.BUGSNAG_API_KEY;
  if (bugsnugApiKey) {
    Bugsnag.start({
      apiKey: bugsnugApiKey,
      autoDetectErrors: true,
      autoTrackSessions: true,
      enabledErrorTypes: {
        unhandledExceptions: true,
        unhandledRejections: true,
      },
      plugins: [new BugsnagPluginReact()],
    });

    ErrorBoundary = Bugsnag.getPlugin("react").createErrorBoundary(React);
  }

  const domElement = document.getElementById("main-app");
  if (!domElement) return;
  const root = createRoot(domElement);
  
  Bugsnag.leaveBreadcrumb("Starting application...");

  root.render(
    <Provider store={store}>
      <ErrorBoundary>
        <Routes />
      </ErrorBoundary>
    </Provider>
  );
});
