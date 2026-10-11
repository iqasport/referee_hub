import classnames from "classnames";
import React, { useState } from "react";
import { TestQuestionRecord, useSetQuestionDisabledMutation } from "../../store/serviceApi";
import Answer from "./Answer";

interface QuestionProps {
  question: TestQuestionRecord;
  testId: string;
}

enum ActiveTab {
  Answers = "answers",
  Details = "details",
}

const Question = (props: QuestionProps) => {
  const { question, testId } = props;

  const [activeTab, setActiveTab] = useState<ActiveTab>(ActiveTab.Answers);
  const [setQuestionDisabled, { isLoading: isTogglingDisabled }] =
    useSetQuestionDisabledMutation();

  const isAnswersActive = activeTab === ActiveTab.Answers;
  const isDetailsActive = activeTab === ActiveTab.Details;

  const handleTabClick = (newTab: ActiveTab) => () => setActiveTab(newTab);

  const handleToggleDisabled = async () => {
    if (question.sequenceNum == null) return;

    await setQuestionDisabled({
      testId,
      sequenceId: question.sequenceNum,
      body: !question.disabled,
    });
  };

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
    return <div dangerouslySetInnerHTML={{ __html: question.question ?? "" }} />;
  };

  return (
    <>
      <div className="flex items-start w-full my-4">
        <div className="question-index">
          <div>{question.sequenceNum}</div>
        </div>
        <div
          className={classnames("w-11/12 border border-gray-300", {
            ["opacity-50"]: question.disabled,
          })}
        >
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
            disabled={isTogglingDisabled}
            className={classnames("rounded h-8 px-3 text-white text-sm", {
              ["bg-gray-500"]: question.disabled,
              ["bg-red-600"]: !question.disabled,
            })}
            onClick={handleToggleDisabled}
          >
            {question.disabled ? "enable" : "disable"}
          </button>
        </div>
      </div>
    </>
  );
};

export default Question;
