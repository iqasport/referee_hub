import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import {
  createTest as createTestApi,
  getTest as getTestApi,
  IdAttributes,
  TestResponse,
  updateTest as updateTestApi,
  UpdateTestRequest,
} from '../../apis/test'
import { Data } from '../../schemas/getTestSchema'
import { AppThunk } from '../../store'
import { getTests } from './tests'

export interface TestState {
  test: Data | null;
  isLoading: boolean;
  error: string | null;
  certification: IdAttributes;
}

const initialState: TestState = {
  certification: null,
  error: null,
  isLoading: false,
  test: null,
}

function testSuccess(state: TestState, action: PayloadAction<TestResponse>) {
  state.isLoading = false
  state.error = null
  state.test = action.payload.test
  state.certification = action.payload.certification
}

function testFailure(state: TestState, action: PayloadAction<string>) {
  state.isLoading = false
  state.test = null
  state.error = action.payload
  state.certification = null
}

const test = createSlice({
  initialState,
  name: 'test',
  reducers: {
    createTestFailure: testFailure,
    createTestStart(state: TestState) {
      state.isLoading = true
    },
    createTestSuccess: testSuccess,
    getTestFailure: testFailure,
    getTestStart(state: TestState) {
      state.isLoading = true
    },
    getTestSuccess: testSuccess,
    updateTestFailure: testFailure,
    updateTestStart(state: TestState) {
      state.isLoading = true
    },
    updateTestSuccess: testSuccess
  }
})

export const {
  createTestFailure,
  createTestStart,
  createTestSuccess,
  getTestFailure,
  getTestStart,
  getTestSuccess,
  updateTestFailure,
  updateTestStart,
  updateTestSuccess,
} = test.actions

export const getTest = (id: string): AppThunk => async dispatch => {
  try {
    dispatch(getTestStart())
    const testResponse = await getTestApi(id)
    dispatch(getTestSuccess(testResponse))
  } catch (err) {
    dispatch(getTestFailure(err.toString()))
  }
}

export const updateTest = (id: string, updatedTest: UpdateTestRequest): AppThunk => async dispatch => {
  try {
    dispatch(updateTestStart())
    const testResponse = await updateTestApi(id, updatedTest)
    dispatch(updateTestSuccess(testResponse))
    dispatch(getTests())
  } catch (err) {
    dispatch(updateTestFailure(err.toString()))
  }
}

export const createTest = (newTest: UpdateTestRequest): AppThunk => async dispatch => {
  try {
    dispatch(createTestStart())
    const testResponse = await createTestApi(newTest)
    dispatch(createTestSuccess(testResponse))
    dispatch(getTests())
  } catch (err) {
    dispatch(createTestFailure(err.toString()))
  }
}

export default test.reducer
