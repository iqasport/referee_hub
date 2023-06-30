import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import {
  createTeam as createTeamApi,
  deleteTeam as deleteTeamApi,
  getTeam as getTeamApi,
  TeamResponse,
  updateTeam as updateTeamApi,
  UpdateTeamRequest,
} from "../../apis/team";
import { DataAttributes, IncludedAttributes } from "../../schemas/getTeamSchema";
import { AppThunk } from "../../store";
import { getNgbTeams } from "./teams";

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
};

function teamSuccess(state: TeamState, action: PayloadAction<TeamResponse>) {
  state.team = action.payload.team;
  state.error = null;
  state.isLoading = false;
  state.socialAccounts = action.payload.socialAccounts;
}

function teamFailure(state: TeamState, action: PayloadAction<string>) {
  state.team = null;
  state.error = action.payload;
  state.isLoading = false;
  state.socialAccounts = [];
}

const team = createSlice({
  initialState,
  name: "team",
  reducers: {
    createTeamFailure: teamFailure,
    createTeamStart(state: TeamState) {
      state.isLoading = true;
    },
    createTeamSuccess: teamSuccess,
    deleteTeamFailure: teamFailure,
    deleteTeamStart(state: TeamState) {
      state.isLoading = true;
    },
    deleteTeamSuccess: teamSuccess,
    getTeamFailure: teamFailure,
    getTeamStart(state: TeamState) {
      state.isLoading = true;
    },
    getTeamSuccess: teamSuccess,
    updateTeamFailure: teamFailure,
    updateTeamStart(state: TeamState) {
      state.isLoading = true;
    },
    updateTeamSuccess: teamSuccess,
  },
});

export const {
  createTeamFailure,
  createTeamStart,
  createTeamSuccess,
  deleteTeamFailure,
  deleteTeamStart,
  deleteTeamSuccess,
  getTeamFailure,
  getTeamStart,
  getTeamSuccess,
  updateTeamFailure,
  updateTeamStart,
  updateTeamSuccess,
} = team.actions;

export const getTeam = (id: string, ngbId: string): AppThunk => async (dispatch) => {
  try {
    dispatch(getTeamStart());
    const teamResponse = await getTeamApi(id, ngbId);
    dispatch(getTeamSuccess(teamResponse));
  } catch (err) {
    dispatch(getTeamFailure(err.toString()));
  }
};

export const createTeam = (newTeam: UpdateTeamRequest): AppThunk => async (dispatch) => {
  try {
    dispatch(createTeamStart());
    const teamResponse = await createTeamApi(newTeam);
    dispatch(createTeamSuccess(teamResponse));
    dispatch(
      getNgbTeams({
        nationalGoverningBodies: [parseInt(newTeam.nationalGoverningBodyId, 10)],
        nationalGoverningBodyId: newTeam.nationalGoverningBodyId,
      })
    );
  } catch (err) {
    dispatch(createTeamFailure(err.toString()));
  }
};

export const updateTeam = (id: string, newTeam: UpdateTeamRequest): AppThunk => async (
  dispatch
) => {
  try {
    dispatch(updateTeamStart());
    const teamResponse = await updateTeamApi(id, newTeam);
    dispatch(updateTeamSuccess(teamResponse));
    dispatch(
      getNgbTeams({
        nationalGoverningBodies: [parseInt(newTeam.nationalGoverningBodyId, 10)],
        nationalGoverningBodyId: newTeam.nationalGoverningBodyId,
      })
    );
  } catch (err) {
    dispatch(updateTeamFailure(err.toString()));
  }
};

export const deleteTeam = (id: string, ngbId: string): AppThunk => async (dispatch) => {
  try {
    dispatch(deleteTeamStart());
    const teamResponse = await deleteTeamApi(id, ngbId);
    dispatch(deleteTeamSuccess(teamResponse));
    dispatch(getNgbTeams({ nationalGoverningBodyId: ngbId }));
  } catch (err) {
    dispatch(deleteTeamFailure(err.toString()));
  }
};

export default team.reducer;
