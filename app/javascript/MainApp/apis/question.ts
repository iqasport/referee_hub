import { GetQuestionsSchemaDatum, Meta } from "MainApp/schemas/getQuestionsSchema";

export interface QuestionsResponse {
  questions: GetQuestionsSchemaDatum[];
  meta: Meta;
}
