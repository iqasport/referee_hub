import { combineReducers }from '@reduxjs/toolkit'

import currentUserReducer from './modules/currentUser/currentUser'
import jobsReducer from './modules/job/job'
import nationalGoverningBodiesReducer from './modules/nationalGoverningBody/nationalGoverningBodies'
import nationalGoverningBodyReducer from './modules/nationalGoverningBody/nationalGoverningBody';
import getRefereeReducer from './modules/referee/referee';
import refereesReducer from './modules/referee/referees'
import teamReducer from './modules/team/team'
import teamsReducer from './modules/team/teams'

const rootReducer = combineReducers({
  currentUser: currentUserReducer,
  jobs: jobsReducer,
  nationalGoverningBodies: nationalGoverningBodiesReducer,
  nationalGoverningBody: nationalGoverningBodyReducer,
  referee: getRefereeReducer,
  referees: refereesReducer,
  team: teamReducer,
  teams: teamsReducer,
})

export type RootState = ReturnType<typeof rootReducer>

export default rootReducer
