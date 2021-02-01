import { faCheckCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import { Included } from "MainApp/schemas/getQuestionsSchema";

interface AnswerProps {
  answer: Included;
}

const Answer = (props: AnswerProps) => {
  const { answer } = props;

  return (
    <li className="flex flex-row w-full my-4">
      {answer.attributes.correct && (
        <div className="mr-2 font-bold">
          <FontAwesomeIcon icon={faCheckCircle} />
        </div>
      )}
      <div dangerouslySetInnerHTML={{ __html: answer.attributes.description }} />
    </li>
  );
};

export default Answer;
