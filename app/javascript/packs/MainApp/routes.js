/* eslint-disable */
import React from 'react'
import { BrowserRouter as Router, Route } from 'react-router-dom'

import HomePage from './components/HomePage'
import Referees from './components/Referees'
import RefereeProfile from './components/RefereeProfile'
import PrivacyPolicy from './components/PrivacyPolicy'
import Admin from './components/Admin'
import RefereeDiagnostic from './components/RefereeDiagnostic'
import Tests from './components/Tests'

const App = () => (
  <Router>
    <div>
      <Route exact path='/' component={HomePage} />
      <Route exact path='/privacy' component={PrivacyPolicy} />
      <Route exact path='/referees' component={Referees} />
      <Route path='/referees/:id' component={RefereeProfile} />
      <Route exact path='/admin' component={Admin} />
      <Route exact path='/admin/referee-diagnostic' component={RefereeDiagnostic} />
      <Route exact path='/admin/tests' component={Tests} />
    </div>
  </Router>
)

export default App
