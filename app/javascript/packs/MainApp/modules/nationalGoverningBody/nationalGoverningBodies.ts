import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { getNationalGoverningBodies as getNgbsApi, NgbResponse } from '../../apis/nationalGoverningBody';
import { Datum } from '../../schemas/getNationalGoverningBodiesSchema';
import { AppThunk } from '../../store'

export interface NationalGoverningBodyState {
  nationalGoverningBodies: Datum[];
  error: string | null;
  isLoading: boolean;
}

const initialState: NationalGoverningBodyState = {
  error: null,
  isLoading: false,
  nationalGoverningBodies: [],
}

const nationalGoverningBody = createSlice({
  initialState,
  name: 'nationalGoverningBodies',
  reducers: {
    getNationalGoverningBodiesStart(state: NationalGoverningBodyState) {
      state.isLoading = true;
    },
    getNationalGoverningBodiesSuccess(state: NationalGoverningBodyState, action: PayloadAction<NgbResponse>) {
      state.isLoading = false
      state.nationalGoverningBodies = action.payload.nationalGoverningBodies
    },
    getNationalGoverningBodiesFailure(state: NationalGoverningBodyState, action: PayloadAction<string>) {
      state.isLoading = false
      state.nationalGoverningBodies = []
      state.error = action.payload
    }
  }
})

export const {
  getNationalGoverningBodiesFailure,
  getNationalGoverningBodiesStart,
  getNationalGoverningBodiesSuccess,
} = nationalGoverningBody.actions

export const getNationalGoverningBodies = (): AppThunk => async dispatch => {
  try {
    dispatch(getNationalGoverningBodiesStart())
    const ngbResponse = await getNgbsApi()
    dispatch(getNationalGoverningBodiesSuccess(ngbResponse))
  } catch (err) {
    dispatch(getNationalGoverningBodiesFailure(err))
  }
}

export default nationalGoverningBody.reducer
