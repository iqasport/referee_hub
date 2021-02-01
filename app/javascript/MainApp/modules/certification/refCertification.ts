import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import {
  createCertification as createCertApi,
  CreateCertificationRequest,
  RefereeCertificationResponse,
  revokeCertification as revokeCertApi,
  UpdateCertificationRequest,
} from "MainApp/apis/certification";
import { Data } from "MainApp/schemas/getRefereeCertificationSchema";
import { AppThunk } from "MainApp/store";

import { fetchReferee } from "../referee/referee";

interface RefereeCertificationState {
  certification: Data;
  error: string;
  isLoading: boolean;
}

const initialState: RefereeCertificationState = {
  certification: null,
  error: null,
  isLoading: false,
};

const refCertification = createSlice({
  initialState,
  name: "refCertification",
  reducers: {
    createCertificationFailure(state: RefereeCertificationState, action: PayloadAction<string>) {
      state.certification = null;
      state.error = action.payload;
      state.isLoading = false;
    },
    createCertificationStart(state: RefereeCertificationState) {
      state.isLoading = true;
    },
    createCertificationSuccess(
      state: RefereeCertificationState,
      action: PayloadAction<RefereeCertificationResponse>
    ) {
      state.certification = action.payload.certification;
      state.error = null;
      state.isLoading = false;
    },
    revokeCertificationFailure(state: RefereeCertificationState, action: PayloadAction<string>) {
      state.certification = null;
      state.error = action.payload;
      state.isLoading = false;
    },
    revokeCertificationStart(state: RefereeCertificationState) {
      state.isLoading = true;
    },
    revokeCertificationSuccess(
      state: RefereeCertificationState,
      action: PayloadAction<RefereeCertificationResponse>
    ) {
      state.certification = action.payload.certification;
      state.error = null;
      state.isLoading = false;
    },
  },
});

export const {
  createCertificationFailure,
  createCertificationStart,
  createCertificationSuccess,
  revokeCertificationFailure,
  revokeCertificationStart,
  revokeCertificationSuccess,
} = refCertification.actions;

export const createCertification = (certification: CreateCertificationRequest): AppThunk => async (
  dispatch
) => {
  try {
    dispatch(createCertificationStart());
    const refCertResponse = await createCertApi(certification);
    dispatch(createCertificationSuccess(refCertResponse));
    dispatch(fetchReferee(certification.refereeId));
  } catch (err) {
    dispatch(createCertificationFailure(err.toString()));
  }
};

export const revokeCertification = (
  certification: UpdateCertificationRequest,
  certificationId: string
): AppThunk => async (dispatch) => {
  try {
    dispatch(revokeCertificationStart());
    const refCertResponse = await revokeCertApi(certification, certificationId);
    dispatch(revokeCertificationSuccess(refCertResponse));
    dispatch(fetchReferee(certification.refereeId));
  } catch (err) {
    dispatch(revokeCertificationFailure(err.toString()));
  }
};

export default refCertification.reducer;
