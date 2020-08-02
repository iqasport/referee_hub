/* eslint-disable */
import Bugsnag from '@bugsnag/js'
import BugsnagPluginReact from '@bugsnag/plugin-react'
import React from 'react'
import ReactDOM from 'react-dom'
import { Provider } from 'react-redux'

import Routes from './routes'
import store from './store';

document.addEventListener('DOMContentLoaded', () => {
  const apiKey = process.env.RAILS_ENV !== 'production' ? 'iamAFak3apiKey' : process.env.BUGSNAG_API_KEY
  Bugsnag.start({
    apiKey,
    autoDetectErrors: true,
    autoTrackSessions: true,
    enabledErrorTypes: {
      unhandledExceptions: true,
      unhandledRejections: true
    },
    plugins: [
      new BugsnagPluginReact()
    ]
  });

  const ErrorBoundary = Bugsnag.getPlugin('react').createErrorBoundary(React)
  const domElement = document.getElementById('main-app')
  if (!domElement) return
  Bugsnag.leaveBreadcrumb('Starting application...')

  ReactDOM.render(
    <Provider store={store}>
      <ErrorBoundary>
        <Routes />
      </ErrorBoundary>
    </Provider>,
    domElement
  );
});
