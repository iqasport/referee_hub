import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { QuestionResponse, updateQuestion as updateQuestionApi, UpdateQuestionRequest } from 'MainApp/apis/question'
import { Data } from 'MainApp/schemas/getQuestionSchema'

import { AppThunk } from '../../store'

export interface QuestionState {
  question: Data;
  error: string | null;
  isLoading: boolean;
}

const initialState: QuestionState = {
  error: null,
  isLoading: false,
  question: null
}

function questionSuccess(state: QuestionState, action: PayloadAction<QuestionResponse>) {
  state.question = action.payload.question
  state.isLoading = false
  state.error = null
}

function questionFailure(state: QuestionState, action: PayloadAction<string>) {
  state.question = null
  state.isLoading = false
  state.error = action.payload
}

const question = createSlice({
  initialState,
  name: 'question',
  reducers: {
    updateQuestionFailure: questionFailure,
    updateQuestionStart(state: QuestionState) {
      state.isLoading = true
    },
    updateQuestionSuccess: questionSuccess
  }
})

export const {
  updateQuestionFailure,
  updateQuestionStart,
  updateQuestionSuccess
} = question.actions

export const updateQuestion = (questionId: string, newQuestion: UpdateQuestionRequest): AppThunk => async dispatch => {
  try {
    dispatch(updateQuestionStart())
    const questionResponse = await updateQuestionApi(questionId, newQuestion)
    dispatch(updateQuestionSuccess(questionResponse))
  } catch (err) {
    dispatch(updateQuestionFailure(err.toString()))
  }
}

export default question.reducer
