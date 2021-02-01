import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import {
  getReferee as getRefereeApi,
  IdAttributes,
  RefereeResponse,
  updateReferee as updateRefereeApi,
  UpdateRefereeRequest,
} from "../../apis/referee";
import { DataAttributes } from "../../schemas/getRefereeSchema";
import { AppThunk } from "../../store";

export interface RefereeState {
  referee: DataAttributes | null;
  id: string | null;
  ngbs: IdAttributes[] | null;
  testAttempts: IdAttributes[] | null;
  testResults: IdAttributes[] | null;
  certifications: IdAttributes[] | null;
  locations: IdAttributes[] | null;
  error: string | null;
  isLoading: boolean;
  teams: IdAttributes[] | null;
}

const initialState: RefereeState = {
  certifications: [],
  error: null,
  id: null,
  isLoading: false,
  locations: [],
  ngbs: [],
  referee: null,
  teams: [],
  testAttempts: [],
  testResults: [],
};

function refereeFailure(state: RefereeState, action: PayloadAction<string>) {
  state.referee = initialState.referee;
  state.certifications = initialState.certifications;
  state.id = initialState.id;
  state.ngbs = initialState.ngbs;
  state.testAttempts = initialState.testAttempts;
  state.testResults = initialState.testResults;
  state.error = action.payload;
  state.isLoading = false;
  state.locations = initialState.locations;
  state.teams = initialState.teams;
}

function refereeStart(state: RefereeState) {
  state.isLoading = true;
}

function refereeSuccess(state: RefereeState, action: PayloadAction<RefereeResponse>) {
  state.referee = action.payload.referee;
  state.certifications = action.payload.certifications;
  state.id = action.payload.id;
  state.ngbs = action.payload.ngbs;
  state.testAttempts = action.payload.testAttempts;
  state.testResults = action.payload.testResults;
  state.error = null;
  state.isLoading = false;
  state.locations = action.payload.locations;
  state.teams = action.payload.teams;
}

const referee = createSlice({
  initialState,
  name: "getReferee",
  reducers: {
    getRefereeFailure: refereeFailure,
    getRefereeStart: refereeStart,
    getRefereeSuccess: refereeSuccess,
    updateRefereeFailure: refereeFailure,
    updateRefereeStart: refereeStart,
    updateRefereeSuccess: refereeSuccess,
  },
});

export const {
  getRefereeStart,
  getRefereeSuccess,
  getRefereeFailure,
  updateRefereeStart,
  updateRefereeSuccess,
  updateRefereeFailure,
} = referee.actions;

export const fetchReferee = (refId: string): AppThunk => async (dispatch) => {
  try {
    dispatch(getRefereeStart());
    const refereeResponse = await getRefereeApi(refId);
    dispatch(getRefereeSuccess(refereeResponse));
  } catch (err) {
    dispatch(getRefereeFailure(err));
  }
};

export const updateReferee = (
  updatedReferee: UpdateRefereeRequest,
  refId: string
): AppThunk => async (dispatch) => {
  try {
    dispatch(updateRefereeStart());
    const refereeResponse = await updateRefereeApi(updatedReferee, refId);
    dispatch(updateRefereeSuccess(refereeResponse));
  } catch (err) {
    dispatch(updateRefereeFailure(err));
  }
};

export default referee.reducer;
