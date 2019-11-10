/* eslint-disable */
import React from 'react'
import { BrowserRouter as Router, Route } from 'react-router-dom'

import HomePage from './pages/HomePage'
import Referees from './pages/Referees'
import RefereeProfile from './pages/RefereeProfile'
import PrivacyPolicy from './pages/PrivacyPolicy'
import Admin from './pages/Admin'
import RefereeDiagnostic from './pages/RefereeDiagnostic'
import Tests from './pages/Tests'
import Test from './pages/Test'
import StartTest from './pages/StartTest'

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
