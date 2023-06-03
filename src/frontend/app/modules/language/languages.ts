import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { getLanguages as getLanguagesApi, LanguagesResponse } from "../../apis/language";
import { Datum } from "../../schemas/getLanguagesSchema";
import { AppThunk } from "../../store";

export interface LanguagesState {
  languages: Datum[];
  isLoading: boolean;
  error: string | null;
}

const initialState: LanguagesState = {
  error: null,
  isLoading: false,
  languages: [],
};

const languages = createSlice({
  initialState,
  name: "languages",
  reducers: {
    getLanguagesFailure(state: LanguagesState, action: PayloadAction<string>) {
      state.isLoading = false;
      state.languages = [];
      state.error = action.payload;
    },
    getLanguagesStart(state: LanguagesState) {
      state.isLoading = true;
    },
    getLanguagesSuccess(state: LanguagesState, action: PayloadAction<LanguagesResponse>) {
      state.isLoading = false;
      state.error = null;
      state.languages = action.payload.languages;
    },
  },
});

export const { getLanguagesFailure, getLanguagesStart, getLanguagesSuccess } = languages.actions;

export const getLanguages = (): AppThunk => async (dispatch) => {
  try {
    dispatch(getLanguagesStart());
    const languagesResponse = await getLanguagesApi();
    dispatch(getLanguagesSuccess(languagesResponse));
  } catch (err) {
    dispatch(getLanguagesFailure(err.toString()));
  }
};

export default languages.reducer;
