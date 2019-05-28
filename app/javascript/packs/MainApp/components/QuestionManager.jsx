/* eslint-disable react/no-danger */
import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import {
  Message, Button
} from 'semantic-ui-react'
import Question from './Question'

const sanitizeQuestionData = (test) => {
  const { id, attributes } = test
  const {
    // eslint-disable-next-line camelcase
    description, feedback, points_available
  } = attributes

  return {
    description,
    feedback,
    pointsAvailable: points_available,
    id
  }
}

const emptyQuestion = {
  description: '',
  feedback: '',
  pointsAvailable: 0,
  id: null,
}

class QuestionManager extends Component {
  state = {
    questions: [],
    httpStatus: null,
    httpStatusText: null,
  }

  componentDidMount() {
    this.fetchQuestions()
  }

  fetchQuestions = () => {
    const { testId } = this.props

    axios.get(`/api/v1/tests/${testId}/questions`)
      .then(({ data }) => {
        const { data: questionData } = data

        const sanitzedData = questionData.map(sanitizeQuestionData)
        this.setState({ questions: sanitzedData })
      })
      .catch(this.setDataFromError)
  }

  setDataFromError = (error) => {
    const { status, statusText } = error.response || {
      status: 500,
      statusText: 'Error'
    }

    this.setState({ httpStatus: status, httpStatusText: statusText })
  }

  createQuestion = ({ description, feedback, pointsAvailable }) => {
    const { testId } = this.props

    axios.post(`/api/v1/tests/${testId}/questions`, {
      description,
      feedback,
      points_available: pointsAvailable
    })
      .then(this.fetchQuestions)
      .catch(this.setDataFromError)
  }

  updateQuestion = (questionValues) => {
    const {
      description, feedback, pointsAvailable, id
    } = questionValues

    axios.put(`/api/v1/questions/${id}`, {
      description,
      feedback,
      points_available: pointsAvailable
    })
      .then(this.fetchQuestions)
      .catch(this.setDataFromError)
  }

  handleAddNewQuestion = () => {
    const { questions } = this.state

    this.setState({ questions: [...questions, emptyQuestion] })
  }

  handleSaveQuestion = (questionValues) => {
    const hasId = !!questionValues.id

    if (hasId) {
      this.updateQuestion(questionValues)
    } else {
      this.createQuestion(questionValues)
    }
  }

  handleDeleteQuestion = (questionId) => {
    if (!questionId) throw new Error('question id cannot be empty')

    axios.delete(`/api/v1/questions/${questionId}`)
      .then(this.fetchQuestions)
      .catch(this.setDataFromError)
  }

  renderQuestion = (question, index) => (
    <Question
      key={`${String(question.id)}-${index}`}
      onSave={this.handleSaveQuestion}
      values={question}
      onDelete={this.handleDeleteQuestion}
    />
  )

  renderAddNew = () => {
    const { questions } = this.state
    const isQuestionsEmpty = questions.length < 1
    const emptyQuestions = (
      <p>
        Looks like this test has no questions, add some by clicking on the button below
      </p>
    )

    return (
      <div>
        {isQuestionsEmpty && emptyQuestions}
        <Button onClick={this.handleAddNewQuestion} content="Add Question" primary />
      </div>
    )
  }

  render() {
    const { questions, httpStatus, httpStatusText } = this.state
    const isError = httpStatus >= 400

    return (
      <Fragment>
        {isError && <Message content={httpStatusText} error />}
        {questions.map(this.renderQuestion)}
        {this.renderAddNew()}
      </Fragment>
    )
  }
}

QuestionManager.propTypes = {
  testId: PropTypes.string.isRequired
}

export default QuestionManager
