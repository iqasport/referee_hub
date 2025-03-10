import classnames from "classnames";
import React, { useState } from "react";
import Answer from "./Answer";
import { TestQuestionRecord } from "../../store/serviceApi";

interface QuestionProps {
  question: TestQuestionRecord;
}

enum ActiveTab {
  Answers = "answers",
  Details = "details",
}

const Question = (props: QuestionProps) => {
  const { question } = props;

  const [activeTab, setActiveTab] = useState<ActiveTab>(ActiveTab.Answers);
  // const [confirmOpen, setConfirmOpen] = useState(false);

  const isAnswersActive = activeTab === ActiveTab.Answers;
  const isDetailsActive = activeTab === ActiveTab.Details;

  const handleTabClick = (newTab: ActiveTab) => () => setActiveTab(newTab);
  // const handleDeleteClick = () => setConfirmOpen(true);
  // const handleConfirmClose = () => setConfirmOpen(false);


  const renderAnswers = () => {
    return (
      <div>
        <ol className="list-decimal">
          <Answer key={1} description={question.answer1} correct={question.correct === 1} />
          <Answer key={2} description={question.answer2} correct={question.correct === 2} />
          <Answer key={3} description={question.answer3} correct={question.correct === 3} />
          <Answer key={4} description={question.answer4} correct={question.correct === 4} />
        </ol>
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
          <p>{question.feedback}</p>
        </div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">total points</label>
          <p>1</p>
        </div>
      </div>
    );
  };

  const renderDescription = () => {
    return <div dangerouslySetInnerHTML={{ __html: question.question }} />;
  };

  return (
    <>
      <div className="flex items-start w-full my-4">
        <div className="question-index">
          <div>{question.sequenceNum}</div>
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
        {/* <div className="ml-4">
          <button
            type="button"
            className="bg-red-600 text-white rounded h-8 w-8"
            onClick={handleDeleteClick}
          >
            <FontAwesomeIcon icon={faTrash} />
          </button>
        </div> */}
      </div>
      {/* <WarningModal
        action="delete"
        dataType="question"
        open={confirmOpen}
        onCancel={handleConfirmClose}
        onConfirm={handleDelete}
      /> */}
    </>
  );
};

export default Question;
