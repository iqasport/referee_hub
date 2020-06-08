import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { exportNgbTeams as exportNgbTeamsApi, JobResponse } from '../../apis/job';
import { AppThunk } from '../../store';

export interface JobState {
  jobId: string | null;
  error: string | null;
}

const initialState: JobState = {
  error: null,
  jobId: null,
}

const job = createSlice({
  initialState,
  name: 'jobs',
  reducers: {
    exportNgbTeamsSuccess(state: JobState, action: PayloadAction<JobResponse>) {
      state.jobId = action.payload.jobId
    },
    exportNgbTeamsFailure(state: JobState, action: PayloadAction<string>) {
      state.jobId = null
      state.error = action.payload
    }
  }
})

export const {
  exportNgbTeamsFailure,
  exportNgbTeamsSuccess
} = job.actions

export const exportNgbTeams = (): AppThunk => async dispatch => {
  try {
    const jobResponse = await exportNgbTeamsApi()
    dispatch(exportNgbTeamsSuccess(jobResponse))
  } catch (err) {
    dispatch(exportNgbTeamsFailure(err))
  }
}

export default job.reducer
