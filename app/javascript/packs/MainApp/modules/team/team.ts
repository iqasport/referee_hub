import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { createTeam as createTeamApi, getTeam as getTeamApi, TeamResponse, UpdateTeamRequest } from '../../apis/team';
import { DataAttributes, IncludedAttributes } from '../../schemas/getTeamSchema';
import { AppThunk } from '../../store';

export interface TeamState {
  team: DataAttributes;
  isLoading: boolean;
  error: string | null;
  socialAccounts: IncludedAttributes[];
}

const initialState: TeamState = {
  error: null,
  isLoading: false,
  socialAccounts: [],
  team: null,
}

function teamSuccess(state: TeamState, action: PayloadAction<TeamResponse>) {
  state.team = action.payload.team
  state.error = null
  state.isLoading = false
  state.socialAccounts = action.payload.socialAccounts
}

function teamFailure(state: TeamState, action: PayloadAction<string>) {
  state.team = null
  state.error = action.payload
  state.isLoading = false
  state.socialAccounts = []
}

const team = createSlice({
  initialState,
  name: 'team',
  reducers: {
    createTeamFailure: teamFailure,
    createTeamStart(state: TeamState) {
      state.isLoading = true
    },
    createTeamSuccess: teamSuccess,
    getTeamFailure: teamFailure,
    getTeamStart(state: TeamState) {
      state.isLoading = true
    },
    getTeamSuccess: teamSuccess,
  }
})

export const {
  createTeamFailure,
  createTeamStart,
  createTeamSuccess,
  getTeamFailure,
  getTeamStart,
  getTeamSuccess,
} = team.actions

export const getTeam = (id: string): AppThunk => async dispatch => {
  try {
    dispatch(getTeamStart())
    const teamResponse = await getTeamApi(id)
    dispatch(getTeamSuccess(teamResponse))
  } catch (err) {
    dispatch(getTeamFailure(err.toString()))
  }
}

export const createTeam = (newTeam: UpdateTeamRequest): AppThunk => async dispatch => {
  try {
    dispatch(createTeamStart())
    const teamResponse = await createTeamApi(newTeam)
    dispatch(createTeamSuccess(teamResponse))
  } catch(err) {
    dispatch(createTeamFailure(err.toString()))
  }
}

export default team.reducer
