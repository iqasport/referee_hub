import { faTrash } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import React, { useState } from "react";
import { useDispatch } from "react-redux";

import { deleteQuestion } from "MainApp/modules/question/question";
import { GetQuestionsSchemaDatum, Included } from "MainApp/schemas/getQuestionsSchema";

import WarningModal from "../modals/WarningModal";
import Answer from "./Answer";

interface QuestionProps {
  question: GetQuestionsSchemaDatum;
  index: number;
  answers: Included[];
}

enum ActiveTab {
  Answers = "answers",
  Details = "details",
}

const Question = (props: QuestionProps) => {
  const { question, index, answers } = props;

  const [activeTab, setActiveTab] = useState<ActiveTab>(ActiveTab.Answers);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const dispatch = useDispatch();

  const isAnswersActive = activeTab === ActiveTab.Answers;
  const isDetailsActive = activeTab === ActiveTab.Details;

  const handleTabClick = (newTab: ActiveTab) => () => setActiveTab(newTab);
  const handleDelete = () => {
    dispatch(deleteQuestion(question.id));
  };
  const handleDeleteClick = () => setConfirmOpen(true);
  const handleConfirmClose = () => setConfirmOpen(false);

  const renderAnswer = (answer: Included) => <Answer key={answer.id} answer={answer} />;

  const renderAnswers = () => {
    return (
      <div>
        <ol className="list-decimal">{answers.map(renderAnswer)}</ol>
      </div>
    );
  };

  const renderDetails = () => {
    return (
      <div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">
            post-test explanation
          </label>
          <p>{question.attributes.feedback}</p>
        </div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">total points</label>
          <p>{question.attributes.pointsAvailable}</p>
        </div>
      </div>
    );
  };

  const renderDescription = () => {
    return <div dangerouslySetInnerHTML={{ __html: question.attributes.description }} />;
  };

  return (
    <>
      <div className="flex items-start w-full my-4">
        <div className="question-index">
          <div>{index}</div>
        </div>
        <div className="w-11/12 border border-gray-300">
          <h4 className="w-full py-2 px-4 border border-gray-400">{renderDescription()}</h4>
          <div className="flex w-full justify-between mt-4 px-4 min-h-40">
            <div className="w-2/3 px-4">{isAnswersActive ? renderAnswers() : renderDetails()}</div>
            <div className="flex h-12 flex-wrap">
              <button
                onClick={handleTabClick(ActiveTab.Answers)}
                className={classnames("button-tab", {
                  ["active-button-tab"]: isAnswersActive,
                })}
              >
                answers
              </button>
              <button
                onClick={handleTabClick(ActiveTab.Details)}
                className={classnames("button-tab", {
                  ["active-button-tab"]: isDetailsActive,
                })}
              >
                explanation
              </button>
            </div>
          </div>
        </div>
        <div className="ml-4">
          <button
            type="button"
            className="bg-red-600 text-white rounded h-8 w-8"
            onClick={handleDeleteClick}
          >
            <FontAwesomeIcon icon={faTrash} />
          </button>
        </div>
      </div>
      <WarningModal
        action="delete"
        dataType="question"
        open={confirmOpen}
        onCancel={handleConfirmClose}
        onConfirm={handleDelete}
      />
    </>
  );
};

export default Question;
