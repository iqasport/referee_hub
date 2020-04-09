import { combineReducers }from '@reduxjs/toolkit'

import currentUserReducer from './modules/currentUser/currentUser'

const rootReducer = combineReducers({
  currentUser: currentUserReducer
})

export type RootState = ReturnType<typeof rootReducer>

export default rootReducer
