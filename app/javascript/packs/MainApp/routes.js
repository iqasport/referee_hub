import React from 'react'
import { BrowserRouter as Router, Route } from 'react-router-dom'

import HomePage from './components/HomePage'

const App = (props) => (
  <Router>
    <div>
      <Route exact path='/' component={HomePage} />
    </div>
  </Router>
)

export default App
