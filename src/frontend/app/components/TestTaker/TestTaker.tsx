import React, { useEffect, useState } from "react";

import { FormattedQuestion } from "../../pages/StartTest/StartTest";

import Counter from "../Counter";
import ProgressBar from "../ProgressBar";

interface TestTakerProps {
  currentQuestion: FormattedQuestion;
  onAnswerSelect: (answerId: string) => void;
  timeLimit: number;
  totalQuestionCount: number;
  currentIndex: number;
  onTimeLimitMet: (minutes: number, seconds: number) => void;
  setCurrentTime: (currentTime: { minutes: number; seconds: number }) => void;
}

const TestTaker = (props: TestTakerProps) => {
  const {
    currentQuestion,
    onAnswerSelect,
    timeLimit,
    totalQuestionCount,
    currentIndex,
    onTimeLimitMet,
    setCurrentTime,
  } = props;

  const [selectedAnswer, setSelectedAnswer] = useState<string>();

  useEffect(() => {
    if (currentQuestion.selectedAnswer !== selectedAnswer) {
      setSelectedAnswer(currentQuestion.selectedAnswer);
    }
  }, [currentQuestion]);

  const handleAnswerChange = (answerId: string) => () => onAnswerSelect(answerId);

  const renderAnswer = (answer: { answerId: string; description: string }) => {
    const isSelected = selectedAnswer === answer.answerId;

    return (
      <div className="flex my-4 items-center" key={answer.answerId}>
        <input
          type="checkbox"
          className="form-checkbox mx-4"
          onChange={handleAnswerChange(answer.answerId)}
          checked={isSelected}
          id={`box_${answer.answerId}`}
        />
        <label
          className="text-left"
          htmlFor={`box_${answer.answerId}`}
          dangerouslySetInnerHTML={{ __html: answer.description }}
        />
      </div>
    );
  };

  return (
    <div>
      <ProgressBar currentIndex={currentIndex} total={totalQuestionCount} />
      <Counter
        timeLimit={timeLimit}
        onTimeLimitMet={onTimeLimitMet}
        setCurrentTime={setCurrentTime}
      />
      <div className="my-8" dangerouslySetInnerHTML={{ __html: currentQuestion.description }} />
      <div className="w-full h-px border-t border-navy-blue my-8" />
      <div className="w-1/2 my-0 mx-auto">{currentQuestion.answers.map(renderAnswer)}</div>
    </div>
  );
};

export default TestTaker;
