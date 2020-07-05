import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { CertificationsResponse, getCertifications as getCertsApi } from 'MainApp/apis/certification';
import { Datum } from 'MainApp/schemas/getCertificationsSchema';
import { AppThunk } from 'MainApp/store';

interface CertificationsState {
  certifications: Datum[];
  error: string;
}

const initialState: CertificationsState = {
  certifications: null,
  error: null
}

const certifications = createSlice({
  initialState,
  name: 'certifications',
  reducers: {
    getCertificationsSuccess(state: CertificationsState, action: PayloadAction<CertificationsResponse>) {
      state.certifications = action.payload.certifications
      state.error = null
    },
    getCertificationsFailure(state: CertificationsState, action: PayloadAction<string>) {
      state.certifications = null
      state.error = action.payload
    }
  }
})

export const {
  getCertificationsFailure,
  getCertificationsSuccess
} = certifications.actions

export const getCertifications = (): AppThunk => async dispatch => {
  try {
    const certResponse = await getCertsApi()
    dispatch(getCertificationsSuccess(certResponse))
  } catch (err) {
    dispatch(getCertificationsFailure(err.toString()))
  }
}

export default certifications.reducer
