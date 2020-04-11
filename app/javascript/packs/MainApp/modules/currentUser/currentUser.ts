import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { getCurrentUser, UserResponse } from '../../apis/user';
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
  name: 'currentUser',
  reducers: {
    getCurrentUserSuccess(state, action: PayloadAction<UserResponse>) {
      state.currentUser = action.payload.user
      state.roles = action.payload.roles
      state.error = null
    },
    getCurrentUserFailure(state, action: PayloadAction<string>) {
      state.currentUser = null
      state.roles = []
      state.error = action.payload
    }
  }
})

export const {
  getCurrentUserSuccess,
  getCurrentUserFailure,
} = currentUser.actions

export const fetchCurrentUser = (): AppThunk => async dispatch => {
  try {
    const userResponse = await getCurrentUser()
    dispatch(getCurrentUserSuccess(userResponse))
  } catch (err) {
    dispatch(getCurrentUserFailure(err.toString()))
  }
}

export default currentUser.reducer
