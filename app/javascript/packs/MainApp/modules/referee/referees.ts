import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { getReferees as getRefereesApi, GetRefereesFilter, IdAttributes, RefereesResponse } from '../../apis/referee'
import { DataAttributes } from '../../schemas/getRefereeSchema'
import { Meta } from '../../schemas/getRefereesSchema'
import { AppThunk } from '../../store'

export interface Referee {
  referee: DataAttributes | null;
  id: string | null;
  ngbs: IdAttributes[] | null;
  testAttempts: IdAttributes[] | null;
  testResults: IdAttributes[] | null;
  certifications: IdAttributes[] | null;
  locations: IdAttributes[] | null;
  teams: IdAttributes[] | null;
}

export interface RefereesState {
  referees: Referee[];
  meta: Meta | null;
  error: string | null;
  isLoading: boolean;
  filters?: GetRefereesFilter;
}

const initialState: RefereesState = {
  error: null, 
  isLoading: false,
  meta: null,
  referees: [],
}

const referees = createSlice({
  initialState,
  name: 'referees',
  reducers: {
    getRefereesStart(state: RefereesState) {
      state.isLoading = true;
    },
    getRefereesSuccess(state: RefereesState, action: PayloadAction<RefereesResponse>) {
      state.referees = action.payload.referees
      state.meta = action.payload.meta
      state.error = null
      state.isLoading = false
    },
    getRefereesFailure(state: RefereesState, action: PayloadAction<string>) {
      state.referees = []
      state.meta = null
      state.isLoading = false
      state.error = action.payload
    },
    updateFilters(state: RefereesState, action: PayloadAction<GetRefereesFilter>) {
      state.filters = action.payload
    },
    clearFilters(state: RefereesState) {
      state.filters = null
    }
  }
})

export const {
  clearFilters,
  getRefereesFailure,
  getRefereesStart,
  getRefereesSuccess,
  updateFilters,
} = referees.actions

export const getReferees = (filter: GetRefereesFilter): AppThunk => async dispatch => {
  try {
    dispatch(getRefereesStart())
    const refereeResponse = await getRefereesApi(filter)
    dispatch(getRefereesSuccess(refereeResponse))
  } catch (err) {
    dispatch(getRefereesFailure(err))
  }
}

export default referees.reducer
