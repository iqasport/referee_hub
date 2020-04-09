/* eslint-disable */
import bugsnag from '@bugsnag/js'
import bugsnagReact from '@bugsnag/plugin-react'
import React from 'react'
import ReactDOM from 'react-dom'
import { Provider } from 'react-redux'

import Routes from './routes'
import store from './store';

document.addEventListener('DOMContentLoaded', () => {
  const apiKey = process.env.RAILS_ENV !== 'production' ? 'iamAFak3apiKey' : process.env.BUGSNAG_API_KEY
  const bugsnagClient = bugsnag({
    apiKey,
    autoCaptureSessions: false,
  });
  bugsnagClient.use(bugsnagReact, React);
  const ErrorBoundary = bugsnagClient.getPlugin('react');
  const domElement = document.getElementById('main-app')
  if(!domElement) return

  ReactDOM.render(
    <Provider store={store}>
      <ErrorBoundary>
          <Routes />
      </ErrorBoundary>
    </Provider>,
    domElement
  );
});
