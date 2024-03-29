import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";

import { getQuestions } from "../../modules/question/questions";
import { RootState } from "../../rootReducer";
import Question from "./Question";
import { AppDispatch } from "../../store";

interface QuestionsManagerProps {
  testId: string;
}

const QuestionsManager = (props: QuestionsManagerProps) => {
  const { testId } = props;

  const dispatch = useDispatch<AppDispatch>();
  const { questions, answers } = useSelector((state: RootState) => state.questions);

  useEffect(() => {
    dispatch(getQuestions(testId));
  }, [testId]);

  const findAnswers = (answerIds: string[]) => {
    if (!answerIds) return [];

    return answers?.filter((answer) => answerIds.includes(answer.id));
  };

  return (
    <div>
      {questions.map((question, index) => {
        const questionAnswers = findAnswers(
          question.relationships.answers.data.map((answer): string => answer.id)
        );
        return (
          <Question
            key={question.id}
            question={question}
            index={index + 1}
            answers={questionAnswers}
          />
        );
      })}
    </div>
  );
};

export default QuestionsManager;
