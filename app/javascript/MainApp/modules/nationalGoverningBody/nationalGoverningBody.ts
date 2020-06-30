import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { getNationalGoverningBody as getNgbApi, NgbResponse, updateLogo, updateNationalGoverningBody, UpdateNgbRequest } from '../../apis/nationalGoverningBody';
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
  stats: IncludedAttributes[];
}

const initialState: SingleNationalGoverningBodyState = {
  error: null,
  id: null,
  isLoading: false,
  ngb: null,
  refereeCount: 0,
  socialAccounts: [],
  stats: [],
  teamCount: 0,
}

function ngbSuccess(state: SingleNationalGoverningBodyState, action: PayloadAction<NgbResponse>) {
  state.isLoading = false;
  state.ngb = action.payload.nationalGoverningBody
  state.id = action.payload.id
  state.socialAccounts = action.payload.socialAccounts
  state.teamCount = action.payload.teamCount
  state.refereeCount = action.payload.refereeCount
  state.stats = action.payload.stats
}

function ngbFailure(state: SingleNationalGoverningBodyState, action: PayloadAction<string>) {
  state.isLoading = false;
  state.ngb = null;
  state.error = action.payload;
  state.id = null;
  state.socialAccounts = [];
  state.teamCount = 0
  state.refereeCount = 0
  state.stats = []
}

const nationalGoverningBody = createSlice({
  initialState,
  name: 'nationalGoverningBody',
  reducers: {
    getNationalGoverningBodyFailure: ngbFailure,
    getNationalGoverningBodyStart(state: SingleNationalGoverningBodyState) {
      state.isLoading = true;
    },
    getNationalGoverningBodySuccess: ngbSuccess,
    updateLogoFailure: ngbFailure,
    updateLogoSuccess: ngbSuccess,
    updateNgbFailure: ngbFailure,
    updateNgbStart(state: SingleNationalGoverningBodyState) {
      state.isLoading = true;
    },
    updateNgbSuccess: ngbSuccess,
  }
})

export const {
  getNationalGoverningBodyFailure,
  getNationalGoverningBodyStart,
  getNationalGoverningBodySuccess,
  updateLogoFailure,
  updateLogoSuccess,
  updateNgbFailure,
  updateNgbStart,
  updateNgbSuccess,
} = nationalGoverningBody.actions

export const getNationalGoverningBody = (id: number): AppThunk => async dispatch => {
  try {
    dispatch(getNationalGoverningBodyStart())
    const ngbResponse = await getNgbApi(id)
    dispatch(getNationalGoverningBodySuccess(ngbResponse))
  } catch (err) {
    dispatch(getNationalGoverningBodyFailure(err.toString()))
  }
}

export const updateNgbLogo = (ngbId: string, logo: File): AppThunk => async dispatch => {
  try {
    const ngbResponse = await updateLogo(ngbId, logo)
    dispatch(updateLogoSuccess(ngbResponse))
  } catch (err) {
    dispatch(updateLogoFailure(err.toString()))
  }
}

export const updateNgb = (id: number, ngb: UpdateNgbRequest): AppThunk => async dispatch => {
  try {
    dispatch(updateNgbStart())
    const ngbResponse = await updateNationalGoverningBody(id, ngb)
    dispatch(updateNgbSuccess(ngbResponse))
  } catch (err) {
    dispatch(updateNgbFailure(err.toString()))
  }
}

export default nationalGoverningBody.reducer
