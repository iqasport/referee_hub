import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { shuffle, size } from 'lodash'
import { DateTime } from 'luxon'
import {
  Segment, Header, Message, Icon, Divider, Button
} from 'semantic-ui-react'

import TestTaker from './TestTaker'

const formatAnswer = ({ id, attributes }) => ({
  id,
  description: attributes.description
})

const formatQuestions = (questions, allAnswers) => (
  questions.reduce((accObj, question, index) => {
    const questionAnswers = allAnswers.filter(({ attributes }) => String(attributes.question_id) === question.id)
    const formattedAnswers = questionAnswers.map(formatAnswer)
    // eslint-disable-next-line no-param-reassign
    accObj[index] = {
      questionId: question.id,
      description: question.attributes.description,
      answers: shuffle(formattedAnswers),
      selectedAnswer: null
    }
    return accObj
  }, [])
)

const buildRefereeAnswers = testQuestions => (
  testQuestions.map(question => ({ question_id: question.questionId, answer_id: question.selectedAnswer }))
)

class StartTest extends Component {
  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.object
    }).isRequired,
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  state = {
    status: 0,
    statusText: '',
    name: '',
    description: '',
    timeLimit: 0,
    testStarted: false,
    testFinished: false,
    testQuestions: {},
    currentQuestionIndex: 0
  }

  componentDidMount() {
    const { match: { params } } = this.props

    axios.get(`/api/v1/tests/${params.testId}`)
      .then(({ data, status, statusText }) => {
        const { data: { attributes } } = data

        const testData = {
          name: attributes.name,
          description: attributes.description,
          timeLimit: attributes.time_limit
        }
        this.setState({ status, statusText, ...testData })
      })
  }

  get isLastQuestion() {
    const { currentQuestionIndex, testQuestions } = this.state

    return currentQuestionIndex === testQuestions.length - 1
  }

  get currentQuestion() {
    const { currentQuestionIndex, testQuestions } = this.state

    return testQuestions[currentQuestionIndex]
  }

  get isFirstQuestion() {
    const { currentQuestionIndex } = this.state

    return currentQuestionIndex === 0
  }

  handleQuestionChange = action => () => {
    switch (action) {
      case 'prev':
        this.setState(prevState => ({ currentQuestionIndex: prevState.currentQuestionIndex - 1 }))
        break
      case 'next':
        this.handleFetchNextQuestion()
        break
      default:
        this.handleFetchNextQuestion()
        break
    }
  }

  handleAnswerSelect = (answerId) => {
    const { currentQuestionIndex, testQuestions } = this.state

    const updatedQuestion = { ...this.currentQuestion, selectedAnswer: answerId }
    testQuestions.splice(currentQuestionIndex, 1, updatedQuestion)
    // setting state here to essentially cause a re-render
    this.setState({ testQuestions })
  }

  handleFetchNextQuestion = () => {
    const { currentQuestionIndex } = this.state
    let nextIndex = currentQuestionIndex + 1

    if (this.isLastQuestion) {
      this.handleFinishTest()
      nextIndex = 0
    }

    this.setState({ currentQuestionIndex: nextIndex })
  }

  handleGoBackToProfile = () => {
    const { match: { params }, history: { push } } = this.props

    push(`/referees/${params.refereeId}`)
  }

  handleFinishTest = () => {
    const { testQuestions, startedAt } = this.state
    const { match: { params } } = this.props
    const finishedAt = DateTime.local()
    const refereeAnswers = buildRefereeAnswers(testQuestions)

    const postParams = {
      started_at: startedAt,
      finished_at: finishedAt,
      referee_answers: refereeAnswers
    }

    axios.post(`/api/v1/tests/${params.testId}/finish`, postParams)
      .then(() => {
        this.setState({
          testQuestions: {},
          testFinished: true,
          testStarted: false,
          status: null,
          statusText: null
        })
      })
      .catch((error) => {
        const { status, statusText } = error.response || {
          status: 500,
          statusText: 'Error'
        }

        this.setState({ status, statusText })
      })
  }

  handleStartTest = () => {
    const { match: { params } } = this.props

    axios.get(`/api/v1/tests/${params.testId}/start`)
      .then(({ data }) => {
        const { data: questions, included } = data

        const testQuestions = formatQuestions(questions, included)
        const startedAt = DateTime.local()
        this.setState({ testQuestions, testStarted: true, startedAt })
      })
      .catch((error) => {
        const { status, data: { error: dataError } } = error.response || {
          status: 500,
          statusText: 'Error'
        }

        this.setState({ status, statusText: dataError, testQuestions: {} })
      })
  }

  renderButtons = () => {
    const { testStarted, testFinished } = this.state
    const nextContent = this.isLastQuestion ? 'Finish' : 'Next'

    return (
      <div>
        {!testStarted && <Button color="blue" onClick={this.handleGoBackToProfile} content="Go Back to Profile" />}
        {!testStarted && !testFinished && <Button color="green" onClick={this.handleStartTest} content="Start Test" />}
        {
          testStarted
          && !this.isFirstQuestion
          && <Button onClick={this.handleQuestionChange('prev')} content="Previous" />
        }
        {testStarted && <Button color="blue" onClick={this.handleQuestionChange('next')} content={nextContent} />}
      </div>
    )
  }

  renderTestStartContent = () => {
    const { name, description, timeLimit } = this.state
    return (
      <Fragment>
        <Header as="h1" icon>
          <Icon name="edit" color="blue" />
          {name}
          <Header.Subheader>{description}</Header.Subheader>
        </Header>
        <Divider />
        <Header as="h4">{`You will have ${timeLimit} minutes to complete this test.`}</Header>
        <Header as="h4">Once you begin you may not exit the test.</Header>
        <Header as="h4">
          If you need more time to complete this test due to documented test taking challenges please contact
          <a href="mailto:referees@iqasport.org"> referees@iqasport.org</a>
        </Header>
      </Fragment>
    )
  }

  renderTestFinishContent = () => {
    const { name, description } = this.state

    return (
      <Fragment>
        <Header as="h1" icon>
          <Icon name="checkmark box" color="green" />
          {name}
          <Header.Subheader>{description}</Header.Subheader>
        </Header>
        <Divider />
        <Header as="h4">
          We have successfully accepted your answers for this test.
        </Header>
        <Header as="h4">Results will be emailed to you through the email you registered your account with.</Header>
        <Header as="h4">
          If you do not see the results for this test attempt after an hour please reach out to
          <a href="mailto:tech@iqasport.org"> tech@iqasport.org</a>
        </Header>
      </Fragment>
    )
  }

  renderTest = () => {
    const { testQuestions, currentQuestionIndex, timeLimit } = this.state

    return (
      <TestTaker
        timeLimit={timeLimit}
        currentQuestion={testQuestions[currentQuestionIndex]}
        onAnswerSelect={this.handleAnswerSelect}
        totalQuestionCount={size(testQuestions)}
        currentQuestionIndex={currentQuestionIndex + 1}
      />
    )
  }

  renderMainContent = () => {
    const { testStarted, testFinished } = this.state

    if (testFinished) return this.renderTestFinishContent()
    if (testStarted) return this.renderTest()

    return this.renderTestStartContent()
  }

  render() {
    const { status, statusText, testStarted } = this.state
    const isError = status >= 400

    return (
      <Segment
        textAlign="center"
        style={{
          minHeight: '450px',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between'
        }}
      >
        {isError && !testStarted && <Message error header={status} content={statusText} />}
        {this.renderMainContent()}
        {this.renderButtons()}
      </Segment>
    )
  }
}

export default StartTest
