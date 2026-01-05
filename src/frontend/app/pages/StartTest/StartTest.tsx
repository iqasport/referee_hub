import { faEdit, faCheckCircle, faTimesCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { DateTime, Duration } from "luxon";
import React, { useEffect, useState } from "react";

import NewTestTaker from "../../components/TestTaker";
import { useGetTestDetailsQuery, useStartTestMutation, useSubmitTestMutation, Question, SubmittedTestAnswer } from "../../store/serviceApi";
import { getErrorString } from "../../utils/errorUtils";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";
import Loader from "../../components/Loader";
import ResultChart from "../../components/TestResultCards/ResultChart";

export type FormattedQuestion = {
  questionId: string;
  description: string;
  answers: { answerId: string; description: string }[];
  selectedAnswer: string;
};

const formatQuestions = (
  questions: Question[]
): FormattedQuestion[] => {
  return questions.map(
    (question): FormattedQuestion => ({
        answers: question.answers.map(answer => ({ answerId: answer.answerId.toString(), description: answer.htmlText })),
        description: question.htmlText,
        questionId: question.questionId.toString(),
        selectedAnswer: null,
      }));
}

const buildRefereeAnswers = (testQuestions: FormattedQuestion[]): SubmittedTestAnswer[] => {
  return testQuestions.map((question: FormattedQuestion) => ({
    // the plus here converts string to a number
    answerId: +question.selectedAnswer,
    questionId: +question.questionId,
  }));
};

enum TestAction {
  Prev = "prev",
  Next = "next",
  Start = "start",
  Finish = "finish",
}

type TestParams = {
  testId: string;
  refereeId: string;
};

const StartTest = () => {
  const { refereeId, testId } = useNavigationParams<"refereeId" | "testId">();

  const [testQuestions, setTestQuestions] = useState<FormattedQuestion[]>([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [startedAt, setStartedAt] = useState<DateTime>(null);
  const [finishedAt, setFinishedAt] = useState<DateTime>(null);
  const [currentQuestion, setCurrentQuestion] = useState<FormattedQuestion>();
  const [currentTime, setCurrentTime] = useState<{ minutes: number; seconds: number }>({
    minutes: 0,
    seconds: 0,
  });

  const navigate = useNavigate();
  
  // TODO: create a single call endpoint?
  const { currentData: test, isLoading: testDetailsLoading, error: getTestError } = useGetTestDetailsQuery({ testId });

  const [startTest, { data: startedTest, error: startTestError }] = useStartTestMutation();
  const [submitTest, { data: submittedTest, error: submitTestError }] = useSubmitTestMutation();

  const timeLimitInMinutes = test && Duration.fromISOTime(test.timeLimit).as("minutes");

  useEffect(() => {
    if (startedTest) {
      const formattedQuestions = formatQuestions(startedTest.questions);
      setTestQuestions(formattedQuestions);
      setCurrentQuestion(formattedQuestions[0]);
      setStartedAt(DateTime.utc());
    }
  }, [startedTest]);

  useEffect(() => {
    if (submittedTest && !finishedAt) {
      setFinishedAt(DateTime.utc());
    }
  }, [submittedTest, finishedAt]);

  const isLastQuestion = currentQuestionIndex === testQuestions.length - 1;
  const isFirstQuestion = currentQuestionIndex === 0;

  const handleGoBack = () => {
    navigate(`/referees/${refereeId}`);
  };

  const handleStartTest = () => {
    startTest({ testId });
  };

  const handleNext = () => {
    const newIndex = currentQuestionIndex + 1;
    setCurrentQuestionIndex(newIndex);
    setCurrentQuestion(testQuestions[newIndex]);
  };

  const handlePrev = () => {
    const newIndex = currentQuestionIndex - 1;
    setCurrentQuestionIndex(newIndex);
    setCurrentQuestion(testQuestions[newIndex]);
  };

  const handleFinish = (minutes?: number, seconds?: number) => {
    let duration = Duration.fromObject({
      minutes: currentTime.minutes,
      seconds: currentTime.seconds,
    });
    if (minutes || seconds) {
      duration = Duration.fromObject({ minutes, seconds });
    }
    const finishedTime = startedAt.plus(duration);

    submitTest({testId, refereeTestSubmitModel: {
      startedAt: startedAt.toISO(),
      answers: buildRefereeAnswers(testQuestions),
    }});
    setFinishedAt(finishedTime);
    setTestQuestions([]);
    setStartedAt(null);
  };

  const handleTestAction = (action: TestAction) => () => {
    switch (action) {
      case TestAction.Start:
        handleStartTest();
        break;
      case TestAction.Next:
        handleNext();
        break;
      case TestAction.Prev:
        handlePrev();
        break;
      case TestAction.Finish:
        handleFinish();
        break;
    }
  };

  const handleAnswerSelect = (answerId: string) => {
    const updatedQuestion = { ...testQuestions[currentQuestionIndex], selectedAnswer: answerId };
    testQuestions.splice(currentQuestionIndex, 1, updatedQuestion);
    setTestQuestions(testQuestions);
    setCurrentQuestion(updatedQuestion);
  };

  const renderButtons = () => {
    const nextContent = isLastQuestion ? "Finish" : "Next";
    const isNextDisabled = !currentQuestion?.selectedAnswer;
    const nextAction = isLastQuestion ? TestAction.Finish : TestAction.Next;

    return (
      <div className="flex w-1/2 justify-center mx-auto mt-12">
        {!startedAt && (
          <button className="button-tab uppercase" onClick={handleGoBack}>
            Go Back To Profile
          </button>
        )}
        {!startedAt && !finishedAt && (
          <button className="green-button-outline" onClick={handleTestAction(TestAction.Start)}>
            Start Test
          </button>
        )}
        {startedAt && !isFirstQuestion && (
          <button className="button-tab uppercase" onClick={handleTestAction(TestAction.Prev)}>
            Previous
          </button>
        )}
        {startedAt && (
          <button
            className="green-button-outline"
            disabled={isNextDisabled}
            onClick={handleTestAction(nextAction)}
          >
            {nextContent}
          </button>
        )}
      </div>
    );
  };

  const renderTest = () => {
    return (
      <NewTestTaker
        timeLimit={timeLimitInMinutes}
        currentQuestion={currentQuestion}
        onAnswerSelect={handleAnswerSelect}
        totalQuestionCount={testQuestions.length}
        currentIndex={currentQuestionIndex + 1}
        onTimeLimitMet={handleFinish}
        setCurrentTime={setCurrentTime}
      />
    );
  };

  const renderFinish = () => {
    const passed = submittedTest?.passed;
    const scoredPercentage = submittedTest?.scoredPercentage;
    const passPercentage = submittedTest?.passPercentage;
    const resultIcon = passed ? faCheckCircle : faTimesCircle;
    const resultText = passed ? "Passed" : "Failed";
    const resultColorClass = passed ? "text-green-darker" : "text-red-500";
    const iconColorClass = passed ? "text-green-darker" : "text-red-500";

    return (
      <div>
        <div className="flex flex-col items-center">
          <FontAwesomeIcon icon={resultIcon} className={`${iconColorClass} text-6xl mb-4`} />
          <h1 className={`text-4xl font-bold mb-2 ${resultColorClass}`}>{resultText}</h1>
          <h2 className="text-2xl font-bold my-4">{test?.title}</h2>
          <span className="italic text-gray-600">{test?.description}</span>
        </div>
        <div className="w-full h-px border-navy-blue border-t my-6" />
        {submitTestError && <h4 className="font-bold text-red-500 my-4">{getErrorString(submitTestError)}</h4>}
        
        {submittedTest && (
          <div className="flex flex-col items-center my-6">
            <div className="w-64 h-64 mb-4">
              <ResultChart minimum={passPercentage || 0} actual={scoredPercentage || 0} />
            </div>
            <div className="text-center">
              <h3 className="text-xl font-bold mb-2">Your Score: {scoredPercentage}%</h3>
              {!passed && passPercentage && (
                <h4 className="text-lg text-gray-700">
                  Required to Pass: {passPercentage}%
                </h4>
              )}
            </div>
          </div>
        )}

        <div className="text-center mt-6">
          <h4 className="font-bold my-4">
            Results will be emailed to you through the email you registered your account with.
          </h4>
          <h4 className="font-bold my-4">
            If you do not see the results for this test attempt after an hour please reach out to
            <a className="text-blue-darker hover:text-blue" href="mailto:refhub@iqasport.org">
              {" "}
              refhub@iqasport.org
            </a>
          </h4>
        </div>
      </div>
    );
  };

  const renderStart = () => (
    <div>
      <div className="flex flex-col items-center my-4">
        <FontAwesomeIcon icon={faEdit} className="text-blue-darker text-6xl" />
        <h1 className="text-3xl font-bold my-4">{test?.title}</h1>
        <span className="italic text-gray-600">{test?.description}</span>
      </div>
      <div className="w-full h-px border-t border-navy-blue" />
      {startTestError && <h4 className="font-bold text-red-500 my-4">{getErrorString(startTestError)}</h4>}
      <h4 className="font-bold my-4">{`The test has ${test.questionsCount} questions. You will have to answer ${Math.ceil(test.questionsCount * test.passPercentage / 100)} questions correctly to pass.`}</h4>
      <h4 className="font-bold my-4">{`You will have ${timeLimitInMinutes} minutes to complete this test.`}</h4>
      <h4 className="font-bold my-4">Once you begin you may not exit the test.</h4>
      <h4 className="font-bold my-4">If you go over the time limit you will not pass the test.</h4>
      <h4 className="font-bold my-4">
        If you need more time to complete this test due to documented test taking challenges please
        contact
        <a className="text-blue-darker hover:text-blue" href="mailto:refhub@iqasport.org">
          {" "}
          refhub@iqasport.org
        </a>
      </h4>
    </div>
  );

  if (!test) {
    return <Loader></Loader>
  }

  const renderMainContent = () => {
    if (finishedAt) return renderFinish();
    if (startedAt) return renderTest();

    return renderStart();
  };

  return (
    <div
      className="text-center flex flex-col justify-between my-8 mx-auto w-2/3"
      style={{ minHeight: "450px" }}
    >
      {renderMainContent()}
      {renderButtons()}
    </div>
  );
};

export default StartTest;

