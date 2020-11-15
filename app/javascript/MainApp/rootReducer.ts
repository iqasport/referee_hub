import { combineReducers }from '@reduxjs/toolkit'

import certificationsReducer from './modules/certification/certifications'
import refCertificationReducer from './modules/certification/refCertification'
import checkoutReducer from './modules/checkout/checkout'
import productsReducer from './modules/checkout/products'
import currentUserReducer from './modules/currentUser/currentUser'
import jobsReducer from './modules/job/job'
import languagesReducer from './modules/language/languages'
import nationalGoverningBodiesReducer from './modules/nationalGoverningBody/nationalGoverningBodies'
import nationalGoverningBodyReducer from './modules/nationalGoverningBody/nationalGoverningBody'
import questionReducer from './modules/question/question'
import questionsReducer from './modules/question/questions'
import getRefereeReducer from './modules/referee/referee'
import refereesReducer from './modules/referee/referees'
import teamReducer from './modules/team/team'
import teamsReducer from './modules/team/teams'
import testReducer from './modules/test/single_test'
import testsReducer from './modules/test/tests'

const rootReducer = combineReducers({
  certifications: certificationsReducer,
  checkout: checkoutReducer,
  currentUser: currentUserReducer,
  jobs: jobsReducer,
  languages: languagesReducer,
  nationalGoverningBodies: nationalGoverningBodiesReducer,
  nationalGoverningBody: nationalGoverningBodyReducer,
  products: productsReducer,
  question: questionReducer,
  questions: questionsReducer,
  refCertification: refCertificationReducer,
  referee: getRefereeReducer,
  referees: refereesReducer,
  team: teamReducer,
  teams: teamsReducer,
  test: testReducer,
  tests: testsReducer,
})

export type RootState = ReturnType<typeof rootReducer>

export default rootReducer
