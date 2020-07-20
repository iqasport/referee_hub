import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import {
  getRefereeTests as getRefereeTestsApi,
  getTests as getTestsApi,
  IdAttributes,
  TestsResponse
} from '../../apis/test'
import { Datum } from '../../schemas/getTestsSchema'
import { AppThunk } from '../../store'

export interface TestsState {
  tests: Datum[];
  error: string | null;
  isLoading: boolean;
  certifications: IdAttributes[]
}

const initialState: TestsState = {
  certifications: [],
  error: null,
  isLoading: false,
  tests: [],
}

function testsSuccess(state: TestsState, action: PayloadAction<TestsResponse>) {
  state.isLoading = false
  state.error = null
  state.tests = action.payload.tests
  state.certifications = action.payload.certifications
}

function testsFailure(state: TestsState, action: PayloadAction<string>) {
  state.isLoading = false
  state.tests = []
  state.error = action.payload
  state.certifications = []
}

const tests = createSlice({
  initialState,
  name: 'tests',
  reducers: {
    getRefereeTestsFailure: testsFailure,
    getRefereeTestsStart(state: TestsState) {
      state.isLoading = true
    },
    getRefereeTestsSuccess: testsSuccess,
    getTestsFailure: testsFailure,
    getTestsStart(state: TestsState) {
      state.isLoading = true;
    },
    getTestsSuccess: testsSuccess,
  }
})

export const {
  getRefereeTestsFailure,
  getRefereeTestsStart,
  getRefereeTestsSuccess,
  getTestsFailure,
  getTestsStart,
  getTestsSuccess,
} = tests.actions

export const getTests = (): AppThunk => async dispatch => {
  try {
    dispatch(getTestsStart())
    const testsResponse = await getTestsApi()
    dispatch(getTestsSuccess(testsResponse))
  } catch (err) {
    dispatch(getTestsFailure(err.toString()))
  }
}

export const getRefereeTests = (refId: string): AppThunk => async dispatch => {
  try {
    dispatch(getRefereeTestsStart())
    const testsResponse = await getRefereeTestsApi(refId)
    dispatch(getRefereeTestsSuccess(testsResponse))
  } catch (err) {
    dispatch(getRefereeTestsFailure(err.toString()))
  }
}

export default tests.reducer
