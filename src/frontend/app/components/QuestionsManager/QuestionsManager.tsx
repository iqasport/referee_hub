import React from "react";
import { useGetTestQuestionsQuery } from "../../store/serviceApi";
import Question from "./Question";
import Loader from "../Loader";

interface QuestionsManagerProps {
  testId: string;
}

const QuestionsManager = (props: QuestionsManagerProps) => {
  const { testId } = props;

  const { data: questions } = useGetTestQuestionsQuery({ testId });

  if (!questions) {
    return <Loader />
  }

  return (
    <>
      {questions.map((question) => {
        return (
          <Question
            key={question.sequenceNum}
            question={question}
          />
        );
      })}
    </>
  );
};

export default QuestionsManager;
