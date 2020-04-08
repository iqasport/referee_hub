/* eslint-disable */
import React, { useState, useEffect } from 'react'
import { BrowserRouter as Router, Route, Redirect } from 'react-router-dom'
import axios from 'axios';

import Avatar from './components/Avatar'
import Referees from './pages/Referees'
import RefereeProfile from './pages/RefereeProfile/RefereeProfile'
import PrivacyPolicy from './pages/PrivacyPolicy'
import Admin from './pages/Admin'
import RefereeDiagnostic from './pages/RefereeDiagnostic'
import Tests from './pages/Tests'
import Test from './pages/Test'
import StartTest from './pages/StartTest'

const App = () => {
  const [roles, setRoles] = useState([]);
  const [userId, setUserId] = useState();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('')
  
  useEffect(() => {
    axios.get("/api/v1/users/current_user").then((response) => {
      const { data: { id, attributes }, included } = response.data
      const fetchedRoles = included && included.map(({ attributes }) => attributes.access_type)

      setRoles(fetchedRoles)
      setUserId(id);
      setFirstName(attributes.first_name)
      setLastName(attributes.last_name)
    }).catch(() => {
      window.location = `${window.location.origin}/sign_in`
    });
  }, []);

  const getRedirect = () => {
    if (roles && roles.includes("iqa_admin")) return "/admin"
    if (roles && roles.includes("ngb_admin")) return "/privacy" // TODO: replace with ngb admin route once ready

    return `/referees/${userId}`
  }

  if (!userId) return null

  return (
    <Router>
      <div>
        <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
          <p className="flex-shrink mx-8">Management Hub</p>
          <Avatar firstName={firstName} lastName={lastName} />
        </div>
        <Route exact path='/'>
          <Redirect to={getRedirect()} />
        </Route>
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
}

export default App
