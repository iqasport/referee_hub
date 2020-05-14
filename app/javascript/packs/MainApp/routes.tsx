import React, { useEffect, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { BrowserRouter as Router, Redirect, Route } from 'react-router-dom'

import Avatar from './components/Avatar'
import { fetchCurrentUser } from './modules/currentUser/currentUser'
import Admin from './pages/Admin'
import NgbProfile from './pages/NgbProfile'
import OldRefereeProfile from './pages/OldRefereeProfile'
import PrivacyPolicy from './pages/PrivacyPolicy'
import RefereeDiagnostic from './pages/RefereeDiagnostic'
import RefereeProfile from './pages/RefereeProfile'
import Referees from './pages/Referees'
import StartTest from './pages/StartTest'
import Test from './pages/Test'
import Tests from './pages/Tests'
import { RootState } from './rootReducer'

const App = () => {
  const dispatch = useDispatch()
  const { currentUser, roles, id } = useSelector((state: RootState) => state.currentUser)
  
  useEffect(() => {
    try {
      dispatch(fetchCurrentUser())
    } catch {
      window.location.href = `${window.location.origin}/sign_in`;
    }
  }, []);
  
  if(!currentUser) return null

  const newDesignEnabled = currentUser.enabledFeatures.includes('new_design')
  const refProfile = newDesignEnabled ? RefereeProfile : OldRefereeProfile;
  const getRedirect = () => {
    if (!roles.length) return "/sign_in"
    if (roles.includes("iqa_admin")) return "/admin"
    if (roles.includes("ngb_admin")) return `/national_governing_bodies/${currentUser.ownedNgbId}`
    
    return `/referees/${id}`
  }

  return (
    <Router>
      <div>
        {
          newDesignEnabled && (
            <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
              <p className="flex-shrink mx-8">Management Hub</p>
              <Avatar firstName={currentUser?.firstName} lastName={currentUser?.lastName} />
            </div>
          )
        }
        <Route exact={true} path='/'>
          <Redirect to={getRedirect()} />
        </Route>
        <Route exact={true} path='/privacy' component={PrivacyPolicy} />
        <Route exact={true} path='/referees' component={Referees} />
        <Route exact={true} path='/referees/:id' component={refProfile} />
        <Route exact={true} path='/admin' component={Admin} />
        <Route exact={true} path='/admin/referee-diagnostic' component={RefereeDiagnostic} />
        <Route exact={true} path='/admin/tests' component={Tests} />
        <Route exact={true} path='/admin/tests/:id' component={Test} />
        <Route exact={true} path='/referees/:refereeId/tests/:testId' component={StartTest} />
        <Route exact={true} path='/national_governing_bodies/:id' component={NgbProfile} />
      </div>
    </Router>
  )
}

export default App
