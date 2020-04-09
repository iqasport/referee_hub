import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { getCurrentUser, IUserResponse } from '../../api';
import * as schema from '../../schema'
import { AppThunk } from '../../store';

interface CurrentUserState {
  currentUser: schema.Users | null;
  roles: string[];
  error: string | null;
}

const initialState: CurrentUserState = {
  currentUser: null,
  error: null,
  roles: [],
}

const currentUser = createSlice({
  initialState,
  name: 'currentUser',
  reducers: {
    getCurrentUserSuccess(state, action: PayloadAction<IUserResponse>) {
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
