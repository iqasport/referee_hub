import { GetQuestionsSchema, GetQuestionsSchemaDatum, Included, Meta } from "MainApp/schemas/getQuestionsSchema";
import { baseAxios } from "./utils";

export interface QuestionsResponse {
  questions: GetQuestionsSchemaDatum[];
  meta: Meta;
  answers: Included[];
}

export async function getQuestions(testId: string): Promise<QuestionsResponse> {
  const url = `tests/${testId}/questions`

  try {
    const questionsResponse = await baseAxios.get<GetQuestionsSchema>(url)

    return {
      answers: questionsResponse.data.included,
      meta: questionsResponse.data.meta,
      questions: questionsResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}
