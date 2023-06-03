import { faCheckSquare, faEdit } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { shuffle } from "lodash";
import { DateTime, Duration } from "luxon";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { RouteComponentProps, useHistory } from "react-router-dom";

import { FinishTestRequest, RefereeAnswer } from "MainApp/apis/job";
import NewTestTaker from "MainApp/components/TestTaker";
import { finishTest } from "MainApp/modules/job/job";
import { startTest } from "MainApp/modules/question/questions";
import { clearTest, getTest } from "MainApp/modules/test/single_test";
import { RootState } from "MainApp/rootReducer";
import { GetQuestionsSchemaDatum, Included } from "MainApp/schemas/getQuestionsSchema";

export type FormattedQuestion = {
  questionId: string;
  description: string;
  answers: Included[];
  selectedAnswer: string;
};

const formatQuestions = (
  questions: GetQuestionsSchemaDatum[],
  answers: Included[]
): FormattedQuestion[] => {
  return questions.map(
    (question): FormattedQuestion => {
      const filteredAnswers = answers.filter(
        (answer) => String(answer.attributes.questionId) === question.id
      );

      return {
        answers: shuffle(filteredAnswers),
        description: question.attributes.description,
        questionId: question.id,
        selectedAnswer: null,
      };
    }
  );
};

const buildRefereeAnswers = (testQuestions: FormattedQuestion[]): RefereeAnswer[] => {
  return testQuestions.map((question: FormattedQuestion) => ({
    answer_id: question.selectedAnswer,
    question_id: question.questionId,
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

const StartTest = (props: RouteComponentProps<TestParams>) => {
  const {
    match: {
      params: { refereeId, testId },
    },
  } = props;

  const [testQuestions, setTestQuestions] = useState<FormattedQuestion[]>([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [startedAt, setStartedAt] = useState<DateTime>(null);
  const [finishedAt, setFinishedAt] = useState<DateTime>(null);
  const [currentQuestion, setCurrentQuestion] = useState<FormattedQuestion>();
  const [currentTime, setCurrentTime] = useState<{ minutes: number; seconds: number }>({
    minutes: 0,
    seconds: 0,
  });

  const history = useHistory();
  const dispatch = useDispatch();
  const { test, error: testError, isLoading: testLoading } = useSelector(
    (state: RootState) => state.test,
    shallowEqual
  );
  const { questions, answers, isLoading: questionsLoading, error: questionsError } = useSelector(
    (state: RootState) => state.questions,
    shallowEqual
  );

  useEffect(() => {
    if (!test || test.id !== testId) {
      dispatch(getTest(testId));
    }
  }, [testId, test]);

  useEffect(() => {
    const questionsMatchTest = questions[0]?.attributes.testId === parseInt(testId, 10);
    if (!questionsLoading && questionsMatchTest) {
      const formattedQuestions = formatQuestions(questions, answers);
      setTestQuestions(formattedQuestions);
      setCurrentQuestion(formattedQuestions[0]);
      setStartedAt(DateTime.local());
    }
  }, [questions, answers, questionsLoading, testId]);

  const isLastQuestion = currentQuestionIndex === testQuestions.length - 1;
  const isFirstQuestion = currentQuestionIndex === 0;

  const handleGoBack = () => {
    dispatch(clearTest);
    history.push(`/referees/${refereeId}`);
  };

  const handleStartTest = () => {
    dispatch(startTest(testId));
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

    const request: FinishTestRequest = {
      finishedAt: finishedTime,
      refereeAnswers: buildRefereeAnswers(testQuestions),
      startedAt,
    };

    dispatch(finishTest(testId, request));
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
        timeLimit={test?.attributes.timeLimit}
        currentQuestion={currentQuestion}
        onAnswerSelect={handleAnswerSelect}
        totalQuestionCount={testQuestions.length}
        currentIndex={currentQuestionIndex + 1}
        onTimeLimitMet={handleFinish}
        setCurrentTime={setCurrentTime}
      />
    );
  };

  const renderFinish = () => (
    <div>
      <div className="flex flex-col items-center">
        <FontAwesomeIcon icon={faCheckSquare} className="text-blue-darker text-6xl" />
        <h1 className="text-3xl font-bold my-4">{test?.attributes.name}</h1>
        <span className="italic text-gray-600">{test?.attributes.description}</span>
      </div>
      <div className="w-full h-px border-navy-blue border-t" />
      <h4 className="font-bold my-4">We have received your answers for this test.</h4>
      <h4 className="font-bold my-4">
        Results will be emailed to you through the email you registered your account with.
      </h4>
      <h4 className="font-bold my-4">
        If you do not see the results for this test attempt after an hour please reach out to
        <a className="text-blue-darker hover:text-blue" href="mailto:tech@iqasport.org">
          {" "}
          tech@iqasport.org
        </a>
      </h4>
    </div>
  );

  const renderStart = () => (
    <div>
      <div className="flex flex-col items-center my-4">
        <FontAwesomeIcon icon={faEdit} className="text-blue-darker text-6xl" />
        <h1 className="text-3xl font-bold my-4">{test?.attributes.name}</h1>
        <span className="italic text-gray-600">{test?.attributes.description}</span>
      </div>
      <div className="w-full h-px border-t border-navy-blue" />
      <h4 className="font-bold my-4">{`You will have ${test?.attributes.timeLimit} minutes to complete this test.`}</h4>
      <h4 className="font-bold my-4">Once you begin you may not exit the test.</h4>
      <h4 className="font-bold my-4">If you go over the time limit you will not pass the test.</h4>
      <h4 className="font-bold my-4">
        If you need more time to complete this test due to documented test taking challenges please
        contact
        <a className="text-blue-darker hover:text-blue" href="mailto:referees@iqasport.org">
          {" "}
          referees@iqasport.org
        </a>
      </h4>
    </div>
  );

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
