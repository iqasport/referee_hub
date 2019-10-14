/* eslint-disable */
import React from 'react'
import ReactDOM from 'react-dom'
import Routes from './routes'
import bugsnag from '@bugsnag/js'
import bugsnagReact from '@bugsnag/plugin-react'

document.addEventListener('DOMContentLoaded', () => {
  var bugsnagClient = bugsnag({
    apiKey: process.env.BUGSNAG_API_KEY,
    autoCaptureSessions: false,
  });
  bugsnagClient.use(bugsnagReact, React);
  var ErrorBoundary = bugsnagClient.getPlugin('react');

  ReactDOM.render(
    <ErrorBoundary>
      <Routes />
    </ErrorBoundary>,
    document.getElementById('main-app')
  );
});
