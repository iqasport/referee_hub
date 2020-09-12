import React, { useEffect, useState } from 'react'

import { FormattedQuestion } from 'MainApp/pages/StartTest/StartTest'
import { Included } from 'MainApp/schemas/getQuestionsSchema'

import Counter from '../Counter'
import ProgressBar from '../ProgressBar'

interface TestTakerProps {
  currentQuestion: FormattedQuestion;
  onAnswerSelect: (answerId: string) => void;
  timeLimit: number;
  totalQuestionCount: number;
  currentIndex: number;
  onTimeLimitMet: () => void;
}

const NewTestTaker = (props: TestTakerProps) => {
  const {
    currentQuestion, onAnswerSelect, timeLimit, totalQuestionCount, currentIndex, onTimeLimitMet
  } = props

  const [selectedAnswer, setSelectedAnswer] = useState<string>()

  useEffect(() => {
    if (currentQuestion.selectedAnswer !== selectedAnswer) {
      setSelectedAnswer(currentQuestion.selectedAnswer)
    }
  }, [currentQuestion])

  const handleAnswerChange = (answerId: string) => () => onAnswerSelect(answerId)

  const renderAnswer = (answer: Included) => {
    const isSelected = selectedAnswer === answer.id

    return (
      <div className="flex my-4 items-center" key={answer.id}>
        <input
          type="checkbox"
          className="form-checkbox mx-4"
          onChange={handleAnswerChange(answer.id)}
          checked={isSelected}
        />
        <div
          className="text-left"
          dangerouslySetInnerHTML={{ __html: answer.attributes.description }}
        />
      </div>
    )
  }

  return (
    <div>
      <ProgressBar currentIndex={currentIndex} total={totalQuestionCount} />
      <Counter timeLimit={timeLimit} onTimeLimitMet={onTimeLimitMet} />
      <div className="my-8" dangerouslySetInnerHTML={{ __html: currentQuestion.description }} />
      <div className="w-full h-px border-t border-navy-blue my-8" />
      <div className="w-1/2 my-0 mx-auto">
        {currentQuestion.answers.map(renderAnswer)}
      </div>
    </div>
  )
}

export default NewTestTaker
