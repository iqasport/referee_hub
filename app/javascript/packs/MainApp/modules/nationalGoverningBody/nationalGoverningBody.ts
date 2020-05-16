import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { getNationalGoverningBody as getNgbApi, NgbResponse } from '../../apis/nationalGoverningBody';
import { DataAttributes, IncludedAttributes } from '../../schemas/getNationalGoverningBodySchema';
import { AppThunk } from '../../store';

export interface SingleNationalGoverningBodyState {
  ngb: DataAttributes | null;
  id: string | null;
  error: string | null;
  isLoading: boolean;
  socialAccounts: IncludedAttributes[];
  teamCount: number;
  refereeCount: number;
}

const initialState: SingleNationalGoverningBodyState = {
  error: null,
  id: null,
  isLoading: false,
  ngb: null,
  refereeCount: 0,
  socialAccounts: [],
  teamCount: 0,
}

const nationalGoverningBody = createSlice({
  initialState,
  name: 'nationalGoverningBody',
  reducers: {
    getNationalGoverningBodyStart(state: SingleNationalGoverningBodyState) {
      state.isLoading = true;
    },
    getNationalGoverningBodySuccess(state: SingleNationalGoverningBodyState, action: PayloadAction<NgbResponse>) {
      state.isLoading = false;
      state.ngb = action.payload.nationalGoverningBody
      state.id = action.payload.id
      state.socialAccounts = action.payload.socialAccounts
      state.teamCount = action.payload.teamCount
      state.refereeCount = action.payload.refereeCount
    },
    getNationalGoverningBodyFailure(state: SingleNationalGoverningBodyState, action: PayloadAction<string>) {
      state.isLoading = false;
      state.ngb = null;
      state.error = action.payload;
      state.id = null;
      state.socialAccounts = [];
      state.teamCount = 0
      state.refereeCount = 0
    }
  }
})

export const {
  getNationalGoverningBodyFailure,
  getNationalGoverningBodyStart,
  getNationalGoverningBodySuccess,
} = nationalGoverningBody.actions

export const getNationalGoverningBody = (id: number): AppThunk => async dispatch => {
  try {
    dispatch(getNationalGoverningBodyStart())
    const ngbResponse = await getNgbApi(id)
    dispatch(getNationalGoverningBodySuccess(ngbResponse))
  } catch (err) {
    dispatch(getNationalGoverningBodyFailure(err))
  }
}

export default nationalGoverningBody.reducer
