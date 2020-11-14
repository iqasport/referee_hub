import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { deleteQuestion as deleteQuestionApi, QuestionResponse, updateQuestion as updateQuestionApi, UpdateQuestionRequest } from 'MainApp/apis/question'
import { Data } from 'MainApp/schemas/getQuestionSchema'

import { AppThunk } from '../../store'
import { getQuestions } from './questions'

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
    deleteQuestionFailure: questionFailure,
    deleteQuestionStart(state: QuestionState) {
      state.isLoading = true
    },
    deleteQuestionSuccess: questionSuccess,
    updateQuestionFailure: questionFailure,
    updateQuestionStart(state: QuestionState) {
      state.isLoading = true
    },
    updateQuestionSuccess: questionSuccess
  }
})

export const {
  deleteQuestionFailure,
  deleteQuestionStart,
  deleteQuestionSuccess,
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

export const deleteQuestion = (questionId: string): AppThunk => async dispatch => {
  try {
    dispatch(deleteQuestionStart())
    const questionResponse = await deleteQuestionApi(questionId)
    dispatch(deleteQuestionSuccess(questionResponse))
    dispatch(getQuestions(questionResponse.question.attributes.testId.toString(10)))
  } catch (err) {
    dispatch(deleteQuestionFailure(err.toString()))
  }
}

export default question.reducer
