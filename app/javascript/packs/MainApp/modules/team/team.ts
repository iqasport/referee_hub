import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { getTeams as getTeamsApi, GetTeamsFilter, TeamResponse } from '../../apis/team';
import { Datum, Meta } from '../../schemas/getTeamsSchema';
import { AppThunk } from '../../store';

export interface TeamsState {
  teams: Datum[];
  filters?: GetTeamsFilter; 
  error: string | null;
  isLoading: boolean;
  meta?: Meta;
}

const initialState: TeamsState = {
  error: null,
  isLoading: false,
  teams: [],
}

const team = createSlice({
  initialState,
  name: 'teams',
  reducers: {
    getTeamsStart(state: TeamsState) {
      state.isLoading = true
    },
    getTeamsSuccess(state: TeamsState, action: PayloadAction<TeamResponse>) {
      state.teams = action.payload.teams
      state.meta = action.payload.meta
      state.error = null
      state.isLoading = false
    },
    getTeamsFailure(state: TeamsState, action: PayloadAction<string>) {
      state.teams = initialState.teams
      state.error = action.payload
      state.isLoading = false
    },
    updateFilters(state: TeamsState, action: PayloadAction<GetTeamsFilter>) {
      state.filters = action.payload
    },
    clearFilters(state: TeamsState) {
      state.filters = null
    }
  }
})

export const {
  clearFilters,
  getTeamsFailure,
  getTeamsStart,
  getTeamsSuccess,
  updateFilters,
} = team.actions

export const getTeams = (filters: GetTeamsFilter): AppThunk => async dispatch => {
  try {
    dispatch(getTeamsStart())
    const teamsResponse = await getTeamsApi(filters)
    dispatch(getTeamsSuccess(teamsResponse))
  } catch (err) {
    dispatch(getTeamsFailure(err))
  }
}

export default team.reducer
