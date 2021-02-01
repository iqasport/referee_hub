import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import {
  createSession as createSessionApi,
  CreateSessionRequest,
  SessionResponse,
} from "MainApp/apis/checkout";
import { AppThunk } from "MainApp/store";

export interface CheckoutState {
  sessionId: string | null;
  error: string | null;
  isLoading: boolean;
}

const initialState: CheckoutState = {
  error: null,
  isLoading: false,
  sessionId: null,
};

const checkout = createSlice({
  initialState,
  name: "checkout",
  reducers: {
    createSessionStart(state: CheckoutState) {
      state.isLoading = true;
    },
    createSessionSuccess(state: CheckoutState, action: PayloadAction<SessionResponse>) {
      state.error = null;
      state.isLoading = false;
      state.sessionId = action.payload.sessionId;
    },
    createSessionFailure(state: CheckoutState, action: PayloadAction<string>) {
      state.error = action.payload;
      state.isLoading = false;
      state.sessionId = null;
    },
  },
});

export const { createSessionFailure, createSessionStart, createSessionSuccess } = checkout.actions;

export const createSession = (session: CreateSessionRequest): AppThunk => async (dispatch) => {
  try {
    dispatch(createSessionStart());
    const sessionResponse = await createSessionApi(session);
    dispatch(createSessionSuccess(sessionResponse));
  } catch (err) {
    dispatch(createSessionFailure(err.toString()));
  }
};

export default checkout.reducer;
