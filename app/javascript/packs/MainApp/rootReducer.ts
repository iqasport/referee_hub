import { combineReducers }from '@reduxjs/toolkit'

import currentUserReducer from './modules/currentUser/currentUser'
import getRefereeReducer from './modules/referee/getReferee';

const rootReducer = combineReducers({
  currentUser: currentUserReducer,
  referee: getRefereeReducer,
})

export type RootState = ReturnType<typeof rootReducer>

export default rootReducer
