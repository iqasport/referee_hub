import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import {
  AnnotatedNgb,
  getNationalGoverningBodies as getNgbsApi,
  importNgbs as importNgbsApi,
  NgbsResponse
} from 'MainApp/apis/nationalGoverningBody';
import { HeadersMap } from 'MainApp/pages/ImportWizard/MapStep';
import { Meta } from 'MainApp/schemas/getNationalGoverningBodiesSchema';
import { AppThunk } from 'MainApp/store'

export interface NationalGoverningBodyState {
  nationalGoverningBodies: AnnotatedNgb[];
  meta: Meta;
  error: string | null;
  isLoading: boolean;
}

const initialState: NationalGoverningBodyState = {
  error: null,
  isLoading: false,
  meta: null,
  nationalGoverningBodies: [],
}

const nationalGoverningBodies = createSlice({
  initialState,
  name: 'nationalGoverningBodies',
  reducers: {
    getNationalGoverningBodiesStart(state: NationalGoverningBodyState) {
      state.isLoading = true;
    },
    getNationalGoverningBodiesSuccess(state: NationalGoverningBodyState, action: PayloadAction<NgbsResponse>) {
      state.isLoading = false
      state.nationalGoverningBodies = action.payload.nationalGoverningBodies
      state.meta = action.payload.meta
      state.error = null
    },
    getNationalGoverningBodiesFailure(state: NationalGoverningBodyState, action: PayloadAction<string>) {
      state.isLoading = false
      state.nationalGoverningBodies = []
      state.error = action.payload
      state.meta = null
    },
    importNgbsFailure(state: NationalGoverningBodyState, action: PayloadAction<string>) {
      state.isLoading = false
      state.nationalGoverningBodies = []
      state.error = action.payload
      state.meta = null
    },
    importNgbsStart(state: NationalGoverningBodyState) {
      state.isLoading = true;
    },
    importNgbsSuccess(state: NationalGoverningBodyState, action: PayloadAction<NgbsResponse>) {
      state.isLoading = false
      state.nationalGoverningBodies = action.payload.nationalGoverningBodies
      state.meta = action.payload.meta
      state.error = null
    }
  }
})

export const {
  getNationalGoverningBodiesFailure,
  getNationalGoverningBodiesStart,
  getNationalGoverningBodiesSuccess,
  importNgbsFailure,
  importNgbsStart,
  importNgbsSuccess,
} = nationalGoverningBodies.actions

export const getNationalGoverningBodies = (): AppThunk => async dispatch => {
  try {
    dispatch(getNationalGoverningBodiesStart())
    const ngbResponse = await getNgbsApi()
    dispatch(getNationalGoverningBodiesSuccess(ngbResponse))
  } catch (err) {
    dispatch(getNationalGoverningBodiesFailure(err.toString()))
  }
}

export const importNgbs = (uploadedFile: File, mappedData: HeadersMap): AppThunk => async dispatch => {
  try {
    dispatch(importNgbsStart())
    const ngbsResponse = await importNgbsApi(uploadedFile, mappedData)
    dispatch(importNgbsSuccess(ngbsResponse))
  } catch (err) {
    dispatch(importNgbsFailure(err.toString()))
  }
}

export default nationalGoverningBodies.reducer
