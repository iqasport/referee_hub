import { combineReducers }from '@reduxjs/toolkit'

import currentUserReducer from './modules/currentUser/currentUser'
import nationalGoverningBodiesReducer from './modules/nationalGoverningBody/nationalGoverningBodies'
import nationalGoverningBodyReducer from './modules/nationalGoverningBody/nationalGoverningBody';
import getRefereeReducer from './modules/referee/referee';
import refereesReducer from './modules/referee/referees'
import teamsReducer from './modules/team/team'

const rootReducer = combineReducers({
  currentUser: currentUserReducer,
  nationalGoverningBodies: nationalGoverningBodiesReducer,
  nationalGoverningBody: nationalGoverningBodyReducer,
  referee: getRefereeReducer,
  referees: refereesReducer,
  teams: teamsReducer,
})

export type RootState = ReturnType<typeof rootReducer>

export default rootReducer
