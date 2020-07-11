import { createSlice, PayloadAction } from '@reduxjs/toolkit'

import { QuestionsResponse } from 'MainApp/apis/question'
import { HeadersMap } from 'MainApp/pages/ImportWizard/MapStep'
import { GetQuestionsSchemaDatum, Meta } from 'MainApp/schemas/getQuestionsSchema'
import { importTests as importTestsApi } from '../../apis/test'
import { AppThunk } from '../../store'

export interface QuestionsState {
  questions: GetQuestionsSchemaDatum[];
  error: string | null;
  isLoading: boolean;
  meta?: Meta
}

const initialState: QuestionsState = {
  error: null,
  isLoading: false,
  questions: []
}

const questions = createSlice({
  initialState,
  name: 'questions',
  reducers: {
    importTestQuestionsStart(state: QuestionsState) {
      state.isLoading = true
    },
    importTestQuestionsSuccess(state: QuestionsState, action: PayloadAction<QuestionsResponse>) {
      state.questions = action.payload.questions
      state.isLoading = false
      state.error = null
      state.meta = action.payload.meta
    },
    importTestQuestionsFailure(state: QuestionsState, action: PayloadAction<string>) {
      state.questions = []
      state.isLoading = false
      state.error = action.payload
    }
  }
})

export const {
  importTestQuestionsFailure,
  importTestQuestionsStart,
  importTestQuestionsSuccess
} = questions.actions

export const importTestQuestions = (file: File, mappedData: HeadersMap, testId: string): AppThunk => async dispatch => {
  try {
    dispatch(importTestQuestionsStart())
    const questionsResponse = await importTestsApi(file, mappedData, testId)
    dispatch(importTestQuestionsSuccess(questionsResponse))
  } catch (err) {
    dispatch(importTestQuestionsFailure(err.toString()))
  }
}
export default questions.reducer
