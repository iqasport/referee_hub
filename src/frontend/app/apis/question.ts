import { Data, GetQuestionSchema } from "../schemas/getQuestionSchema";
import {
  DatumAttributes,
  GetQuestionsSchema,
  GetQuestionsSchemaDatum,
  Included,
  Meta,
} from "../schemas/getQuestionsSchema";
import { baseAxios } from "./utils";

export interface QuestionsResponse {
  questions: GetQuestionsSchemaDatum[];
  meta: Meta;
  answers: Included[];
}

export interface QuestionResponse {
  question: Data;
}

export interface UpdateQuestionRequest extends Omit<DatumAttributes, "testId"> {}

export async function getQuestions(testId: string): Promise<QuestionsResponse> {
  const url = `tests/${testId}/questions`;

  try {
    const questionsResponse = await baseAxios.get<GetQuestionsSchema>(url);

    return {
      answers: questionsResponse.data.included,
      meta: questionsResponse.data.meta,
      questions: questionsResponse.data.data,
    };
  } catch (err) {
    throw err;
  }
}

export async function updateQuestion(
  questionId: string,
  newQuestion: UpdateQuestionRequest
): Promise<QuestionResponse> {
  const url = `questions/${questionId}`;

  try {
    const questionResponse = await baseAxios.patch<GetQuestionSchema>(url, newQuestion);

    return {
      question: questionResponse.data.data,
    };
  } catch (err) {
    throw err;
  }
}

export async function deleteQuestion(questionId): Promise<QuestionResponse> {
  const url = `questions/${questionId}`;

  try {
    const questionResponse = await baseAxios.delete<GetQuestionSchema>(url);

    return {
      question: questionResponse.data.data,
    };
  } catch (err) {
    throw err;
  }
}
