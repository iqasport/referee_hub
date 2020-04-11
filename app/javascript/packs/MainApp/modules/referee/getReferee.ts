import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { getReferee as getRefereeApi, GetRefereeResponse } from '../../apis/referee';
import { DataAttributes, IncludedAttributes } from '../../schemas/getRefereeSchema';
import { AppThunk } from '../../store';

interface RefereeState {
  referee: DataAttributes | null;
  id: string | null;
  ngbs: IncludedAttributes[] | null;
  testAttempts: IncludedAttributes[] | null;
  testResults: IncludedAttributes[] | null;
  certifications: IncludedAttributes[] | null;
  error: string | null;
  isLoading: boolean;
}

const initialState: RefereeState = {
  certifications: [],
  error: null,
  id: null,
  isLoading: false,
  ngbs: [],
  referee: null,
  testAttempts: [],
  testResults: [],
}

const getReferee = createSlice({
  initialState,
  name: 'getReferee',
  reducers: {
    getRefereeStart(state: RefereeState) {
      state.isLoading = true
    },
    getRefereeSuccess(state: RefereeState, action: PayloadAction<GetRefereeResponse>) {
      state.referee = action.payload.referee
      state.certifications = action.payload.certifications
      state.id = action.payload.id
      state.ngbs = action.payload.ngbs
      state.testAttempts = action.payload.testAttempts
      state.testResults = action.payload.testResults
      state.error = null
      state.isLoading = false
    },
    getRefereeFailure(state: RefereeState, action: PayloadAction<string>) {
      state.referee = initialState.referee
      state.certifications = initialState.certifications;
      state.id = initialState.id;
      state.ngbs = initialState.ngbs;
      state.testAttempts = initialState.testAttempts;
      state.testResults = initialState.testResults;
      state.error = null;
      state.isLoading = false;
    }
  }
})

export const {
  getRefereeStart,
  getRefereeSuccess,
  getRefereeFailure,
} = getReferee.actions

export const fetchReferee = (refId: string): AppThunk => async dispatch => {
  try {
    dispatch(getRefereeStart())
    const refereeResponse = await getRefereeApi(refId)
    dispatch(getRefereeSuccess(refereeResponse))
  } catch (err) {
    dispatch(getRefereeFailure(err))
  }
}
