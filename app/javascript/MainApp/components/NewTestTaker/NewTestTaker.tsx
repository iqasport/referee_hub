import React from 'react'

import { FormattedQuestion } from 'MainApp/pages/NewStartTest/NewStartTest'
import { Included } from 'MainApp/schemas/getQuestionsSchema'

import Answer from '../Answer'
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
  const { currentQuestion, onAnswerSelect, timeLimit, totalQuestionCount, currentIndex, onTimeLimitMet } = props
  if (!currentQuestion) return null

  const handleAnswerChange = (answerId) => onAnswerSelect(answerId)

  const renderAnswer = (answer: Included) => {
    const isSelected = currentQuestion.selectedAnswer === answer.id

    return (
      <Answer
        key={answer.id}
        isCorrect={isSelected}
        onCorrectChange={handleAnswerChange}
        isEditable={false}
        values={{
          description: answer.attributes.description,
          id: answer.id,
        }}
      />
    )
  }

  return (
    <div>
      <ProgressBar currentIndex={currentIndex} total={totalQuestionCount} />
      <Counter timeLimit={timeLimit} onTimeLimitMet={onTimeLimitMet} />
      <div dangerouslySetInnerHTML={{ __html: currentQuestion.description }} />
      <div className="w-full h-px border border-navy-blue" />
      <div className="w-1/2 my-0 mx-auto">
        {currentQuestion.answers.map(renderAnswer)}
      </div>
    </div>
  )
}

export default NewTestTaker
