import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { getTests as getTestsApi, IdAttributes, TestsResponse } from '../../apis/test'
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

const tests = createSlice({
  initialState,
  name: 'tests',
  reducers: {
    getTestsStart(state: TestsState) {
      state.isLoading = true;
    },
    getTestsSuccess(state: TestsState, action: PayloadAction<TestsResponse>) {
      state.isLoading = false
      state.error = null
      state.tests = action.payload.tests
      state.certifications = action.payload.certifications
    },
    getTestsFailure(state: TestsState, action: PayloadAction<string>) {
      state.isLoading = false
      state.tests = []
      state.error = action.payload
      state.certifications = []
    }
  }
})

export const {
  getTestsFailure,
  getTestsStart,
  getTestsSuccess
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

export default tests.reducer
