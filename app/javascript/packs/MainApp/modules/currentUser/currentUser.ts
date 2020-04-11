import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import {getReferee as getRefereeApi} from '../../apis/referee';
import { getCurrentUser, updatePolicyAcceptance, UserResponse } from '../../apis/user';
import { DataAttributes } from '../../schemas/currentUserSchema';
import { AppThunk } from '../../store';

interface CurrentUserState {
  currentUser: DataAttributes | null;
  roles: string[];
  id: string | null;
  error: string | null;
}

const initialState: CurrentUserState = {
  currentUser: null,
  error: null,
  id: null,
  roles: [],
}

const currentUser = createSlice({
  initialState,
  name: "currentUser",
  reducers: {
    getCurrentUserSuccess(state, action: PayloadAction<UserResponse>) {
      state.currentUser = action.payload.user;
      state.roles = action.payload.roles;
      state.error = null;
    },
    getCurrentUserFailure(state, action: PayloadAction<string>) {
      state.currentUser = null;
      state.roles = [];
      state.error = action.payload;
    },
    updatePolicySuccess(state, action: PayloadAction<UserResponse>) {
      state.currentUser = action.payload.user;
      state.roles = action.payload.roles;
      state.error = null;
    },
    updatePolicyFailure(state, action: PayloadAction<string>) {
      state.currentUser = null;
      state.roles = [];
      state.error = action.payload;
    },
  },
});

export const {
  getCurrentUserSuccess,
  getCurrentUserFailure,
  updatePolicySuccess,
  updatePolicyFailure
} = currentUser.actions

export const fetchCurrentUser = (): AppThunk => async dispatch => {
  try {
    const userResponse = await getCurrentUser()
    dispatch(getCurrentUserSuccess(userResponse))
  } catch (err) {
    dispatch(getCurrentUserFailure(err.toString()))
  }
}

export const updateUserPolicy = (userId: string, type: string): AppThunk => async dispatch => {
  try {
    const userResponse = await updatePolicyAcceptance(userId, type)
    dispatch(updatePolicySuccess(userResponse))
    getRefereeApi(userId)
  } catch (err) {
    dispatch(updatePolicyFailure(err))
  }
}

export default currentUser.reducer
