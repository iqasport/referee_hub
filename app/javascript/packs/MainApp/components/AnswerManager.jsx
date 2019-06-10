import React, { Component } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { Button, Message } from 'semantic-ui-react'
import { remove } from 'lodash'
import Answer from './Answer'

const emptyAnswer = index => (
  {
    id: `${String(null)}-${index}`,
    description: '',
    correct: false
  }
)

const sanitizeAnswer = (answer, index) => {
  const { id, attributes: { description, correct } } = answer

  return {
    id: `${String(id)}-${index}`,
    description,
    correct
  }
}

const findAnswer = id => answer => answer.id === id

class AnswerManager extends Component {
  state = {
    answers: [],
    httpStatus: null,
    httpStatusText: null,
    correctId: null,
  }

  componentDidMount() {
    const { answers } = this.state

    if (answers.length < 1) {
      this.fetchAnswers()
    }
  }

  fetchAnswers = () => {
    const { questionId } = this.props
    axios.get(`/api/v1/questions/${questionId}/answers`)
      .then(this.setDataFromGetResponse)
      .catch(this.setDataFromError)
  }

  setDataFromGetResponse = ({ data }) => {
    const { data: answerData } = data

    const sanitizedData = answerData.map(sanitizeAnswer)
    const correctAnswer = sanitizedData.find(({ correct }) => !!correct)
    const correctId = correctAnswer ? correctAnswer.id : null

    this.setState({ answers: sanitizedData, correctId })
  }

  setDataFromError = (error) => {
    if (!error.response) return
    const { status, statusText } = error.response

    this.setState({ httpStatus: status, httpStatusText: statusText })
  }

  setUpdatedAnswer = id => ({ data }) => {
    const { answers, correctId } = this.state
    const { data: answerData } = data

    // find where the updated answer was in the answer list
    let foundIndex = -1
    answers.forEach((answer, index) => {
      if (answer.id === id) {
        foundIndex = index
      }
    })

    // remove the old data from the old state
    remove(answers, findAnswer(id))

    // sanitize the incoming data, then insert the updated data at the original index
    const updatedAnswer = sanitizeAnswer(answerData, foundIndex)
    answers.splice(foundIndex, 0, updatedAnswer)

    // if the question was created, instead of updated make sure the correctId reflects the new database id
    const newCorrectId = correctId === id && /null/.test(id) ? updatedAnswer.id : correctId

    this.setState({ answers, correctId: newCorrectId })
  }

  handleAddNewAnswer = () => {
    const { answers } = this.state

    this.setState({ answers: [...answers, emptyAnswer(answers.length)] })
  }

  handleCorrectAnswerChange = correctId => this.setState({ correctId })

  handleAnswerSave = ({ id, description }) => {
    const { correctId } = this.state
    const { questionId } = this.props
    const updatedData = {
      description,
      correct: correctId === id
    }
    const isIdNull = /null/.test(id)

    if (isIdNull) {
      axios.post(`/api/v1/questions/${questionId}/answers`, updatedData)
        .then(this.setUpdatedAnswer(id))
        .catch(this.setDataFromError)
    } else {
      const parsedId = id.split('-')[0]
      axios.put(`/api/v1/answers/${parsedId}`, updatedData)
        .then(this.setUpdatedAnswer(id))
        .catch(this.setDataFromError)
    }
  }

  handleAnswerDelete = (id) => {
    const { answers, correctId } = this.state
    const parsedId = id.split('-')[0]

    axios.delete(`/api/v1/answers/${parsedId}`)
      .then(() => {
        const newCorrectId = correctId === id ? null : correctId
        remove(answers, findAnswer(id))

        this.setState({ answers, correctId: newCorrectId })
      })
      .catch(this.setDataFromError)
  }

  renderAddNew = () => {
    const { answers } = this.state
    const isAnswersEmpty = answers.length < 1
    const emptyAnswers = (
      <p>
        Looks like this question has no answers, add some by clicking on the button below
      </p>
    )

    return (
      <div>
        {isAnswersEmpty && emptyAnswers}
        <Button onClick={this.handleAddNewAnswer} content="Add Answer" primary />
      </div>
    )
  }

  renderAnswer = (answer, index) => {
    const { correctId } = this.state

    return (
      <Answer
        key={`${String(answer.id)}-${index}`}
        values={answer}
        isCorrect={correctId === answer.id}
        onSave={this.handleAnswerSave}
        onCorrectChange={this.handleCorrectAnswerChange}
        onDelete={this.handleAnswerDelete}
      />
    )
  }

  render() {
    const { answers, httpStatus, httpStatusText } = this.state
    const isError = httpStatus && httpStatus >= 400

    return (
      <div>
        {isError && <Message content={httpStatusText} error />}
        {answers.map(this.renderAnswer)}
        {this.renderAddNew()}
      </div>
    )
  }
}

AnswerManager.propTypes = {
  questionId: PropTypes.string.isRequired
}

export default AnswerManager
