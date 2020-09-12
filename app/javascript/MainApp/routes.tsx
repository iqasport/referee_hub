import Bugsnag from '@bugsnag/js'
import loadable from '@loadable/component'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { BrowserRouter as Router, Redirect, Route } from 'react-router-dom'

import Avatar from './components/Avatar'
import Loader from './components/Loader'
import { fetchCurrentUser } from './modules/currentUser/currentUser'
import { RootState } from './rootReducer'

const AsyncPage = loadable(props => import(`./pages/${props.page}`), {
  fallback: <Loader />
})

const App = () => {
  const [redirectTo, setRedirectTo] = useState<string>()
  const dispatch = useDispatch()
  const { currentUser, roles, id, error } = useSelector((state: RootState) => state.currentUser, shallowEqual)

  const getRedirect = () => {
    if (roles.includes("iqa_admin")) return "/admin"
    if (roles.includes("ngb_admin")) return `/national_governing_bodies/${currentUser?.ownedNgbId}`
    if (roles.includes("referee")) return `/referees/${id}`

    return null
  }

  useEffect(() => {
    if (!currentUser) {
      dispatch(fetchCurrentUser())
    }
  }, [currentUser]);

  useEffect(() => {
    if (error) {
      window.location.href = `${window.location.origin}/sign_in`
    }
  }, [error])

  useEffect(() => {
    setRedirectTo(getRedirect())
  }, [currentUser, roles])

  if (currentUser) Bugsnag.setUser(id)

  return (
    <Router>
      <div>
        <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
          <p className="flex-shrink mx-8">Management Hub</p>
          <Avatar
            firstName={currentUser?.firstName}
            lastName={currentUser?.lastName}
            roles={roles}
            userId={id}
            ownedNgbId={currentUser?.ownedNgbId}
          />
        </div>
        <Route exact={true} path='/'>
          {redirectTo && <Redirect to={redirectTo} />}
        </Route>
        <Route exact={true} path='/privacy' render={(props) => <AsyncPage {...props} page="PrivacyPolicy" />} />
        <Route
          exact={true}
          path='/referees/:id'
          render={(props) => <AsyncPage {...props} page="RefereeProfile" />}
        />
        <Route exact={true} path='/admin' render={(props) => <AsyncPage {...props} page="Admin" />} />
        <Route exact={true} path='/admin/tests' render={(props) => <AsyncPage {...props} page="Tests" />} />
        <Route exact={true} path='/admin/tests/:id' render={(props) => <AsyncPage {...props} page="Test" />} />
        <Route
          exact={true}
          path='/referees/:refereeId/tests/:testId'
          render={(props) => <AsyncPage {...props} page="StartTest" />}
        />
        <Route
          exact={true}
          path='/national_governing_bodies/:id'
          render={(props) => <AsyncPage {...props} page="NgbProfile" /> }
        />
        <Route
          exact={true}
          path='/import/:scope'
          render={(props) => <AsyncPage {...props} page="ImportWizard" />}
        />
        <Route
          exact={true}
          path='/referees/:refereeId/tests'
          render={(props) => <AsyncPage {...props} page="RefereeTests" />}
        />
      </div>
    </Router>
  )
}

export default App
