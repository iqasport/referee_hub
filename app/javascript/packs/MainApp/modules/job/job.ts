import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { exportNgbReferees as exportNgbRefereesApi, exportNgbTeams as exportNgbTeamsApi, JobResponse } from '../../apis/job';
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
  }
})

export const {
  exportNgbTeamsFailure,
  exportNgbTeamsSuccess,
  exportNgbRefereesFailure,
  exportNgbRefereesSuccess
} = job.actions

export const exportNgbTeams = (): AppThunk => async dispatch => {
  try {
    const jobResponse = await exportNgbTeamsApi()
    dispatch(exportNgbTeamsSuccess(jobResponse))
  } catch (err) {
    dispatch(exportNgbTeamsFailure(err))
  }
}

export const exportNgbReferees = (): AppThunk => async dispatch => {
  try {
    const jobResponse = await exportNgbRefereesApi()
    dispatch(exportNgbRefereesSuccess(jobResponse))
  } catch (err) {
    dispatch(exportNgbRefereesFailure(err))
  }
}

export default job.reducer
