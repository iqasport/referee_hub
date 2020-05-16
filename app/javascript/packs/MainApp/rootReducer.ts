import { combineReducers }from '@reduxjs/toolkit'

import currentUserReducer from './modules/currentUser/currentUser'
import nationalGoverningBodiesReducer from './modules/nationalGoverningBody/nationalGoverningBodies'
import nationalGoverningBodyReducer from './modules/nationalGoverningBody/nationalGoverningBody';
import getRefereeReducer from './modules/referee/referee';
import teamsReducer from './modules/team/team'

const rootReducer = combineReducers({
  currentUser: currentUserReducer,
  nationalGoverningBodies: nationalGoverningBodiesReducer,
  nationalGoverningBody: nationalGoverningBodyReducer,
  referee: getRefereeReducer,
  teams: teamsReducer,
})

export type RootState = ReturnType<typeof rootReducer>

export default rootReducer
