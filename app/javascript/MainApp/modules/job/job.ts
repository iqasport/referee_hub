import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import {
  exportNgbReferees as exportNgbRefereesApi,
  exportNgbTeams as exportNgbTeamsApi,
  finishTest as finishTestApi,
  FinishTestRequest,
  JobResponse,
} from '../../apis/job';
import { AppThunk } from '../../store';

export interface JobState {
  jobId: string | null;
  error: string | null;
}

const initialState: JobState = {
  error: null,
  jobId: null,
}

function jobSuccess(state: JobState, action: PayloadAction<JobResponse>) {
  state.jobId = action.payload.jobId
  state.error = null
}

function jobFailure(state: JobState, action: PayloadAction<string>) {
  state.jobId = null
  state.error = action.payload
}

const job = createSlice({
  initialState,
  name: 'jobs',
  reducers: {
    exportNgbRefereesFailure: jobFailure,
    exportNgbRefereesSuccess: jobSuccess,
    exportNgbTeamsFailure: jobFailure,
    exportNgbTeamsSuccess: jobSuccess,
    finishTestFailure: jobFailure,
    finishTestSuccess: jobSuccess,
  }
})

export const {
  exportNgbTeamsFailure,
  exportNgbTeamsSuccess,
  exportNgbRefereesFailure,
  exportNgbRefereesSuccess,
  finishTestFailure,
  finishTestSuccess
} = job.actions

export const exportNgbTeams = (ngbId: string): AppThunk => async dispatch => {
  try {
    const jobResponse = await exportNgbTeamsApi(ngbId)
    dispatch(exportNgbTeamsSuccess(jobResponse))
  } catch (err) {
    dispatch(exportNgbTeamsFailure(err.toString()))
  }
}

export const exportNgbReferees = (ngbId: string): AppThunk => async dispatch => {
  try {
    const jobResponse = await exportNgbRefereesApi(ngbId)
    dispatch(exportNgbRefereesSuccess(jobResponse))
  } catch (err) {
    dispatch(exportNgbRefereesFailure(err.toString()))
  }
}

export const finishTest = (testId: string, request: FinishTestRequest): AppThunk => async dispatch => {
  try{
    const jobResponse = await finishTestApi(testId, request)
    dispatch(finishTestSuccess(jobResponse))
  } catch (err) {
    dispatch(finishTestFailure(err.toString()))
  }
}

export default job.reducer
