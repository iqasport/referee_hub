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
import Test from './components/Test'
import StartTest from './components/StartTest'

const App = () => (
  <Router>
    <div>
      <Route exact path='/' component={HomePage} />
      <Route exact path='/privacy' component={PrivacyPolicy} />
      <Route exact path='/referees' component={Referees} />
      <Route exact path='/referees/:id' component={RefereeProfile} />
      <Route exact path='/admin' component={Admin} />
      <Route exact path='/admin/referee-diagnostic' component={RefereeDiagnostic} />
      <Route exact path='/admin/tests' component={Tests} />
      <Route exact path='/admin/tests/:id' component={Test} />
      <Route exact path='/referees/:refereeId/tests/:testId' component={StartTest} />
    </div>
  </Router>
)

export default App
