import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import {
  getNgbTeams as getNgbTeamsApi,
  getTeams as getTeamsApi,
  GetTeamsFilter,
  importNgbTeams,
  TeamsResponse,
} from "../../apis/team";
import { HeadersMap } from "../../pages/ImportWizard/MapStep";
import { Datum, Meta } from "../../schemas/getTeamsSchema";
import { AppThunk } from "../../store";

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
};

function teamsSuccess(state: TeamsState, action: PayloadAction<TeamsResponse>) {
  state.teams = action.payload.teams;
  state.meta = action.payload.meta;
  state.error = null;
  state.isLoading = false;
}

function teamsFailure(state: TeamsState, action: PayloadAction<string>) {
  state.teams = initialState.teams;
  state.error = action.payload;
  state.isLoading = false;
}

const teams = createSlice({
  initialState,
  name: "teams",
  reducers: {
    getNgbTeamsFailure: teamsFailure,
    getNgbTeamsStart(state: TeamsState) {
      state.isLoading = true;
    },
    getNgbTeamsSuccess: teamsSuccess,
    getTeamsFailure: teamsFailure,
    getTeamsStart(state: TeamsState) {
      state.isLoading = true;
    },
    getTeamsSuccess: teamsSuccess,
    updateFilters(state: TeamsState, action: PayloadAction<GetTeamsFilter>) {
      state.filters = action.payload;
    },
    clearFilters(state: TeamsState) {
      state.filters = null;
    },
    importTeamsFailure: teamsFailure,
    importTeamsSuccess: teamsSuccess,
  },
});

export const {
  clearFilters,
  getNgbTeamsFailure,
  getNgbTeamsStart,
  getNgbTeamsSuccess,
  getTeamsFailure,
  getTeamsStart,
  getTeamsSuccess,
  importTeamsFailure,
  importTeamsSuccess,
  updateFilters,
} = teams.actions;

export const getTeams = (filters: GetTeamsFilter): AppThunk => async (dispatch) => {
  try {
    dispatch(getTeamsStart());
    const teamsResponse = await getTeamsApi(filters);
    dispatch(getTeamsSuccess(teamsResponse));
  } catch (err) {
    dispatch(getTeamsFailure(err.toString()));
  }
};

export const importTeams = (file: File, mappedData: HeadersMap, ngbId: string): AppThunk => async (
  dispatch
) => {
  try {
    const teamsResponse = await importNgbTeams(file, mappedData, ngbId);
    dispatch(importTeamsSuccess(teamsResponse));
  } catch (err) {
    dispatch(importTeamsFailure(err.toString()));
  }
};

export const getNgbTeams = (filter?: GetTeamsFilter): AppThunk => async (dispatch) => {
  try {
    dispatch(getNgbTeamsStart());
    const teamsResponse = await getNgbTeamsApi(filter);
    dispatch(getNgbTeamsSuccess(teamsResponse));
  } catch (err) {
    dispatch(getNgbTeamsFailure(err.toString()));
  }
};

export default teams.reducer;
