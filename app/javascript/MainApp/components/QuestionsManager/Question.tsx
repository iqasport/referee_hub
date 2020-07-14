import classnames from 'classnames'
import React, { useState } from 'react'

import { GetQuestionsSchemaDatum, Included } from 'MainApp/schemas/getQuestionsSchema';

interface QuestionProps {
  question: GetQuestionsSchemaDatum;
  index: number;
  answers: Included[];
}

enum ActiveTab {
  Answers = 'answers',
  Details = 'details'
}

const Question = (props: QuestionProps) => {
  const { question, index, answers } = props

  const [activeTab, setActiveTab] = useState<ActiveTab>(ActiveTab.Answers)
  const isAnswersActive = activeTab === ActiveTab.Answers
  const isDetailsActive = activeTab === ActiveTab.Details

  const handleTabClick = (newTab: ActiveTab) => () => setActiveTab(newTab)
  const renderAnswer = (answer: Included) => {
    return (
      <li key={answer.id} className="my-4">
        <p className={classnames({ "font-bold": answer.attributes.correct })}>
          {answer.attributes.description}
        </p>
      </li>
    )
  }

  const renderAnswers = () => {
    return (
      <ol className="list-decimal">
        {answers.map(renderAnswer)}
      </ol>
    )
  }

  const renderDetails = () => {
    return (
      <div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">post-test explanation</label>
          <p>{question.attributes.feedback}</p>
        </div>
        <div className="my-4">
          <label className="uppercase text-md font-hairline text-gray-400">total points</label>
          <p>{question.attributes.pointsAvailable}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="flex items-start w-full my-4">
      <div
        className="h-8 w-8 rounded-full bg-green border border-green-darker text-white mr-4 flex items-center justify-center">
        <div>{index}</div>
      </div>
      <div className="w-11/12 border border-gray-300">
        <h4 className="w-full py-2 px-4 border border-gray-400">{question.attributes.description}</h4>
        <div className="flex w-full justify-between mt-4 px-4 min-h-40">
          <div className="w-2/3 px-4">
            {isAnswersActive ? renderAnswers() : renderDetails()}
          </div>
          <div className="flex h-12">
            <button
              onClick={handleTabClick(ActiveTab.Answers)}
              className={classnames('button-tab', { ['active-button-tab']: isAnswersActive })}
            >
              answers
            </button>
            <button
              onClick={handleTabClick(ActiveTab.Details)}
              className={classnames('button-tab', { ['active-button-tab']: isDetailsActive })}
            >
              explanation
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Question
