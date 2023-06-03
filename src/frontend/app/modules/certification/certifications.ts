import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import {
  CertificationsResponse,
  getCertifications as getCertsApi,
} from "../../apis/certification";
import { Datum } from "../../schemas/getCertificationsSchema";
import { AppThunk } from "../../store";

interface CertificationsState {
  certifications: Datum[];
  error: string;
  isLoading: boolean;
}

const initialState: CertificationsState = {
  certifications: null,
  error: null,
  isLoading: false,
};

const certifications = createSlice({
  initialState,
  name: "certifications",
  reducers: {
    getCertificationsSuccess(
      state: CertificationsState,
      action: PayloadAction<CertificationsResponse>
    ) {
      state.certifications = action.payload.certifications;
      state.error = null;
      state.isLoading = false;
    },
    getCertificationsFailure(state: CertificationsState, action: PayloadAction<string>) {
      state.certifications = null;
      state.error = action.payload;
      state.isLoading = false;
    },
    getCertificationsStart(state: CertificationsState) {
      state.isLoading = true;
    },
  },
});

export const {
  getCertificationsFailure,
  getCertificationsStart,
  getCertificationsSuccess,
} = certifications.actions;

export const getCertifications = (): AppThunk => async (dispatch) => {
  try {
    dispatch(getCertificationsStart());
    const certResponse = await getCertsApi();
    dispatch(getCertificationsSuccess(certResponse));
  } catch (err) {
    dispatch(getCertificationsFailure(err.toString()));
  }
};

export default certifications.reducer;
