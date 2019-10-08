/* eslint-disable */
import React from 'react'
import ReactDOM from 'react-dom'
import Routes from './routes'
import bugsnag from '@bugsnag/js'
import bugsnagReact from '@bugsnag/plugin-react'

var bugsnagClient = bugsnag({
  apiKey: process.env.BUGSNAG_API_KEY,
  autoCaptureSessions: false,
});
bugsnagClient.use(bugsnagReact, React);
var ErrorBoundary = bugsnagClient.getPlugin('react');

document.addEventListener('DOMContentLoaded', () => {
  ReactDOM.render(
    <ErrorBoundary>
      <Routes />
    </ErrorBoundary>,
    document.getElementById('main-app')
  );
});
