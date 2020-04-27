import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import {getReferee as getRefereeApi} from '../../apis/referee';
import { getCurrentUser, updateAvatar, updatePolicyAcceptance, UserResponse } from '../../apis/user';
import { DataAttributes } from '../../schemas/currentUserSchema';
import { AppThunk } from '../../store';
import { fetchReferee } from '../referee/referee';

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

function userSuccess(state: CurrentUserState, action: PayloadAction<UserResponse>) {
  state.currentUser = action.payload.user;
  state.roles = action.payload.roles;
  state.error = null;
}

function userFailure(state, action: PayloadAction<string>) {
  state.currentUser = null;
  state.roles = [];
  state.error = action.payload;
}

const currentUser = createSlice({
  initialState,
  name: "currentUser",
  reducers: {
    getCurrentUserFailure: userFailure,
    getCurrentUserSuccess: userSuccess,
    updateAvatarFailure: userFailure,
    updateAvatarSuccess: userSuccess,
    updatePolicyFailure: userFailure,
    updatePolicySuccess: userSuccess,
  },
});

export const {
  getCurrentUserSuccess,
  getCurrentUserFailure,
  updatePolicySuccess,
  updatePolicyFailure,
  updateAvatarFailure,
  updateAvatarSuccess,
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
    dispatch(fetchReferee(userId))
  } catch (err) {
    dispatch(updatePolicyFailure(err.toString()))
  }
}

export const updateUserAvatar = (userId: string, avatar: File): AppThunk => async dispatch => {
  try {
    const userResponse = await updateAvatar(userId, avatar)
    dispatch(updateAvatarSuccess(userResponse))
  } catch (err) {
    dispatch(updateAvatarFailure(err.toString()))
  }
}
export default currentUser.reducer
