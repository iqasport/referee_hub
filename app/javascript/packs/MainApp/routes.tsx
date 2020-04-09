import axios from 'axios';
/* eslint-disable */
import React, { useEffect, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { Redirect, Route, BrowserRouter as Router } from 'react-router-dom'

import Avatar from './components/Avatar'
import { fetchCurrentUser } from './modules/currentUser/currentUser'
import Admin from './pages/Admin'
import PrivacyPolicy from './pages/PrivacyPolicy'
import RefereeDiagnostic from './pages/RefereeDiagnostic'
import RefereeProfile from './pages/RefereeProfile/RefereeProfile'
import Referees from './pages/Referees'
import StartTest from './pages/StartTest'
import Test from './pages/Test'
import Tests from './pages/Tests'
import { RootState } from './rootReducer'

const App = () => {
  const dispatch = useDispatch()
  const { currentUser, roles } = useSelector((state: RootState) => state.currentUser)
  console.log({ roles })
  useEffect(() => {
    try {
      dispatch(fetchCurrentUser())
    } catch {
      window.location.href = `${window.location.origin}/sign_in`;
    }
  }, []);

  const getRedirect = () => {
    if (roles.includes("iqa_admin")) return "/admin"
    if (roles.includes("ngb_admin")) return "/privacy" // TODO: replace with ngb admin route once ready

    return `/referees/${currentUser?.id}`
  }

  if(!currentUser) return null

  return (
    <Router>
      <div>
        <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
          <p className="flex-shrink mx-8">Management Hub</p>
          <Avatar firstName={currentUser?.firstName} lastName={currentUser?.lastName} />
        </div>
        <Route exact={true} path='/'>
          <Redirect to={getRedirect()} />
        </Route>
        <Route exact={true} path='/privacy' component={PrivacyPolicy} />
        <Route exact={true} path='/referees' component={Referees} />
        <Route exact={true} path='/referees/:id' component={RefereeProfile} />
        <Route exact={true} path='/admin' component={Admin} />
        <Route exact={true} path='/admin/referee-diagnostic' component={RefereeDiagnostic} />
        <Route exact={true} path='/admin/tests' component={Tests} />
        <Route exact={true} path='/admin/tests/:id' component={Test} />
        <Route exact={true} path='/referees/:refereeId/tests/:testId' component={StartTest} />
      </div>
    </Router>
  )
}

export default App
