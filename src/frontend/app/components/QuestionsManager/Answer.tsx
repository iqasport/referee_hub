import { faCheckCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

interface AnswerProps {
  description: string;
  correct: boolean;
}

const Answer = (props: AnswerProps) => {
  const { description, correct } = props;

  return (
    <li className="flex flex-row w-full my-4">
      {correct && (
        <div className="mr-2 font-bold">
          <FontAwesomeIcon icon={faCheckCircle} />
        </div>
      )}
      <div dangerouslySetInnerHTML={{ __html: description }} />
    </li>
  );
};

export default Answer;
