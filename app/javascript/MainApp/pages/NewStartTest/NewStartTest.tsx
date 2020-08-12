import { faCheckSquare, faEdit } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { shuffle } from 'lodash'
import { DateTime } from 'luxon'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'

import { FinishTestRequest, RefereeAnswer } from 'MainApp/apis/job'
import NewTestTaker from 'MainApp/components/NewTestTaker'
import { finishTest } from 'MainApp/modules/job/job'
import { startTest } from 'MainApp/modules/question/questions'
import { getTest } from 'MainApp/modules/test/test'
import { RootState } from 'MainApp/rootReducer'
import { GetQuestionsSchemaDatum, Included } from 'MainApp/schemas/getQuestionsSchema'

export type FormattedQuestion = {
  questionId: string;
  description: string;
  answers: Included[];
  selectedAnswer: string;
}

const formatQuestions = (questions: GetQuestionsSchemaDatum[], answers: Included[]): FormattedQuestion[] => {
  return questions.map((question): FormattedQuestion => {
    const filteredAnswers = answers.filter((answer) => String(answer.attributes.questionId) === question.id)

    return {
      answers: shuffle(filteredAnswers),
      description: question.attributes.description,
      questionId: question.id,
      selectedAnswer: null,
    }
  })
}

const buildRefereeAnswers = (testQuestions: FormattedQuestion[]): RefereeAnswer[] => {
  return testQuestions.map((question: FormattedQuestion) => (
    {
      answer_id: question.selectedAnswer,
      question_id: question.questionId,
    }
  ))
}

enum TestAction {
  Prev = 'prev',
  Next = 'next',
  Start = 'start',
  Finish = 'finish'
}

type TestParams = {
  testId: string;
  refereeId: string;
}

const NewStartTest = (props: RouteComponentProps<TestParams>) => {
  const { match: { params: { refereeId, testId } } } = props

  const [testQuestions, setTestQuestions] = useState<FormattedQuestion[]>([])
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0)
  const [startedAt, setStartedAt] = useState<DateTime>(null)
  const [finishedAt, setFinishedAt] = useState<DateTime>(null)
  const history = useHistory()
  const dispatch = useDispatch()
  const { test, error: testError } = useSelector((state: RootState) => state.test, shallowEqual)
  const { questions, answers, error: questionsError } = useSelector((state: RootState) => state.questions, shallowEqual)

  useEffect(() => {
    if (!test) {
      dispatch(getTest(testId))
    }
  }, [testId, test])

  useEffect(() => {
    if (questions.length && answers.length) {
      setTestQuestions(formatQuestions(questions, answers))
      setStartedAt(DateTime.local())
    }
  }, [questions, answers])

  const isLastQuestion = currentQuestionIndex === testQuestions.length - 1
  const isFirstQuestion = currentQuestionIndex === 0

  const handleGoBack = () => history.push(`/referees/${refereeId}`)

  const handleStartTest = () => dispatch(startTest(testId))

  const handleNext = () => setCurrentQuestionIndex(currentQuestionIndex + 1)

  const handlePrev = () => setCurrentQuestionIndex(currentQuestionIndex - 1)

  const handleFinish = () => {
    const finishedTime = DateTime.local()
    const request: FinishTestRequest = {
      finishedAt: finishedTime,
      refereeAnswers: buildRefereeAnswers(testQuestions),
      startedAt,
    }

    dispatch(finishTest(testId, request))
    setFinishedAt(finishedTime)
    setTestQuestions([])
    setStartedAt(null)
  }

  const handleTestAction = (action: TestAction) => () => {
    switch (action) {
      case TestAction.Start:
        handleStartTest()
        break
      case TestAction.Next:
        handleNext()
        break
      case TestAction.Prev:
        handlePrev()
        break
      case TestAction.Finish:
        handleFinish()
        break
    }
  }

  const handleAnswerSelect = (answerId: string) => {
    const updatedQuestion = { ...testQuestions[currentQuestionIndex], selectedAnswer: answerId }
    testQuestions.splice(currentQuestionIndex, 1, updatedQuestion)
  }

  const renderButtons = () => {
    const nextContent = isLastQuestion ? 'Finish' : 'Next'
    const isNextDisabled = !testQuestions[currentQuestionIndex]?.selectedAnswer
    const nextAction = isLastQuestion ? TestAction.Finish : TestAction.Next

    return (
      <div className="flex w-1/2 justify-between mx-auto">
        {!startedAt && <button className="button-tab" onClick={handleGoBack}>Go Back To Profile</button>}
        {(!startedAt && !finishedAt) && (
          <button
            className="green-button-outline"
            onClick={handleTestAction(TestAction.Start)}
          >
            Start Test
          </button>
        )}
        {(startedAt && !isFirstQuestion) && (
          <button className="button-tab" onClick={handleTestAction(TestAction.Prev)}>Previous</button>
        )}
        {startedAt && (
          <button className="" disabled={isNextDisabled} onClick={handleTestAction(nextAction)}>{nextContent}</button>
        )}
      </div>
    )
  }

  const renderTest = () => {
    return (
      <NewTestTaker
        timeLimit={test?.attributes.timeLimit}
        currentQuestion={testQuestions[currentQuestionIndex]}
        onAnswerSelect={handleAnswerSelect}
        totalQuestionCount={testQuestions.length}
        currentIndex={currentQuestionIndex + 1}
        onTimeLimitMet={handleFinish}
      />
    )
  }

  const renderFinish = () => (
    <div>
      <div className="flex flex-col items-center">
        <FontAwesomeIcon icon={faCheckSquare} className="text-blue-darker text-6xl" />
        <h1 className="text-3xl font-bold my-4">{test?.attributes.name}</h1>
        <span className="italic text-gray-600">{test?.attributes.description}</span>
      </div>
      <div className="w-full h-px border border-navy-blue" />
      <h4 className="font-bold my-4">We have received your answers for this test.</h4>
      <h4 className="font-bold my-4">Results will be emailed to you through the email you registered your account with.</h4>
      <h4 className="font-bold my-4">
        If you do not see the results for this test attempt after an hour please reach out to
        <a href="mailto:tech@iqasport.org"> tech@iqasport.org</a>
      </h4>
    </div>
  )

  const renderStart = () => (
    <div>
      <div className="flex flex-col items-center my-4">
        <FontAwesomeIcon icon={faEdit} className="text-blue-darker text-6xl" />
        <h1 className="text-3xl font-bold my-4">{test?.attributes.name}</h1>
        <span className="italic text-gray-600">{test?.attributes.description}</span>
      </div>
      <div className="w-full h-px border border-navy-blue" />
      <h4 className="font-bold my-4">{`You will have ${test?.attributes.timeLimit} minutes to complete this test.`}</h4>
      <h4 className="font-bold my-4">Once you begin you may not exit the test.</h4>
      <h4 className="font-bold my-4">If you go over the time limit you will not pass the test.</h4>
      <h4 className="font-bold my-4">
        If you need more time to complete this test due to documented test taking challenges please contact
        <a href="mailto:referees@iqasport.org"> referees@iqasport.org</a>
      </h4>
    </div>
  )

  const renderMainContent = () => {
    if (finishedAt) return renderFinish()
    if (startedAt) return renderTest()

    return renderStart()
  }

  return (
    <div className="text-center flex flex-col justify-between my-8 mx-auto w-2/3" style={{ minHeight: '450px' }}>
      {renderMainContent()}
      {renderButtons()}
    </div>
  )
}

export default NewStartTest
