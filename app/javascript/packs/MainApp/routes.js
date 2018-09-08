import React from 'react'
import { BrowserRouter as Router, Route } from 'react-router-dom'

import HomePage from './components/HomePage'
import Referees from './components/Referees'

const App = (props) => (
  <Router>
    <div>
      <Route exact path='/' component={HomePage} />
      <Route exact path='/referees' component={Referees} />
    </div>
  </Router>
)

export default App
